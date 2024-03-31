using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance;

    private int clientsPlacedItemsOnCounter = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ClientRpc]
    public void SendConfigToPlayerClientRpc(SyncedConfigData syncedConfigData, ClientRpcParams clientRpcParams = default)
    {
        if (SellMyScrapBase.IsHostOrServer) return;

        SellMyScrapBase.mls.LogInfo("Syncing config with host.");
        SellMyScrapBase.Instance.ConfigManager.SetHostConfigData(syncedConfigData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PerformSellServerRpc(int fromPlayerId, string networkObjectIdsString, SellType sellType, int value, int amount, int scrapEaterIndex = -1)
    {
        PlayerControllerB fromPlayerScript = StartOfRound.Instance.allPlayerScripts[fromPlayerId];
        List<GrabbableObject> grabbableObjects = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);

        string message = $"{fromPlayerScript.playerUsername} requested to {Enum.GetName(typeof(SellType), sellType)} {amount} items for ${value}";

        SellMyScrapBase.mls.LogInfo(message);
        Utils.DisplayNotification(message);
        SellMyScrapBase.Instance.PerformSellOnServerFromClient(grabbableObjects, sellType, scrapEaterIndex);
    }

    [ClientRpc]
    public void PlaceItemsOnCounterClientRpc(string networkObjectIdsString)
    {
        DepositItemsDeskPatch.PlaceItemsOnCounter(NetworkUtils.GetGrabbableObjects(networkObjectIdsString));
        PlacedItemsOnCounterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlacedItemsOnCounterServerRpc()
    {
        clientsPlacedItemsOnCounter++;
    }

    public bool AllClientsPlacedItemsOnCounter()
    {
        if (clientsPlacedItemsOnCounter >= GameNetworkManager.Instance.connectedPlayers)
        {
            clientsPlacedItemsOnCounter = 0;
            return true;
        }

        return false;
    }

    [ClientRpc]
    public void SetDepositItemsDeskAudioClipClientRpc(int index)
    {
        DepositItemsDeskPatch.SetAudioClip(index);
    }

    [ClientRpc]
    public void EnableSpeakInShipClientRpc()
    {
        DepositItemsDeskPatch.EnableSpeakInShip();
    }
}
