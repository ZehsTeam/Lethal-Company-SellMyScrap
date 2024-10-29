using BepInEx.Bootstrap;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;
using HarmonyLib;
using ShipInventory.Helpers;
using ShipInventory.Objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

internal enum SpawnItemsStatus
{
    None,
    Spawning,
    Success,
    Failed,
    Busy
}

internal class ShipInventoryProxy
{
    public const string PLUGIN_GUID = ShipInventory.MyPluginInfo.PLUGIN_GUID;
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);

    public static bool IsSpawning {  get; private set; }
    public static SpawnItemsStatus SpawnItemsStatus
    {
        get
        {
            return _spawnItemsStatus;
        }
        set
        {
            _spawnItemsStatus = value;

            if (_spawnItemsStatus == SpawnItemsStatus.Spawning)
            {
                IsSpawning = true;
            }
            else
            {
                IsSpawning = false;
            }
        }
    }

    private static SpawnItemsStatus _spawnItemsStatus;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAll(Harmony harmony)
    {
        harmony.PatchAll(typeof(ChuteInteractPatch));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static ShipInventoryItemData[] GetItems()
    {
        List<ShipInventoryItemData> shipInventoryItems = [];
        
        foreach (var itemData in ItemManager.GetItems().Where(x => ScrapHelper.IsScrap(ScrapHelper.GetItemByName(x.ID))))
        {
            shipInventoryItems.Add(new ShipInventoryItemData(itemData.ID, itemData.SCRAP_VALUE, itemData.SAVE_DATA, itemData.PERSISTED_THROUGH_ROUNDS));
        }

        return shipInventoryItems.ToArray();
    }

    public static void SpawnItemsOnServer(ShipInventoryItemData[] shipInventoryItems)
    {
        if (!NetworkUtils.IsServer) return;

        Utils.StartCoroutine(SpawnItemsOnServerCoroutine(shipInventoryItems));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static IEnumerator SpawnItemsOnServerCoroutine(ShipInventoryItemData[] shipInventoryItems)
    {
        SpawnItemsStatus = SpawnItemsStatus.Spawning;

        if (ChuteInteract.Instance == null)
        {
            Plugin.logger.LogError("Failed to spawn ShipInventory items. ChuteInteract instance is null.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        if (ChuteInteract.Instance.spawnCoroutine != null)
        {
            Plugin.logger.LogError("Failed to spawn ShipInventory items. ChuteInteract instance spawnCoroutine is busy.");
            SpawnItemsStatus = SpawnItemsStatus.Busy;
            yield break;
        }

        ItemData[] items = shipInventoryItems.Select(x => x.GetItemData()).ToArray();

        if (items.Length == 0)
        {
            Plugin.logger.LogError("Failed to spawn ShipInventory items. ItemData array is empty.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        foreach (ItemData itemData in items)
        {
            ChuteInteract.Instance.spawnQueue.Enqueue(itemData);
        }

        ChuteInteract.Instance.spawnCoroutine = ChuteInteract.Instance.StartCoroutine(ChuteInteract.Instance.SpawnCoroutine());

        Plugin.logger.LogInfo($"Server scheduled to spawn {items.Count()} new ShipInventory items!");

        float startTime = Time.realtimeSinceStartup;
        float maxWaitTime = (ShipInventory.ShipInventory.Config.SpawnDelay.Value * items.Length) + 30f;

        yield return new WaitUntil(() => ChuteInteract.Instance.spawnCoroutine == null || Time.realtimeSinceStartup - startTime > maxWaitTime);

        if (ChuteInteract.Instance.spawnCoroutine == null)
        {
            Plugin.logger.LogInfo($"Successfully spawned items from ShipInventory!");
            SpawnItemsStatus = SpawnItemsStatus.Success;
        }
        else
        {
            Plugin.logger.LogError($"Failed to spawn items from ShipInventory. ChuteInteract instance spawnCoroutine timed out.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static List<GrabbableObject> GetSpawnedGrabbableObjects()
    {
        return ChuteInteractPatch.GetSpawnedGrabbableObjects();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ClearSpawnedGrabbableObjectsCache()
    {
        ChuteInteractPatch.ClearSpawnedGrabbableObjectsCache();
    }
}
