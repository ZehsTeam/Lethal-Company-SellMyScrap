using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance {  get; private set; }

    private void Awake()
    {
        // Ensure there is only one instance of the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate object
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null && Instance != this)
        {
            // Ensure only the server can handle despawning duplicate instances
            if (IsServer)
            {
                NetworkObject.Despawn(); // Despawn the networked object
            }

            return;
        }

        Instance = this;
    }

    [ClientRpc]
    public void SetSyncedConfigValueClientRpc(string section, string key, string value, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkUtils.IsServer) return;

        SyncedConfigEntryBase.SetValueFromServer(section, key, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PerformSellServerRpc(ScrapToSell scrapToSell, SellType sellType, int scrapEaterIndex = -2, int scrapEaterVariantIndex = -1, ServerRpcParams serverRpcParams = default)
    {
        var senderClientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(senderClientId)) return;

        PlayerControllerB playerScript = PlayerUtils.GetPlayerScriptByClientId(senderClientId);

        if (playerScript == null)
        {
            Plugin.Logger.LogError("Failed to perform sell server rpc. PlayerControllerB is null.");
            return;
        }

        string message = $"{playerScript.playerUsername} requested to {Enum.GetName(typeof(SellType), sellType)} {scrapToSell.ItemCount} items for ${scrapToSell.RealTotalScrapValue}";

        Plugin.Logger.LogInfo(message);
        HUDManager.Instance.DisplayGlobalNotification(message);

        Plugin.Instance.PerformSellOnServerFromClient(scrapToSell, sellType, scrapEaterIndex, scrapEaterVariantIndex);
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
