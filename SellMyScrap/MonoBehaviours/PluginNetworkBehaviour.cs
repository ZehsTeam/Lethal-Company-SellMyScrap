﻿using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
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
        if (Plugin.IsHostOrServer) return;

        Plugin.logger.LogInfo("Syncing config with host.");
        Plugin.ConfigManager.SetHostConfigData(syncedConfigData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PerformSellServerRpc(int fromPlayerId, string networkObjectIdsString, SellType sellType, int value, int amount, int scrapEaterIndex = -1)
    {
        PlayerControllerB fromPlayerScript = StartOfRound.Instance.allPlayerScripts[fromPlayerId];
        List<GrabbableObject> grabbableObjects = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);

        string message = $"{fromPlayerScript.playerUsername} requested to {Enum.GetName(typeof(SellType), sellType)} {amount} items for ${value}";

        Plugin.logger.LogInfo(message);
        Utils.DisplayNotification(message);
        Plugin.Instance.PerformSellOnServerFromClient(grabbableObjects, sellType, scrapEaterIndex);
    }

    [ClientRpc]
    public void PlaceItemsOnCounterClientRpc(string networkObjectIdsString)
    {
        if (Plugin.IsHostOrServer) return;

        DepositItemsDeskPatch.PlaceItemsOnCounter(NetworkUtils.GetGrabbableObjects(networkObjectIdsString));
    }

    [ClientRpc]
    public void SetMicrophoneSpeakDataClientRpc(bool speakInShip, int clipIndex)
    {
        if (Plugin.IsHostOrServer) return;

        DepositItemsDeskPatch.SetMicrophoneSpeakDataOnClient(speakInShip, clipIndex);
    }
}
