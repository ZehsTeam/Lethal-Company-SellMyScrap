using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ClientRpc]
    public void SendConfigToPlayerClientRpc(SyncedConfigData syncedConfigData, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkUtils.IsServer) return;

        Plugin.logger.LogInfo("Syncing config with host.");
        Plugin.ConfigManager.SetHostConfigData(syncedConfigData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PerformSellServerRpc(NetworkObjectReference[] networkObjectReferences, SellType sellType, int value, int amount, int scrapEaterIndex = -1, ServerRpcParams serverRpcParams = default)
    {
        var senderClientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(senderClientId)) return;

        PlayerControllerB playerScript = PlayerUtils.GetPlayerScriptByClientId(senderClientId);

        if (playerScript == null)
        {
            Plugin.logger.LogError("Failed to perform sell server rpc. PlayerControllerB is null.");
            return;
        }

        string message = $"{playerScript.playerUsername} requested to {Enum.GetName(typeof(SellType), sellType)} {amount} items for ${value}";

        Plugin.logger.LogInfo(message);
        HUDManager.Instance.DisplayGlobalNotification(message);

        Plugin.Instance.PerformSellOnServerFromClient(networkObjectReferences, sellType, scrapEaterIndex);
    }

    [ClientRpc]
    public void PlaceItemsOnCounterClientRpc(NetworkObjectReference[] networkObjectReferences)
    {
        if (NetworkUtils.IsServer) return;

        DepositItemsDeskPatch.PlaceItemsOnCounter(NetworkUtils.GetGrabbableObjects(networkObjectReferences));
    }

    [ClientRpc]
    public void SetMicrophoneSpeakDataClientRpc(bool speakInShip, int clipIndex)
    {
        if (NetworkUtils.IsServer) return;

        DepositItemsDeskPatch.SetMicrophoneSpeakDataOnClient(speakInShip, clipIndex);
    }
}
