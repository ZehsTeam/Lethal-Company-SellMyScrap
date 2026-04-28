using BepInEx.Bootstrap;
using com.github.zehsteam.TakeyPlush.Managers;
using System;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.SellMyScrap.Dependencies.TakeyPlushMod;

internal static class TakeyPlushProxy
{
    public const string PLUGIN_GUID = TakeyPlush.MyPluginInfo.PLUGIN_GUID;
    public static bool IsEnabled { get { _isEnabled ??= Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID); return _isEnabled ?? false; } }
    private static bool? _isEnabled;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void TriggerDinkDonkScrapEaterSpawnedEvent()
    {
        try
        {
            Events.InvokeOnDinkDonkScrapEaterSpawned();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to invoke OnDinkDonkScrapEaterSpawned event in TakeyPlush. {ex}");
        }
    }
}
