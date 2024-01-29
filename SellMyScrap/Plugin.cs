using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using Zehs.SellMyScrap.Patches;

namespace Zehs.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SellMyScrapBase : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static SellMyScrapBase Instance;
    internal static ManualLogSource mls;
    internal SyncedConfig ConfigManager;

    public SellRequest sellRequest;
    public ScrapToSell scrapToSell;
    private string[] dontSellList;

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
        ConfigManager.RebindConfigs(new SyncedConfigData());
        dontSellList = Instance.ConfigManager.DontSellListJson;

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

    public ScrapToSell GetScrapToSell(int amount, GameObject ship)
    {
        // Get all scrap
        if (amount == -1)
        {
            scrapToSell = new ScrapToSell(GetScrapFromShip(ship));
            return scrapToSell;
        }

        // Get scrap based on amount
        scrapToSell = ScrapCalculator.GetScrapToSell(GetScrapFromShip(ship), amount, StartOfRound.Instance.companyBuyingRate);
        return scrapToSell;
    }

    public void RequestSell(int amount, GameObject ship, DepositItemsDesk depositItemsDesk)
    {
        if (scrapToSell == null|| scrapToSell.value != amount)
        {
            GetScrapToSell(amount, ship);
        }

        PerformSell(scrapToSell.scrap, depositItemsDesk);

        scrapToSell = null;
    }

    public void RequestSellAll(GameObject ship, DepositItemsDesk depositItemsDesk)
    {
        PerformSell(GetScrapFromShip(ship), depositItemsDesk);
    }

    public List<GrabbableObject> GetScrapFromShip(GameObject ship)
    {
        GrabbableObject[] itemsInShip = ship.GetComponentsInChildren<GrabbableObject>();
        List<GrabbableObject> scrap = new List<GrabbableObject>();

        foreach (var item in itemsInShip)
        {
            if (!IsItemAllowedScrap(item)) continue;
            scrap.Add(item);
        }

        return scrap;
    }

    public bool IsItemAllowedScrap(GrabbableObject item)
    {
        if (!item.itemProperties.isScrap) return false;
        if (item.isPocketed) return false;
        if (item.isHeld) return false;

        string itemName = item.itemProperties.itemName;
        if (itemName == "Gift" && !Instance.ConfigManager.SellGifts) return false;
        if (itemName == "Shotgun" && !Instance.ConfigManager.SellShotguns) return false;
        if (itemName == "Ammo" && !Instance.ConfigManager.SellAmmo) return false;
        if (itemName == "Homemade flashbang" && !Instance.ConfigManager.SellHomemadeFlashbang) return false;
        if (itemName == "Jar of pickles" && !Instance.ConfigManager.SellPickles) return false;

        // Dont sell list
        foreach (var dontSellItem in dontSellList)
        {
            if (itemName.ToLower() == dontSellItem.ToLower()) return false;
        }

        // Add item to sell pool
        return true;
    }

    public void PerformSell(List<GrabbableObject> scrap, DepositItemsDesk depositItemsDesk)
    {
        scrap.ForEach(item =>
        {
            item.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
            
            depositItemsDesk.AddObjectToDeskServerRpc(item.gameObject.GetComponent<NetworkObject>());
        });

        depositItemsDesk.SellItemsOnServer();
    }
}
