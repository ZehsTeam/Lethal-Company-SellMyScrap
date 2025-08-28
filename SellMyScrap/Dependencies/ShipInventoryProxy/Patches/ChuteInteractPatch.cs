using CSync.Lib;
using HarmonyLib;
using ShipInventory;
using ShipInventory.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

    [HarmonyPatch(nameof(ChuteInteract.SpawnCoroutine), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SpawnCoroutineTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo targetField = AccessTools.Field(typeof(Configuration), nameof(Configuration.StopAfter));
        MethodInfo targetMethod = AccessTools.PropertyGetter(typeof(SyncedEntry<int>), nameof(SyncedEntry<int>.Value));
        MethodInfo replacementMethod = AccessTools.Method(typeof(ChuteInteractPatch), nameof(GetStopAfterValue));

        if (targetField == null || targetMethod == null || replacementMethod == null)
        {
            Logger.LogError("Failed to apply SpawnCoroutine transpiler.");
            return instructions;
        }

        List<CodeInstruction> newInstructions = [];

        foreach (var instruction in instructions)
        {
            if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
            {
                newInstructions.Add(instruction);
                continue;
            }

            if (instruction.operand is not MethodInfo methodInfo)
            {
                newInstructions.Add(instruction);
                continue;
            }

            if (methodInfo == targetMethod && newInstructions[^1].operand is FieldInfo fieldInfo && fieldInfo == targetField)
            {
                newInstructions.RemoveAt(newInstructions.Count - 1);
                newInstructions.RemoveAt(newInstructions.Count - 1);
                newInstructions.Add(new CodeInstruction(OpCodes.Call, replacementMethod));
                continue;
            }

            newInstructions.Add(instruction);
        }

        return newInstructions.AsEnumerable();
    }

    private static int GetStopAfterValue()
    {
        if (ShipInventoryProxy.IsSpawning)
        {
            return int.MaxValue;
        }

        return ShipInventory.ShipInventory.Configuration.StopAfter.Value;
    }
}
