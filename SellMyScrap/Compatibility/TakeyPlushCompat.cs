using BepInEx.Bootstrap;

namespace com.github.zehsteam.SellMyScrap.Compatibility;

internal class TakeyPlushCompat
{
    public const string ModGUID = TakeyPlush.MyPluginInfo.PLUGIN_GUID;
    public static bool HasMod => Chainloader.PluginInfos.ContainsKey(ModGUID);

    public static void TriggerDinkDonkScrapEaterSpawnedEvent()
    {
        try
        {
            if (!HasMod) return;

            TakeyPlush.Events.OnDinkDonkScrapEaterSpawned?.Invoke();
        }
        catch (System.Exception e)
        {
            Plugin.logger.LogError($"Failed to trigger DinkDonk scrap eater spawned event for TakeyPlush.\n\n{e}");
        }
    }
}
