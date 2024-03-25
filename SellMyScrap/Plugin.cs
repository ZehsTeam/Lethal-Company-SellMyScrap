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
public class SellMyScrapBase : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static SellMyScrapBase Instance;
    internal static ManualLogSource mls;

    internal SyncedConfig ConfigManager;

    public static bool IsHostOrServer => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

    public ScrapToSell scrapToSell;
    public SellRequest sellRequest;

    void Awake()
    {
        if (Instance == null) Instance = this;

        mls = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        mls.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        harmony.PatchAll(typeof(GameNetworkManagerPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(TimeOfDayPatch));
        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(DepositItemsDeskPatch));

        ConfigManager = new SyncedConfig();

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

    public ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true, bool withOvertimeBonus = false)
    {
        scrapToSell = ScrapHelper.GetScrapToSell(value, onlyAllowedScrap, withOvertimeBonus);
        return scrapToSell;
    }

    public ScrapToSell SetScrapToSell(List<GrabbableObject> scrap)
    {
        scrapToSell = new ScrapToSell(scrap);
        return scrapToSell;
    }

    #region SellRequest Methods
    public void CreateSellRequest(SellType sellType, int value, int requestedValue, ConfirmationType confirmationType)
    {
        sellRequest = new SellRequest(sellType, value, requestedValue, confirmationType);

        mls.LogInfo($"Created sell request. {scrapToSell.amount} items for ${value}.");
    }

    public void ConfirmSellRequest()
    {
        if (scrapToSell == null || sellRequest == null) return;

        sellRequest.confirmationType = ConfirmationType.Confirmed;

        mls.LogInfo($"Attempting to sell {scrapToSell.amount} items for ${scrapToSell.value}.");

        Utils.checkOvertimeBonus = true;

        if (IsHostOrServer)
        {
            ConfirmSellRequestOnServer();
        }
        else
        {
            ConfirmSellRequestOnClient();
        }

        sellRequest = null;
    }

    private void ConfirmSellRequestOnServer()
    {
        StartOfRound.Instance.StartCoroutine(PerformSellOnServer());
    }

    private void ConfirmSellRequestOnClient()
    {
        int fromPlayerId = (int)StartOfRound.Instance.localPlayerController.playerClientId;
        string networkObjectIdsString = NetworkUtils.GetNetworkObjectIdsString(scrapToSell.scrap);

        PluginNetworkBehaviour.Instance.PerformSellServerRpc(fromPlayerId, networkObjectIdsString, sellRequest.sellType, sellRequest.realValue, scrapToSell.amount);
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
        if (scrapToSell == null || sellRequest == null) yield return null;
        if (sellRequest.confirmationType != ConfirmationType.Confirmed) yield return null;

        if (DepositItemsDeskPatch.DepositItemsDesk == null)
        {
            mls.LogError($"Error: could not find depositItemsDesk. Are you landed at The Company building?");
            yield break;
        }

        if (ScrapEaterManager.CanUseScrapEater())
        {
            ScrapEaterManager.SetScrapToSuckOnServer(scrapToSell.scrap);
            yield return new WaitForSeconds(0.1f);
            ScrapEaterManager.StartRandomScrapEaterOnServer();

            yield break;
        }

        DepositItemsDeskPatch.PlaceItemsOnCounter(scrapToSell.scrap);
        PluginNetworkBehaviour.Instance.PlaceItemsOnCounterClientRpc(NetworkUtils.GetNetworkObjectIdsString(scrapToSell.scrap));
        PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();

        yield return new WaitForSeconds(0.5f);

        DepositItemsDeskPatch.DepositItemsDesk.SellItemsOnServer();

        scrapToSell = null;
    }
}
