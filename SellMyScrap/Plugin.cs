using BepInEx;
using com.github.zehsteam.SellMyScrap.Commands;
using com.github.zehsteam.SellMyScrap.Dependencies;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ShipInventoryProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(TakeyPlushProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance { get; private set; }

    #pragma warning disable IDE0051 // Remove unused private members
    private void Awake()
    #pragma warning restore IDE0051 // Remove unused private members
    {
        Instance = this;

        SellMyScrap.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        SellMyScrap.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(GameNetworkManagerPatch));
        _harmony.PatchAll(typeof(StartOfRoundPatch));
        _harmony.PatchAll(typeof(TimeOfDayPatch));
        _harmony.PatchAll(typeof(HUDManagerPatch));
        _harmony.PatchAll(typeof(TerminalPatch));
        _harmony.PatchAll(typeof(DepositItemsDeskPatch));
        _harmony.PatchAll(typeof(StartMatchLeverPatch));
        _harmony.PatchAll(typeof(InteractTriggerPatch));
        
        if (ShipInventoryProxy.Enabled)
        {
            ShipInventoryProxy.PatchAll(_harmony);
        }

        Assets.Load();

        ConfigManager.Initialize(Config);
        ScrapEaterManager.Initialize();

        NetcodePatcherAwake();
    }

    private void NetcodePatcherAwake()
    {
        try
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    try
                    {
                        // Safely attempt to retrieve custom attributes
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                        if (attributes.Length > 0)
                        {
                            try
                            {
                                // Safely attempt to invoke the method
                                method.Invoke(null, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                Logger.LogWarning($"Failed to invoke method {method.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle errors when fetching custom attributes, due to missing types or dependencies
                        Logger.LogWarning($"Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catch any general exceptions that occur in the process
            Logger.LogError($"An error occurred in NetcodePatcherAwake: {ex.Message}");
        }
    }

    public static void HandleLocalDisconnect()
    {
        CommandManager.OnLocalDisconnect();
        SellManager.CancelSellRequest();
    }

    public static void HandleTerminalQuit()
    {
        CommandManager.OnTerminalQuit();
        SellManager.CancelSellRequest();
    }
}
