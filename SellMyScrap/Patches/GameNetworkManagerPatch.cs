using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using Unity.Netcode;

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
        NetworkManager.Singleton.AddNetworkPrefab(Content.networkHandlerPrefab);
        SellMyScrapBase.mls.LogInfo($"Registered \"{Content.networkHandlerPrefab.name}\" network prefab.");

        ScrapEaterManager.scrapEaters.ForEach(scrapEater =>
        {
            NetworkManager.Singleton.AddNetworkPrefab(scrapEater.spawnPrefab);
            SellMyScrapBase.mls.LogInfo($"Registered \"{scrapEater.spawnPrefab.name}\" network prefab.");
        });
    }
}
