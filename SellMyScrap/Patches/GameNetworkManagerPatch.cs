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
        AddNetworkPrefab(Content.NetworkHandlerPrefab);

        ScrapEaterManager.ScrapEaters.ForEach(scrapEater =>
        {
            AddNetworkPrefab(scrapEater.SpawnPrefab);
        });
    }

    private static void AddNetworkPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        NetworkManager.Singleton.AddNetworkPrefab(prefab);

        Plugin.logger.LogInfo($"Registered \"{prefab.name}\" network prefab.");
    }
}
