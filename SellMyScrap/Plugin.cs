using BepInEx;
using BepInEx.Logging;
using com.github.zehsteam.SellMyScrap.Commands;
using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Dependencies;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ShipInventoryProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance;
    internal static ManualLogSource logger;

    internal static SyncedConfigManager ConfigManager;

    public ScrapToSell ScrapToSell { get; private set; }
    public SellRequest SellRequest { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;

        logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        harmony.PatchAll(typeof(GameNetworkManagerPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(TimeOfDayPatch));
        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(DepositItemsDeskPatch));
        harmony.PatchAll(typeof(StartMatchLeverPatch));
        harmony.PatchAll(typeof(InteractTriggerPatch));
        
        if (ShipInventoryProxy.Enabled)
        {
            ShipInventoryProxy.PatchAll(harmony);
        }

        ConfigManager = new SyncedConfigManager();

        Content.Load();
        ModpackSaveSystem.Initialize();

        CommandManager.Initialize();
        ConfigHelper.Initialize();
        ScrapEaterManager.Initialize();

        ConfigHelper.SetModIcon(Content.ModIcon);
        ConfigHelper.SetModDescription("Adds a few terminal commands to sell your scrap from the ship. Highly Configurable. SellFromTerminal +");

        NetcodePatcherAwake();
    }

    private void NetcodePatcherAwake()
    {
        try
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    try
                    {
                        // Safely attempt to retrieve custom attributes
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                        if (attributes.Length > 0)
                        {
                            try
                            {
                                // Safely attempt to invoke the method
                                method.Invoke(null, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                Debug.LogWarning($"Failed to invoke method {method.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle errors when fetching custom attributes, due to missing types or dependencies
                        Debug.LogWarning($"Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catch any general exceptions that occur in the process
            Debug.LogError($"An error occurred in NetcodePatcherAwake: {ex.Message}");
        }
    }

    public void OnLocalDisconnect()
    {
        logger.LogInfo($"Local player disconnected. Removing hostConfigData.");
        ConfigManager.SetHostConfigData(null);

        CommandManager.OnLocalDisconnect();
        CancelSellRequest();
    }

    public void OnTerminalQuit()
    {
        CommandManager.OnTerminalQuit();
        CancelSellRequest();
    }

    public ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true, bool withOvertimeBonus = false, bool onlyUseShipInventory = false)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(value, onlyAllowedScrap, withOvertimeBonus, onlyUseShipInventory);
        return ScrapToSell;
    }

    public ScrapToSell GetScrapToSell(string[] sellList, bool onlyUseShipInventory = false)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(sellList);
        return ScrapToSell;
    }

    public ScrapToSell SetScrapToSell(List<ItemData> items)
    {
        ScrapToSell = new ScrapToSell(items);
        return ScrapToSell;
    }

    #region SellRequest Methods
    public void CreateSellRequest(SellType sellType, int value, int requestedValue, ConfirmationStatus confirmationType, int scrapEaterIndex = -2, int scrapEaterVariantIndex = -1)
    {
        SellRequest = new SellRequest(sellType, value, requestedValue, confirmationType, scrapEaterIndex, scrapEaterVariantIndex);

        string message = $"Created sell request. {ScrapToSell.ItemCount} items for ${value}.";

        if (scrapEaterIndex >= 0)
        {
            message += $" (ScrapEaterIndex: {scrapEaterIndex}, ScrapEaterVariantIndex: {scrapEaterVariantIndex})";
        }

        logger.LogInfo(message);
    }

    public void ConfirmSellRequest()
    {
        if (ScrapToSell == null || SellRequest == null) return;

        SellRequest.ConfirmationStatus = ConfirmationStatus.Confirmed;

        logger.LogInfo($"Attempting to sell {ScrapToSell.ItemCount} items for ${ScrapToSell.TotalScrapValue}.");

        if (NetworkUtils.IsServer)
        {
            ConfirmSellRequestOnServer();
        }
        else
        {
            ConfirmSellRequestOnClient();
        }

        SellRequest = null;
    }

    private void ConfirmSellRequestOnServer()
    {
        StartOfRound.Instance.StartCoroutine(PerformSellOnServer());
    }

    private void ConfirmSellRequestOnClient()
    {
        PluginNetworkBehaviour.Instance.PerformSellServerRpc(ScrapToSell, SellRequest.SellType, SellRequest.ScrapEaterIndex);
    }

    public void CancelSellRequest()
    {
        SellRequest = null;
        ScrapToSell = null;
    }
    #endregion

    public void PerformSellOnServerFromClient(ScrapToSell scrapToSell, SellType sellType, int scrapEaterIndex = -2, int scrapEaterVariantIndex = -1)
    {
        ScrapToSell = scrapToSell;
        CreateSellRequest(sellType, ScrapToSell.TotalScrapValue, ScrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, scrapEaterIndex, scrapEaterVariantIndex);
        ConfirmSellRequest();
    }

    public IEnumerator PerformSellOnServer()
    {
        if (ScrapToSell == null || SellRequest == null) yield return null;
        if (SellRequest.ConfirmationStatus != ConfirmationStatus.Confirmed) yield return null;

        if (DepositItemsDeskPatch.Instance == null)
        {
            logger.LogError($"Could not find depositItemsDesk. Are you landed at The Company building?");
            yield break;
        }

        int scrapEaterIndex = SellRequest.ScrapEaterIndex;
        int scrapEaterVariantIndex = SellRequest.ScrapEaterVariantIndex;

        List<GrabbableObject> grabbableObjects = ScrapToSell.GrabbableObjects;

        if (ShipInventoryProxy.Enabled && ScrapToSell.ShipInventoryItems.Length > 0)
        {
            ShipInventoryProxy.SpawnItemsOnServer(ScrapToSell.ShipInventoryItems);

            yield return new WaitUntil(() => !ShipInventoryProxy.IsSpawning);

            if (ShipInventoryProxy.SpawnItemsStatus == SpawnItemsStatus.Success)
            {
                grabbableObjects.AddRange(ShipInventoryProxy.GetSpawnedGrabbableObjects());
                ShipInventoryProxy.ClearSpawnedGrabbableObjects();
            }
            else if (ShipInventoryProxy.SpawnItemsStatus == SpawnItemsStatus.Failed)
            {
                HUDManager.Instance.DisplayTip("SellMyScrap", "Failed to spawn items from ShipInventory!", isWarning: true);
                yield break;
            }
        }

        // Try to show a scrap eater if the ship is not leaving.
        if (!StartOfRound.Instance.shipIsLeaving)
        {
            if (scrapEaterIndex == -1)
            {
                ScrapEaterManager.StartRandomScrapEaterOnServer(grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }

            if (scrapEaterIndex > -1 && ScrapEaterManager.HasScrapEater(scrapEaterIndex))
            {
                ScrapEaterManager.StartScrapEaterOnServer(scrapEaterIndex, grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }

            if (ScrapEaterManager.CanUseScrapEater())
            {
                ScrapEaterManager.StartRandomScrapEaterOnServer(grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }
        }

        DepositItemsDeskPatch.PlaceItemsOnCounter(grabbableObjects);
        PluginNetworkBehaviour.Instance.PlaceItemsOnCounterClientRpc(NetworkUtils.GetNetworkObjectReferences(grabbableObjects));
        yield return new WaitForSeconds(0.5f);
        DepositItemsDeskPatch.SellItemsOnServer();

        ScrapToSell = null;
    }

    public void LogInfoExtended(object data)
    {
        if (ConfigManager.ExtendedLogging)
        {
            logger.LogInfo(data);
        }
    }

    public void LogMessageExtended(object data)
    {
        if (ConfigManager.ExtendedLogging)
        {
            logger.LogMessage(data);
        }
    }
}
