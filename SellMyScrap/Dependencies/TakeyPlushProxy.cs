using BepInEx.Bootstrap;
using com.github.zehsteam.TakeyPlush;
using System;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.SellMyScrap.Dependencies;

internal static class TakeyPlushProxy
{
    public const string PLUGIN_GUID = TakeyPlush.MyPluginInfo.PLUGIN_GUID;
    public static bool Enabled
    {
        get
        {
            _enabled ??= Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);
            return _enabled.Value;
        }
    }

    private static bool? _enabled;

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
