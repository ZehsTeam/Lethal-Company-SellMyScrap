using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

public class ScrapEaterManager
{
    internal static List<ScrapEater> scrapEaters = new List<ScrapEater>();

    internal static void Initialize()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        scrapEaters = [
            new ScrapEater(Content.octolarScrapEaterPrefab, () => {
                return configManager.OctolarSpawnWeight;
            }),
            new ScrapEater(Content.takeyScrapEaterPrefab, () => {
                return configManager.TakeySpawnWeight;
            }),
            new ScrapEater(Content.maxwellScrapEaterPrefab, () => {
                return configManager.MaxwellSpawnWeight;
            }),
            new ScrapEater(Content.yippeeScrapEaterPrefab, () => {
                return configManager.YippeeSpawnWeight;
            }),
            new ScrapEater(Content.cookieFumoScrapEaterPrefab, () => {
                return configManager.CookieFumoSpawnWeight;
            }),
            new ScrapEater(Content.psychoScrapEaterPrefab, () => {
                return configManager.PsychoSpawnWeight;
            }),
        ];
    }

    internal static bool CanUseScrapEater()
    {
        int spawnChance = Plugin.ConfigManager.ScrapEaterChance;
        return Utils.RandomPercent(spawnChance);
    }

    internal static bool HasScrapEater(int index)
    {
        if (scrapEaters.Count == 0) return false;
        if (index < 0 || index > scrapEaters.Count - 1) return false;

        return true;
    }

    /// <summary>
    /// Register your scrap eater.
    /// </summary>
    /// <param name="spawnPrefab">Your scrap eater spawn prefab.</param>
    /// <param name="GetSpawnWeight">Func for getting your spawnWeight config setting value.</param>
    public static void AddScrapEater(GameObject spawnPrefab, System.Func<int> GetSpawnWeight)
    {
        scrapEaters.Add(new ScrapEater(spawnPrefab, GetSpawnWeight));
    }

    internal static void StartRandomScrapEaterOnServer(List<GrabbableObject> scrap)
    {
        if (!Plugin.IsHostOrServer) return;

        int index = GetRandomScrapEaterIndex();
        if (index == -1) return;

        StartScrapEaterOnServer(index, scrap);
    }

    internal static void StartScrapEaterOnServer(int index, List<GrabbableObject> scrap)
    {
        if (!Plugin.IsHostOrServer) return;

        GameObject prefab = scrapEaters[index].spawnPrefab;
        GameObject gameObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);

        ScrapEaterBehaviour behaviour = gameObject.GetComponent<ScrapEaterBehaviour>();
        behaviour.SetTargetScrapOnServer(scrap);

        Plugin.logger.LogInfo($"Spawned scrap eater #{index + 1}");
    }

    private static int GetRandomScrapEaterIndex()
    {
        List<(int index, int weight)> weightedItems = new List<(int index, int weight)>();

        for (int i = 0; i < scrapEaters.Count; i++)
        {
            int spawnWeight = scrapEaters[i].GetSpawnWeight();
            if (spawnWeight <= 0) continue;

            weightedItems.Add((i, spawnWeight));
        }

        int totalWeight = 0;
        foreach (var (_, weight) in weightedItems)
        {
            totalWeight += weight;
        }

        if (totalWeight == 0) return -1;

        int randomNumber = Random.Range(0, totalWeight);

        int cumulativeWeight = 0;
        foreach (var (index, weight) in weightedItems)
        {
            cumulativeWeight += weight;
            if (randomNumber < cumulativeWeight)
            {
                return index;
            }
        }

        // This should never happen if weights are correctly specified
        throw new System.InvalidOperationException("Weights are not properly specified.");
    }
}
