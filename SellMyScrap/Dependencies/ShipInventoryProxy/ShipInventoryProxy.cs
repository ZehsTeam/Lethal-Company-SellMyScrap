using BepInEx.Bootstrap;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Extensions;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Objects;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using HarmonyLib;
using ShipInventoryUpdated.Configurations;
using ShipInventoryUpdated.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using SI_ItemData = ShipInventoryUpdated.Objects.ItemData;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

internal enum SpawnItemsStatus
{
    None,
    Spawning,
    Success,
    Failed
}

internal class ShipInventoryProxy
{
    public const string PLUGIN_GUID = ShipInventoryUpdated.MyPluginInfo.PLUGIN_GUID;
    public static bool Enabled
    {
        get
        {
            _enabled ??= Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);
            return _enabled.Value;
        }
    }

    private static bool? _enabled;

    public static bool IsSpawning => SpawnItemsStatus == SpawnItemsStatus.Spawning;

    public static SpawnItemsStatus SpawnItemsStatus { get; private set; }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAll(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(ChuteRetrievePatch));
        }
        catch (Exception ex)
        {
            Logger.LogError($"[ShipInventoryProxy] Failed to apply patches. {ex}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static SI_ItemDataProxy[] GetItemsAsProxies()
    {
        List<SI_ItemDataProxy> items = [];

        try
        {
            foreach (var itemData in Inventory.Items.Where(x => x.IsScrap()))
            {
                items.Add(new SI_ItemDataProxy(itemData));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get ShipInventory items. {ex}");
        }
        
        return items.ToArray();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static SI_ItemData[] GetValidItemsFromProxies(SI_ItemDataProxy[] proxyItems)
    {
        List<SI_ItemData> items = [];
        List<SI_ItemData> inventoryItems = Inventory.Items.ToList();

        foreach (var proxyItem in proxyItems)
        {
            SI_ItemData itemData = proxyItem.CreateItemData();

            if (inventoryItems.Contains(itemData))
            {
                inventoryItems.Remove(itemData);
                items.Add(itemData);
            }
        }

        return items.ToArray();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SpawnItems_Server(SI_ItemDataProxy[] proxyItems)
    {
        if (!NetworkUtils.IsServer) return;

        CoroutineRunner.Start(SpawnItems_ServerCoroutine(proxyItems));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static IEnumerator SpawnItems_ServerCoroutine(SI_ItemDataProxy[] proxyItems)
    {
        SpawnItemsStatus = SpawnItemsStatus.Spawning;

        SI_ItemData[] items = GetValidItemsFromProxies(proxyItems);

        if (items.Length == 0)
        {
            Logger.LogError("[ShipInventoryProxy] Failed to spawn ShipInventory items. No items to spawn.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        if (proxyItems.Length != items.Length)
        {
            Logger.LogWarning($"[ShipInventoryProxy] Received {proxyItems.Length} proxy items, but only {items.Length} are valid!");
        }

        Logger.LogInfo($"[ShipInventoryProxy] Server scheduled to spawn {items.Count()} items!");

        ChuteRetrievePatch.StartCapturingSpawnedItems(items);

        Inventory.Remove(items);

        float startTime = Time.realtimeSinceStartup;
        float retrieveSpeed = Configuration.Instance.Inventory.RetrieveSpeed.Value;
        float maxWaitTime = (ChuteRetrievePatch.Instance._spawnQueue.Count * retrieveSpeed) + 5f;

        float getElapsedTime() => Time.realtimeSinceStartup - startTime;

        yield return new WaitUntil(() =>
            ChuteRetrievePatch.Instance.SpawnCoroutine == null ||
            ChuteRetrievePatch.SpawnedGrabbableObjects.Count >= items.Length ||
            getElapsedTime() >= maxWaitTime
        );

        ChuteRetrievePatch.StopCapturingSpawnItems();

        List<GrabbableObject> grabbableObjects = GetSpawnedGrabbableObjects();

        if (grabbableObjects.Count != items.Length)
        {
            Logger.LogError($"[ShipInventoryProxy] Something went wrong when spawning items. Found {grabbableObjects.Count} GrabbableObject(s), but expected {items.Length}.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        ChuteRetrievePatch.StopCapturingSpawnItems();

        Logger.LogInfo($"[ShipInventoryProxy] Successfully spawned {grabbableObjects.Count} items from ShipInventory!");

        SpawnItemsStatus = SpawnItemsStatus.Success;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static List<GrabbableObject> GetSpawnedGrabbableObjects()
    {
        return ChuteRetrievePatch.SpawnedGrabbableObjects;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ClearSpawnedGrabbableObjectsCache()
    {
        ChuteRetrievePatch.ClearSpawnedGrabbableObjectsCache();
    }
}
