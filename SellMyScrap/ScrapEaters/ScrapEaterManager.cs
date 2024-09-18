﻿using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class ScrapEaterManager
{
    public static List<ScrapEater> ScrapEaters { get; private set; } = [];

    internal static void Initialize()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        ScrapEaters = [
            new ScrapEater(Content.OctolarScrapEaterPrefab, () => {
                return configManager.OctolarSpawnWeight;
            }),
            new ScrapEater(Content.TakeyScrapEaterPrefab, () => {
                return configManager.TakeySpawnWeight;
            }),
            new ScrapEater(Content.MaxwellScrapEaterPrefab, () => {
                return configManager.MaxwellSpawnWeight;
            }),
            new ScrapEater(Content.YippeeScrapEaterPrefab, () => {
                return configManager.YippeeSpawnWeight;
            }),
            new ScrapEater(Content.CookieFumoScrapEaterPrefab, () => {
                return configManager.CookieFumoSpawnWeight;
            }),
            new ScrapEater(Content.PsychoScrapEaterPrefab, () => {
                return configManager.PsychoSpawnWeight;
            }),
            new ScrapEater(Content.ZombiesScrapEaterPrefab, () => {
                return configManager.ZombiesSpawnWeight;
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
        if (ScrapEaters.Count == 0) return false;
        if (index < 0 || index > ScrapEaters.Count - 1) return false;

        return true;
    }

    /// <summary>
    /// Register your scrap eater.
    /// </summary>
    /// <param name="spawnPrefab">Your scrap eater spawn prefab.</param>
    /// <param name="GetSpawnWeight">Func for getting your spawnWeight config setting value.</param>
    public static void AddScrapEater(GameObject spawnPrefab, System.Func<int> GetSpawnWeight)
    {
        ScrapEaters.Add(new ScrapEater(spawnPrefab, GetSpawnWeight));
    }

    internal static void StartRandomScrapEaterOnServer(List<GrabbableObject> scrap)
    {
        if (!NetworkUtils.IsServer) return;

        int index = GetRandomScrapEaterIndex();
        if (index == -1) return;

        StartScrapEaterOnServer(index, scrap);
    }

    internal static void StartScrapEaterOnServer(int index, List<GrabbableObject> scrap)
    {
        if (!NetworkUtils.IsServer) return;

        GameObject prefab = ScrapEaters[index].SpawnPrefab;
        GameObject gameObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Spawn(destroyWithScene: true);

        ScrapEaterBehaviour behaviour = gameObject.GetComponent<ScrapEaterBehaviour>();
        behaviour.SetTargetScrapOnServer(scrap);

        Plugin.logger.LogInfo($"Spawned scrap eater #{index + 1}");
    }

    private static int GetRandomScrapEaterIndex()
    {
        if (PlayerUtils.IsLocalPlayer(PlayerName.PsychoHypnotic) && !ModpackSaveSystem.ReadValue("ForceShowedTakeyScrapEater3", false) && Utils.RandomPercent(80))
        {
            ModpackSaveSystem.WriteValue("ForceShowedTakeyScrapEater3", true);
            return 1; // Takey scrap eater index.
        }

        return Utils.GetRandomIndexFromWeightList(ScrapEaters.Select(_ => _.GetSpawnWeight()).ToList());
    }
}
