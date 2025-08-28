using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal static class GameNetworkManagerPatch
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        AddNetworkPrefabs();
    }

    private static void AddNetworkPrefabs()
    {
        AddNetworkPrefab(Assets.NetworkHandlerPrefab);

        foreach (var scrapEater in ScrapEaterManager.ScrapEaters)
        {
            AddNetworkPrefab(scrapEater.SpawnPrefab);
        }
    }

    private static void AddNetworkPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Logger.LogError("Failed to register network prefab. GameObject is null.");
            return;
        }

        NetworkManager.Singleton.AddNetworkPrefab(prefab);

        Logger.LogInfo($"Registered \"{prefab.name}\" network prefab.");
    }
}
