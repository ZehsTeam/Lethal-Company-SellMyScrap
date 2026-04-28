using HarmonyLib;
using System;
using System.Reflection;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.Vanilla;

internal static class VehicleControllerProxy
{
    private static readonly FieldInfo _attachedVehicleField = AccessTools.Field(typeof(StartOfRound), "attachedVehicle");

    public static bool IsEnabled => _attachedVehicleField != null;

    public static GrabbableObject[] GetGrabbableObjects()
    {
        if (StartOfRound.Instance == null)
            return [];

        if (_attachedVehicleField == null)
            return [];

        try
        {
            object vehicleController = _attachedVehicleField.GetValue(StartOfRound.Instance);
            if (vehicleController == null) return [];

            if (vehicleController is not NetworkBehaviour networkBehaviour)
                return [];

            return networkBehaviour.GetComponentsInChildren<GrabbableObject>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get GrabbableObjects from attached vehicle. {ex}");
            return [];
        }
    }
}
