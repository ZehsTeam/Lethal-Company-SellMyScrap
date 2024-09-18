using BepInEx.Bootstrap;
using com.github.zehsteam.TakeyPlush;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.SellMyScrap.Dependencies;

internal static class TakeyPlushProxy
{
    public const string PLUGIN_GUID = TakeyPlush.MyPluginInfo.PLUGIN_GUID;
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void TriggerDinkDonkScrapEaterSpawnedEvent()
    {
        Events.OnDinkDonkScrapEaterSpawned?.Invoke();
    }
}
