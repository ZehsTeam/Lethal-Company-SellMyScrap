using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SellMyScrapBase : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static SellMyScrapBase Instance;
    internal static ManualLogSource mls;
    internal SyncedConfig ConfigManager;

    public SellRequest sellRequest;
    public ScrapToSell scrapToSell;

    private string[] cachedDontSellList;

    void Awake()
    {
        if (Instance == null) Instance = this;

        mls = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        mls.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        harmony.PatchAll(typeof(NetworkObjectManagerPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(DepositItemsDeskPatch));

        ConfigManager = new SyncedConfig();

        NetcodePatcherAwake();
    }

    private void NetcodePatcherAwake()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    public void OnLocalDisconnect()
    {
        mls.LogInfo($"Local player disconnected. Removing hostConfigData.");
        ConfigManager.SetHostConfigData(null);

        CancelSellRequest();
    }

    public void OnTerminalQuit()
    {
        CancelSellRequest();
    }

    public void UpdateCachedDontSellList(string[] dontSellList)
    {
        this.cachedDontSellList = dontSellList;
    }

    #region Get ScrapToSell
    public ScrapToSell GetAllowedScrapToSell(int amount)
    {
        scrapToSell = ScrapCalculator.GetScrapToSell(GetAllowedScrapFromShip(), amount, StartOfRound.Instance.companyBuyingRate);

        return scrapToSell;
    }

    public ScrapToSell GetAllAllowedScrapToSell()
    {
        scrapToSell = new ScrapToSell(GetAllowedScrapFromShip());

        return scrapToSell;
    }

    public ScrapToSell GetScrapToSell(int amount)
    {
        scrapToSell = ScrapCalculator.GetScrapToSell(GetScrapFromShip(), amount, StartOfRound.Instance.companyBuyingRate);

        return scrapToSell;
    }

    public ScrapToSell GetAllScrapToSell()
    {
        scrapToSell = new ScrapToSell(GetScrapFromShip());

        return scrapToSell;
    }
    #endregion

    #region Getting Scrap
    public List<GrabbableObject> GetScrapFromShip()
    {
        GameObject ship = GameObject.Find("/Environment/HangarShip");
        GrabbableObject[] itemsInShip = ship.GetComponentsInChildren<GrabbableObject>();
        List<GrabbableObject> scrap = new List<GrabbableObject>();

        foreach (var item in itemsInShip)
        {
            if (!IsScrapItem(item)) continue;
            scrap.Add(item);
        }

        return scrap;
    }

    public List<GrabbableObject> GetAllowedScrapFromShip()
    {
        GameObject ship = GameObject.Find("/Environment/HangarShip");
        GrabbableObject[] itemsInShip = ship.GetComponentsInChildren<GrabbableObject>();
        List<GrabbableObject> scrap = new List<GrabbableObject>();

        foreach (var item in itemsInShip)
        {
            if (!IsAllowedScrapItem(item)) continue;
            scrap.Add(item);
        }

        return scrap;
    }

    public bool IsScrapItem(GrabbableObject item)
    {
        if (!item.itemProperties.isScrap) return false;
        if (item.isPocketed) return false;
        if (item.isHeld) return false;

        return true;
    }

    public bool IsAllowedScrapItem(GrabbableObject item)
    {
        if (!IsScrapItem(item)) return false;

        string itemName = item.itemProperties.itemName;

        if (itemName == "Gift" && !Instance.ConfigManager.SellGifts) return false;
        if (itemName == "Shotgun" && !Instance.ConfigManager.SellShotguns) return false;
        if (itemName == "Ammo" && !Instance.ConfigManager.SellAmmo) return false;
        if (itemName == "Homemade flashbang" && !Instance.ConfigManager.SellHomemadeFlashbang) return false;
        if (itemName == "Jar of pickles" && !Instance.ConfigManager.SellPickles) return false;

        // Dont sell list
        foreach (var dontSellItem in cachedDontSellList)
        {
            if (itemName.ToLower() == dontSellItem.ToLower()) return false;
        }

        return true;
    }
    #endregion

    #region SellRequest Methods
    public void CreateSellRequest(SellType sellType, int amountFound, int requestedAmount, ConfirmationType confirmationType)
    {
        sellRequest = new SellRequest(sellType, amountFound, requestedAmount, confirmationType);
    }

    public void ConfirmSellRequest()
    {
        if (sellRequest == null) return;

        sellRequest.confirmationType = ConfirmationType.Confirmed;

        if (NetworkManager.Singleton.IsHost)
        {
            PerformSell();
        }
        else
        {
            MainNetworkBehaviour.Instance.RequestSellServerRpc(sellRequest.amountFound);
        }

        sellRequest = null;
        scrapToSell = null;
    }

    public void CancelSellRequest()
    {
        sellRequest = null;
        scrapToSell = null;
    }
    #endregion

    public void PerformSell()
    {
        DepositItemsDesk depositItemsDesk = UnityEngine.Object.FindAnyObjectByType<DepositItemsDesk>();

        if (depositItemsDesk == null)
        {
            mls.LogError($"ERROR! Could not find depositItemsDesk. Are you landed at The Company building?");
            return;
        }

        // Has valid sell request?
        if (sellRequest == null) return;
        if (sellRequest.confirmationType != ConfirmationType.Confirmed) return;

        // Has scrap to sell?
        if (scrapToSell == null) return;

        scrapToSell.scrap.ForEach(item =>
        {
            item.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
            
            depositItemsDesk.AddObjectToDeskServerRpc(item.gameObject.GetComponent<NetworkObject>());
        });

        depositItemsDesk.SellItemsOnServer();
    }
}
