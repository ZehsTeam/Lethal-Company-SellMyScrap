using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Zehs.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    static void AwakePatch()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var networkHandlerHost = Object.Instantiate(NetworkObjectManagerPatch.networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }

    [HarmonyPatch("OnClientConnect")]
    [HarmonyPrefix]
    static void OnClientConnectPatch(ref ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        SellMyScrapBase.mls.LogInfo($"Sending config to client: {clientId}");

        ConfigSyncBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(SellMyScrapBase.Instance.ConfigManager), clientRpcParams);
    }
}
