using BepInEx;
using BepInEx.Logging;
using com.github.zehsteam.SellMyScrap.Commands;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance;
    internal static ManualLogSource logger;

    internal static SyncedConfigManager ConfigManager;

    public static bool IsHostOrServer => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

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

        ModpackSaveSystem.Initialize();
        
        ConfigManager = new SyncedConfigManager();

        Content.Load();
        CommandManager.Initialize();
        ConfigHelper.Initialize();
        ScrapEaterManager.Initialize();

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

    public ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true, bool withOvertimeBonus = false)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(value, onlyAllowedScrap, withOvertimeBonus);
        return ScrapToSell;
    }

    public ScrapToSell GetScrapToSell(string[] sellListJson)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(sellListJson);
        return ScrapToSell;
    }

    public ScrapToSell SetScrapToSell(List<GrabbableObject> scrap)
    {
        ScrapToSell = new ScrapToSell(scrap);
        return ScrapToSell;
    }

    #region SellRequest Methods
    public void CreateSellRequest(SellType sellType, int value, int requestedValue, ConfirmationType confirmationType, int scrapEaterIndex = -1)
    {
        SellRequest = new SellRequest(sellType, value, requestedValue, confirmationType, scrapEaterIndex);

        string message = $"Created sell request. {ScrapToSell.Amount} items for ${value}.";

        if (scrapEaterIndex >= 0)
        {
            message += $" (scrapEaterIndex: {scrapEaterIndex})";
        }

        logger.LogInfo(message);
    }

    public void ConfirmSellRequest()
    {
        if (ScrapToSell == null || SellRequest == null) return;

        SellRequest.ConfirmationType = ConfirmationType.Confirmed;

        logger.LogInfo($"Attempting to sell {ScrapToSell.Amount} items for ${ScrapToSell.Value}.");

        if (IsHostOrServer)
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
        int fromPlayerId = (int)StartOfRound.Instance.localPlayerController.playerClientId;
        string networkObjectIdsString = NetworkUtils.GetNetworkObjectIdsString(ScrapToSell.Scrap);

        PluginNetworkBehaviour.Instance.PerformSellServerRpc(fromPlayerId, networkObjectIdsString, SellRequest.SellType, SellRequest.RealValue, ScrapToSell.Amount, SellRequest.ScrapEaterIndex);
    }

    public void CancelSellRequest()
    {
        SellRequest = null;
        ScrapToSell = null;
    }
    #endregion

    public void PerformSellOnServerFromClient(List<GrabbableObject> scrap, SellType sellType, int scrapEaterIndex = -1)
    {
        ScrapToSell = new ScrapToSell(scrap);
        CreateSellRequest(sellType, ScrapToSell.Value, ScrapToSell.Value, ConfirmationType.AwaitingConfirmation, scrapEaterIndex);
        ConfirmSellRequest();
    }

    public IEnumerator PerformSellOnServer()
    {
        if (ScrapToSell == null || SellRequest == null) yield return null;
        if (SellRequest.ConfirmationType != ConfirmationType.Confirmed) yield return null;

        if (DepositItemsDeskPatch.Instance == null)
        {
            logger.LogError($"Error: could not find depositItemsDesk. Are you landed at The Company building?");
            yield break;
        }

        int scrapEaterIndex = SellRequest.ScrapEaterIndex;

        if (scrapEaterIndex == 0)
        {
            ScrapEaterManager.StartRandomScrapEaterOnServer(ScrapToSell.Scrap);
            yield break;
        }

        if (scrapEaterIndex > 0 && ScrapEaterManager.HasScrapEater(scrapEaterIndex - 1))
        {
            ScrapEaterManager.StartScrapEaterOnServer(scrapEaterIndex - 1, ScrapToSell.Scrap);
            yield break;
        }

        if (ScrapEaterManager.CanUseScrapEater())
        {
            ScrapEaterManager.StartRandomScrapEaterOnServer(ScrapToSell.Scrap);
            yield break;
        }

        DepositItemsDeskPatch.PlaceItemsOnCounter(ScrapToSell.Scrap);
        PluginNetworkBehaviour.Instance.PlaceItemsOnCounterClientRpc(NetworkUtils.GetNetworkObjectIdsString(ScrapToSell.Scrap));
        yield return new WaitForSeconds(0.5f);
        DepositItemsDeskPatch.SellItemsOnServer();

        ScrapToSell = null;
    }
}
