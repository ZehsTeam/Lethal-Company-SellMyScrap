using com.github.zehsteam.SellMyScrap.Helpers;
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
        ScrapEaters = [
            new ScrapEater(Assets.OctolarScrapEaterPrefab, () => {
                return ConfigManager.OctolarSpawnWeight.Value;
            }),
            new ScrapEater(Assets.TakeyScrapEaterPrefab, () => {
                return ConfigManager.TakeySpawnWeight.Value;
            }),
            new ScrapEater(Assets.MaxwellScrapEaterPrefab, () => {
                return ConfigManager.MaxwellSpawnWeight.Value;
            }),
            new ScrapEater(Assets.YippeeScrapEaterPrefab, () => {
                return ConfigManager.YippeeSpawnWeight.Value;
            }),
            new ScrapEater(Assets.CookieFumoScrapEaterPrefab, () => {
                return ConfigManager.CookieFumoSpawnWeight.Value;
            }),
            new ScrapEater(Assets.PsychoScrapEaterPrefab, () => {
                return ConfigManager.PsychoSpawnWeight.Value;
            }),
            new ScrapEater(Assets.ZombiesScrapEaterPrefab, () => {
                return ConfigManager.ZombiesSpawnWeight.Value;
            }),
            new ScrapEater(Assets.WolfyScrapEaterPrefab, () => {
                return ConfigManager.WolfySpawnWeight.Value;
            }),
            new ScrapEater(Assets.GlitchScrapEaterPrefab, () => {
                return ConfigManager.GlitchSpawnWeight.Value;
            }),
        ];
    }

    internal static bool CanUseScrapEater()
    {
        int spawnChance = ConfigManager.ScrapEaterChance.Value;
        return Utils.RollPercentChance(spawnChance);
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

        Logger.LogInfo($"Spawned scrap eater #{index + 1}");
    }

    private static int GetRandomScrapEaterIndex()
    {
        return Utils.GetRandomIndexFromWeightList(ScrapEaters.Select(x => x.GetSpawnWeight()).ToList());
    }
}
