using BepInEx.Bootstrap;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;
using com.github.zehsteam.SellMyScrap.Helpers;
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
        try
        {
            harmony.PatchAll(typeof(ChuteInteractPatch));
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to apply ShipInventory patches. {ex}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static ShipInventoryItemData[] GetItems()
    {
        List<ShipInventoryItemData> shipInventoryItems = [];
        
        try
        {
            foreach (var itemData in ItemManager.GetItems().Where(x => ScrapHelper.IsScrap(x.GetItem())))
            {
                shipInventoryItems.Add(new ShipInventoryItemData(itemData));
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to get ShipInventory items. {ex}");
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
            Plugin.Logger.LogError("Failed to spawn ShipInventory items. ChuteInteract instance is null.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        if (ChuteInteract.Instance.spawnCoroutine != null)
        {
            Plugin.Logger.LogError("Failed to spawn ShipInventory items. ChuteInteract instance spawnCoroutine is busy.");
            SpawnItemsStatus = SpawnItemsStatus.Busy;
            yield break;
        }

        ItemData[] items = shipInventoryItems.Select(x => x.GetItemData()).Where(x => !x.Equals(default)).ToArray();

        if (items.Length == 0)
        {
            Plugin.Logger.LogError("Failed to spawn ShipInventory items. ItemData array is empty.");
            SpawnItemsStatus = SpawnItemsStatus.Failed;
            yield break;
        }

        Plugin.Logger.LogInfo($"Server scheduled to spawn {items.Count()} new ShipInventory items!");

        ChuteInteract.Instance.RetrieveItems(items);

        float startTime = Time.realtimeSinceStartup;
        float maxWaitTime = (items.Length * ShipInventory.ShipInventory.Config.TimeToRetrieve.Value) + 30f;

        yield return new WaitUntil(() => ChuteInteract.Instance.spawnCoroutine == null || Time.realtimeSinceStartup - startTime > maxWaitTime);

        if (ChuteInteract.Instance.spawnCoroutine == null)
        {
            Plugin.Logger.LogInfo($"Successfully spawned items from ShipInventory!");
            SpawnItemsStatus = SpawnItemsStatus.Success;
        }
        else
        {
            Plugin.Logger.LogError($"Failed to spawn items from ShipInventory. ChuteInteract instance spawnCoroutine timed out.");
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
