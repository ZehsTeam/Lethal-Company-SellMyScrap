using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void StartPatch()
    {
        AddNetworkPrefabs();
    }

    private static void AddNetworkPrefabs()
    {
        AddNetworkPrefab(Content.networkHandlerPrefab);

        ScrapEaterManager.scrapEaters.ForEach(scrapEater =>
        {
            AddNetworkPrefab(scrapEater.spawnPrefab);
        });
    }

    private static void AddNetworkPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        NetworkManager.Singleton.AddNetworkPrefab(prefab);

        SellMyScrapBase.mls.LogInfo($"Registered \"{prefab.name}\" network prefab.");
    }
}
