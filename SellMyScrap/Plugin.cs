﻿using BepInEx;
using BepInEx.Logging;
using com.github.zehsteam.SellMyScrap.Commands;
using com.github.zehsteam.SellMyScrap.Patches;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

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
        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(DepositItemsDeskPatch));

        ConfigManager = new SyncedConfig();

        CommandManager.Initialize();
        ConfigHelper.Initialize();

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

        CommandManager.OnLocalDisconnect();
        CancelSellRequest();
    }

    public void OnTerminalQuit()
    {
        CommandManager.OnTerminalQuit();
        CancelSellRequest();
    }

    public void DisplayGlobalNotification(string displayText)
    {
        HUDManager.Instance.globalNotificationAnimator.SetTrigger("TriggerNotif");
        HUDManager.Instance.globalNotificationText.text = displayText;
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
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
        if (!ConfigManager.SellScrapWorthZero && item.scrapValue <= 0) return false;

        string itemName = item.itemProperties.itemName;

        if (itemName == "Gift" && !Instance.ConfigManager.SellGifts) return false;
        if (itemName == "Shotgun" && !Instance.ConfigManager.SellShotguns) return false;
        if (itemName == "Ammo" && !Instance.ConfigManager.SellAmmo) return false;
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
    public void CreateSellRequest(SellType sellType, int value, int requestedValue, ConfirmationType confirmationType)
    {
        sellRequest = new SellRequest(sellType, value, requestedValue, confirmationType);

        mls.LogInfo($"Created sell request. {scrapToSell.scrap.Count} items for ${value}.");
    }

    public void ConfirmSellRequest()
    {
        if (scrapToSell == null) return;
        if (sellRequest == null) return;

        sellRequest.confirmationType = ConfirmationType.Confirmed;

        mls.LogInfo($"Attempting to sell {scrapToSell.scrap.Count} items for ${scrapToSell.value}.");

        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

        if (isHostOrServer)
        {
            ConfirmSellRequestOnServer();
        }
        else
        {
            ConfirmSellRequestOnClient();
        }

        sellRequest = null;
        scrapToSell = null;
    }

    private void ConfirmSellRequestOnServer()
    {
        StartOfRound.Instance.StartCoroutine(PerformSellOnServer());
    }

    private void ConfirmSellRequestOnClient()
    {
        int fromPlayerId = (int)StartOfRound.Instance.localPlayerController.playerClientId;
        string networkObjectIdsString = NetworkUtils.GetNetworkObjectIdsString(scrapToSell.scrap);

        PluginNetworkBehaviour.Instance.PerformSellServerRpc(fromPlayerId, networkObjectIdsString, sellRequest.sellType, sellRequest.value, scrapToSell.scrap.Count);
    }

    public void CancelSellRequest()
    {
        sellRequest = null;
        scrapToSell = null;
    }
    #endregion

    public void PerformSellOnServerFromClient(List<GrabbableObject> scrap, SellType sellType)
    {
        scrapToSell = new ScrapToSell(scrap);
        CreateSellRequest(sellType, scrapToSell.value, scrapToSell.value, ConfirmationType.AwaitingConfirmation);
        ConfirmSellRequest();
    }

    public IEnumerator PerformSellOnServer()
    {
        if (scrapToSell == null) yield return null;
        if (sellRequest == null) yield return null;
        if (sellRequest.confirmationType != ConfirmationType.Confirmed) yield return null;

        if (DepositItemsDeskPatch.DepositItemsDesk == null)
        {
            mls.LogError($"Error: could not find depositItemsDesk. Are you landed at The Company building?");
            yield return null;
        }

        DepositItemsDeskPatch.PlaceItemsOnCounter(scrapToSell.scrap);
        PluginNetworkBehaviour.Instance.PlaceItemsOnCounterClientRpc(NetworkUtils.GetNetworkObjectIdsString(scrapToSell.scrap));
        PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();

        yield return new WaitForSeconds(0.2f);

        DepositItemsDeskPatch.DepositItemsDesk.SellItemsOnServer();
    }
}
