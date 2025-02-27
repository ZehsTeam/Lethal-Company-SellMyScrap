using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class ScrapEaterManager
{
    public static List<ScrapEater> ScrapEaters { get; private set; } = [];

    internal static void Initialize()
    {
        ConfigManager configManager = Plugin.ConfigManager;

        ScrapEaters = [
            new ScrapEater(Content.OctolarScrapEaterPrefab, () => {
                return configManager.OctolarSpawnWeight.Value;
            }),
            new ScrapEater(Content.TakeyScrapEaterPrefab, () => {
                return configManager.TakeySpawnWeight.Value;
            }),
            new ScrapEater(Content.MaxwellScrapEaterPrefab, () => {
                return configManager.MaxwellSpawnWeight.Value;
            }),
            new ScrapEater(Content.YippeeScrapEaterPrefab, () => {
                return configManager.YippeeSpawnWeight.Value;
            }),
            new ScrapEater(Content.CookieFumoScrapEaterPrefab, () => {
                return configManager.CookieFumoSpawnWeight.Value;
            }),
            new ScrapEater(Content.PsychoScrapEaterPrefab, () => {
                return configManager.PsychoSpawnWeight.Value;
            }),
            new ScrapEater(Content.ZombiesScrapEaterPrefab, () => {
                return configManager.ZombiesSpawnWeight.Value;
            }),
            new ScrapEater(Content.WolfyScrapEaterPrefab, () => {
                return configManager.WolfySpawnWeight.Value;
            }),
        ];
    }

    internal static bool CanUseScrapEater()
    {
        int spawnChance = Plugin.ConfigManager.ScrapEaterChance.Value;
        return Utils.RandomPercent(spawnChance);
    }

    internal static bool HasScrapEater(int index)
    {
        if (ScrapEaters.Count == 0) return false;
        if (index < 0 || index > ScrapEaters.Count - 1) return false;

        return true;
    }

    /// <summary>
    /// Register your scrap eater.
    /// </summary>
    /// <param name="spawnPrefab">Your scrap eater spawn prefab.</param>
    /// <param name="GetSpawnWeight">Func for getting your spawnWeight config setting value.</param>
    public static void AddScrapEater(GameObject spawnPrefab, Func<int> GetSpawnWeight)
    {
        ScrapEaters.Add(new ScrapEater(spawnPrefab, GetSpawnWeight));
    }

    internal static void StartRandomScrapEaterOnServer(List<GrabbableObject> scrap, int variantIndex = -1)
    {
        if (!NetworkUtils.IsServer) return;

        int index = GetRandomScrapEaterIndex();
        if (index == -1) return;

        StartScrapEaterOnServer(index, scrap, variantIndex);
    }

    internal static void StartScrapEaterOnServer(int index, List<GrabbableObject> scrap, int variantIndex = -1)
    {
        if (!NetworkUtils.IsServer) return;

        GameObject prefab = ScrapEaters[index].SpawnPrefab;
        GameObject gameObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);

        ScrapEaterBehaviour behaviour = gameObject.GetComponent<ScrapEaterBehaviour>();
        behaviour.SetData(scrap, variantIndex);

        Plugin.Logger.LogInfo($"Spawned scrap eater #{index + 1}");
    }

    private static int GetRandomScrapEaterIndex()
    {
        return Utils.GetRandomIndexFromWeightList(ScrapEaters.Select(x => x.GetSpawnWeight()).ToList());
    }
}
