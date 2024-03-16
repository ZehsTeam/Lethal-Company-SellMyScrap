using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class PluginNetworkBehaviour : NetworkBehaviour
{
    public static PluginNetworkBehaviour Instance;

    void Awake()
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
    public void PerformSellServerRpc(int fromPlayerId, string networkObjectIdsString, SellType sellType, int value, int amount)
    {
        PlayerControllerB fromPlayerScript = StartOfRound.Instance.allPlayerScripts[fromPlayerId];
        List<GrabbableObject> grabbableObjects = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);
        string message = $"{fromPlayerScript.playerUsername} requested to {Enum.GetName(typeof(SellType), sellType)} {amount} items for ${value}";

        SellMyScrapBase.mls.LogInfo(message);
        SellMyScrapBase.Instance.DisplayGlobalNotification(message);
        SellMyScrapBase.Instance.PerformSellOnServerFromClient(grabbableObjects, sellType);
    }

    [ClientRpc]
    public void SetScrapToSuckClientRpc(string networkObjectIdsString)
    {
        ScrapEaterManager.SetScrapToSuckOnClient(NetworkUtils.GetGrabbableObjects(networkObjectIdsString));
    }

    [ClientRpc]
    public void StartScrapEaterClientRpc(int index, int slideMaterialIndex)
    {
        ScrapEaterManager.StartScrapEaterOnClient(index, slideMaterialIndex);
    }

    [ClientRpc]
    public void PlaceItemsOnCounterClientRpc(string networkObjectIdsString)
    {
        if (IsHost || IsServer) return;

        DepositItemsDeskPatch.PlaceItemsOnCounter(NetworkUtils.GetGrabbableObjects(networkObjectIdsString));
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
