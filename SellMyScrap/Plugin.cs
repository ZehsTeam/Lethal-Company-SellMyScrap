using BepInEx;
using com.github.zehsteam.SellMyScrap.Dependencies.LethalConfigMod;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryMod;
using com.github.zehsteam.SellMyScrap.Dependencies.TakeyPlushMod;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ShipInventoryProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(TakeyPlushProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance { get; private set; }

#if LC_VERSION_73
    const string TargetUnityVersion = "2022.3.62";
#elif LC_VERSION_72
    const string TargetUnityVersion = "2022.3.9";
#else
    const string TargetUnityVersion = "2022.3";
#endif

    #pragma warning disable IDE0051 // Remove unused private members
    private void Awake()
    #pragma warning restore IDE0051 // Remove unused private members
    {
        SellMyScrap.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        SellMyScrap.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        if (!Utils.IsUnityVersion(TargetUnityVersion))
        {
            SellMyScrap.Logger.LogWarning($"Skipping {MyPluginInfo.PLUGIN_NAME} because it targets a different version of Unity ({TargetUnityVersion})");
            return;
        }

        Instance = this;

        _harmony.PatchAll(typeof(GameNetworkManagerPatch));
        _harmony.PatchAll(typeof(StartOfRoundPatch));
        _harmony.PatchAll(typeof(TimeOfDayPatch));
        _harmony.PatchAll(typeof(HUDManagerPatch));
        _harmony.PatchAll(typeof(TerminalPatch));
        _harmony.PatchAll(typeof(DepositItemsDeskPatch));
        _harmony.PatchAll(typeof(StartMatchLeverPatch));
        _harmony.PatchAll(typeof(InteractTriggerPatch));
        
        if (ShipInventoryProxy.IsEnabled)
        {
            ShipInventoryProxy.PatchAll(_harmony);
        }

        Assets.Load();

        ConfigManager.Initialize(Config);
        ScrapEaterManager.Initialize();

        NetworkUtils.NetcodePatcherAwake();
    }
}
