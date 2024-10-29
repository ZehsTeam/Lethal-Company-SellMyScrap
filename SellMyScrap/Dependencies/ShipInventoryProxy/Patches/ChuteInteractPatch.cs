using HarmonyLib;
using ShipInventory.Objects;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;

[HarmonyPatch(typeof(ChuteInteract))]
internal static class ChuteInteractPatch
{
    private static List<GrabbableObject> _spawnedGrabbableObjects = [];

    [HarmonyPatch(nameof(ChuteInteract.SpawnItemClientRpc))]
    [HarmonyPostfix]
    private static void SpawnItemClientRpcPatch(NetworkObjectReference networkObject)
    {
        if (!ShipInventoryProxy.IsSpawning) return;

        #pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
        if (!networkObject.TryGet(out NetworkObject netObject))
        {
            return;
        }
        #pragma warning restore Harmony003 // Harmony non-ref patch parameters modified

        if (netObject.TryGetComponent(out GrabbableObject grabbableObject))
        {
            _spawnedGrabbableObjects.Add(grabbableObject);
        }
    }
    
    public static List<GrabbableObject> GetSpawnedGrabbableObjects()
    {
        return _spawnedGrabbableObjects.Where(x => x != null).ToList();
    }

    public static void ClearSpawnedGrabbableObjectsCache()
    {
        _spawnedGrabbableObjects.Clear();
    }
}
