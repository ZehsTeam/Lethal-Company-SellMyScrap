using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        var networkHandlerHost = Object.Instantiate(Assets.networkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch("OnClientConnect")]
    [HarmonyPrefix]
    static void OnClientConnectPatch(ref ulong clientId)
    {
        SendConfigToNewConnectedPlayer(clientId);
    }

    private static void SendConfigToNewConnectedPlayer(ulong clientId)
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        SellMyScrapBase.mls.LogInfo($"Sending config to client: {clientId}");

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(SellMyScrapBase.Instance.ConfigManager), clientRpcParams);
    }

    [HarmonyPatch("OnLocalDisconnect")]
    [HarmonyPrefix]
    static void OnLocalDisconnectPatch()
    {
        SellMyScrapBase.Instance.OnLocalDisconnect();
    }
}
