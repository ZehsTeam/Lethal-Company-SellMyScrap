using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.SellMyScrap.Dependencies.Vanilla;

internal static class VehicleControllerProxy
{
    public static bool Enabled
    {
        get
        {
            _enabled ??= GetEnabledState();
            return _enabled.Value;
        }
    }

    private static bool? _enabled;

    private static bool GetEnabledState()
    {
        try
        {
            Assembly assembly = typeof(StartOfRound).Assembly;
            return assembly.GetType("VehicleController") != null;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get enabled state from VehicleControllerProxy. {ex}");
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static GrabbableObject[] GetGrabbableObjects()
    {
        if (StartOfRound.Instance == null)
        {
            return [];
        }

        try
        {
            VehicleController vehicleController = StartOfRound.Instance.attachedVehicle;
            if (vehicleController == null) return [];

            return vehicleController.GetComponentsInChildren<GrabbableObject>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get GrabbableObjects from attached vehicle. {ex}");
            return [];
        }
    }
}
