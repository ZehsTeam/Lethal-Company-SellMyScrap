using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!NetworkUtils.IsServer) return;

        var networkHandlerHost = Object.Instantiate(Content.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Plugin.ConfigManager.TrySetCustomValues();
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    private static void OnClientConnectPatch(ref ulong clientId)
    {
        SendConfigToNewConnectedPlayer(clientId);
    }

    private static void SendConfigToNewConnectedPlayer(ulong clientId)
    {
        if (!NetworkUtils.IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        Plugin.logger.LogInfo($"Sending config to client: {clientId}");

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(Plugin.ConfigManager), clientRpcParams);
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPrefix]
    private static void OnLocalDisconnectPatch()
    {
        Plugin.Instance.OnLocalDisconnect();
    }
}
