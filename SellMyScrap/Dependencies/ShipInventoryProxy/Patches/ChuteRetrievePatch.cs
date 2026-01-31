using HarmonyLib;
using ShipInventoryUpdated.Scripts;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using SI_ItemData = ShipInventoryUpdated.Objects.ItemData;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;

[HarmonyPatch(typeof(ChuteRetrieve))]
internal static class ChuteRetrievePatch
{
    public static ChuteRetrieve Instance { get; private set; }
    public static List<GrabbableObject> SpawnedGrabbableObjects { get; private set; } = [];

    private static List<SI_ItemData> _itemsToCapture = [];

    [HarmonyPatch(nameof(ChuteRetrieve.Awake))]
    [HarmonyPrefix]
    private static void AwakePatch(ChuteRetrieve __instance)
    {
        Instance = __instance;
    }

    [HarmonyPatch(nameof(ChuteRetrieve.SpawnItemServer))]
    [HarmonyPostfix]
    private static void SpawnItemServerPatch(SI_ItemData data, NetworkObject __result)
    {
        if (_itemsToCapture.Count == 0) return;

        if (!_itemsToCapture.Contains(data))
        {
            return;
        }

        _itemsToCapture.Remove(data);

        if (__result.TryGetComponent(out GrabbableObject grabbableObject))
        {
            SpawnedGrabbableObjects.Add(grabbableObject);
        }
    }

    public static void StartCapturingSpawnedItems(SI_ItemData[] items)
    {
        ClearSpawnedGrabbableObjectsCache();
        _itemsToCapture = items.ToList();
    }

    public static void StopCapturingSpawnItems()
    {
        _itemsToCapture.Clear();
    }

    public static void ClearSpawnedGrabbableObjectsCache()
    {
        SpawnedGrabbableObjects.Clear(); 
    }
}
