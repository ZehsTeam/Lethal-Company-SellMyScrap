using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class SyncedConfigManager
{
    private SyncedConfigData _hostConfigData;

    #region ConfigEntries
    // General Settings
    private ConfigEntry<bool> ExtendedLoggingCfg;

    // Sell Settings (Synced)
    private ConfigEntry<bool> SellGiftsCfg;
    private ConfigEntry<bool> SellShotgunsCfg;
    private ConfigEntry<bool> SellAmmoCfg;
    private ConfigEntry<bool> SellKnivesCfg;
    private ConfigEntry<bool> SellPicklesCfg;

    // Advanced Sell Settings (Synced)
    private ConfigEntry<bool> SellScrapWorthZeroCfg;
    private ConfigEntry<bool> OnlySellScrapOnFloorCfg;
    private ConfigEntry<string> PrioritySellListCfg;
    private ConfigEntry<string> DontSellListCfg;
    private ConfigEntry<string> SellListCfg;

    // Terminal Settings
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<bool> SortFoundItemsPriceCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;
    private ConfigEntry<float> RareVoiceLineChanceCfg;
    private ConfigEntry<bool> ShowQuotaWarningCfg;

    // Scrap Eater Settingss
    private ConfigEntry<int> ScrapEaterChanceCfg;
    private ConfigEntry<int> OctolarSpawnWeightCfg;
    private ConfigEntry<int> TakeySpawnWeightCfg;
    private ConfigEntry<int> MaxwellSpawnWeightCfg;
    private ConfigEntry<int> YippeeSpawnWeightCfg;
    private ConfigEntry<int> CookieFumoSpawnWeightCfg;
    private ConfigEntry<int> PsychoSpawnWeightCfg;
    private ConfigEntry<int> ZombiesSpawnWeightCfg;
    private ConfigEntry<int> WolfySpawnWeightCfg;
    #endregion

    #region Config Setting Get/Set Properties
    // General Settings
    internal bool ExtendedLogging { get { return ExtendedLoggingCfg.Value; } set => ExtendedLoggingCfg.Value = value; }

    // Sell Settings (Synced)
    internal bool SellGifts
    { 
        get => _hostConfigData == null ? SellGiftsCfg.Value : _hostConfigData.SellGifts;
        set
        {
            SellGiftsCfg.Value = value;
            SyncedConfigsChanged();
        }
    }
    
    internal bool SellShotguns
    { 
        get => _hostConfigData == null ? SellShotgunsCfg.Value : _hostConfigData.SellShotguns;
        set
        {
            SellShotgunsCfg.Value = value;
            SyncedConfigsChanged();
        }
    }
    
    internal bool SellAmmo 
    {
        get => _hostConfigData == null ? SellAmmoCfg.Value : _hostConfigData.SellAmmo;
        set
        {
            SellAmmoCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool SellKnives
    {
        get => _hostConfigData == null ? SellKnivesCfg.Value : _hostConfigData.SellKnives;
        set
        {
            SellKnivesCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool SellPickles
    { 
        get => _hostConfigData == null ? SellPicklesCfg.Value : _hostConfigData.SellPickles;
        set
        {
            SellPicklesCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    // Advanced Sell Settings (Synced)
    internal bool SellScrapWorthZero
    {
        get => _hostConfigData == null ? SellScrapWorthZeroCfg.Value : _hostConfigData.SellScrapWorthZero;
        set
        {
            SellScrapWorthZeroCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool OnlySellScrapOnFloor
    {
        get => _hostConfigData == null ? OnlySellScrapOnFloorCfg.Value : _hostConfigData.OnlySellScrapOnFloor;
        set
        {
            OnlySellScrapOnFloorCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal string[] PrioritySellList
    {
        get
        {
            string text = _hostConfigData == null ? PrioritySellListCfg.Value : _hostConfigData.PrioritySellList;
            return text.Split(",", System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
        set
        {
            PrioritySellListCfg.Value = string.Join(", ", value);
            SyncedConfigsChanged();
        }
    }

    internal string[] DontSellList
    { 
        get
        {
            string text = _hostConfigData == null ? DontSellListCfg.Value : _hostConfigData.DontSellList;
            return text.Split(",", System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
        set
        {
            DontSellListCfg.Value = string.Join(", ", value);
            SyncedConfigsChanged();
        }
    }

    internal string[] SellList
    {
        get
        {
            string text = _hostConfigData == null ? SellListCfg.Value : _hostConfigData.SellList;
            return text.Split(",", System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
        set
        {
            SellListCfg.Value = string.Join(", ", value);
            SyncedConfigsChanged();
        }
    }

    // Terminal Settings
    internal bool OverrideWelcomeMessage { get { return OverrideWelcomeMessageCfg.Value; } set => OverrideWelcomeMessageCfg.Value = value; }
    internal bool OverrideHelpMessage { get { return OverrideHelpMessageCfg.Value; } set => OverrideHelpMessageCfg.Value = value; }
    internal bool ShowFoundItems { get { return ShowFoundItemsCfg.Value; } set => ShowFoundItemsCfg.Value = value; }
    internal bool SortFoundItemsPrice { get { return SortFoundItemsPriceCfg.Value; } set => SortFoundItemsPriceCfg.Value = value; }
    internal bool AlignFoundItemsPrice { get { return AlignFoundItemsPriceCfg.Value; } set => AlignFoundItemsPriceCfg.Value = value; }

    // Misc Settings
    internal bool SpeakInShip { get { return SpeakInShipCfg.Value; } set => SpeakInShipCfg.Value = value; }
    internal float RareVoiceLineChance { get { return RareVoiceLineChanceCfg.Value; } set => RareVoiceLineChanceCfg.Value = value; }
    internal bool ShowQuotaWarning { get { return ShowQuotaWarningCfg.Value; } set => ShowQuotaWarningCfg.Value = value; }

    // Scrap Eaters
    internal int ScrapEaterChance { get { return ScrapEaterChanceCfg.Value; } set => ScrapEaterChanceCfg.Value = value; }
    internal int OctolarSpawnWeight { get { return OctolarSpawnWeightCfg.Value; } set => OctolarSpawnWeightCfg.Value = value; }
    internal int TakeySpawnWeight { get { return TakeySpawnWeightCfg.Value; } set => TakeySpawnWeightCfg.Value = value; }
    internal int MaxwellSpawnWeight { get { return MaxwellSpawnWeightCfg.Value; } set => MaxwellSpawnWeightCfg.Value = value; }
    internal int YippeeSpawnWeight { get { return YippeeSpawnWeightCfg.Value; } set => YippeeSpawnWeightCfg.Value = value; }
    internal int CookieFumoSpawnWeight { get { return CookieFumoSpawnWeightCfg.Value; } set => CookieFumoSpawnWeightCfg.Value = value; }
    internal int PsychoSpawnWeight { get { return PsychoSpawnWeightCfg.Value; } set => PsychoSpawnWeightCfg.Value = value; }
    internal int ZombiesSpawnWeight { get { return ZombiesSpawnWeightCfg.Value; } set => ZombiesSpawnWeightCfg.Value = value; }
    internal int WolfySpawnWeight { get { return WolfySpawnWeightCfg.Value; } set => WolfySpawnWeightCfg.Value = value; }
    #endregion

    public SyncedConfigManager()
    {
        BindConfigs();
        MigrateOldConfigSettings();
        ConfigHelper.ClearUnusedEntries();
    }
    
    private void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // General Settings
        ExtendedLoggingCfg = ConfigHelper.Bind("General Settings", "ExtendedLogging", defaultValue: false, requiresRestart: false, "Enable extended logging.");

        // Sell Settings
        SellGiftsCfg =    ConfigHelper.Bind("Sell Settings", "SellGifts",    defaultValue: false, requiresRestart: false, "Do you want to sell Gifts?");
        SellShotgunsCfg = ConfigHelper.Bind("Sell Settings", "SellShotguns", defaultValue: false, requiresRestart: false, "Do you want to sell Shotguns?");
        SellAmmoCfg =     ConfigHelper.Bind("Sell Settings", "SellAmmo",     defaultValue: false, requiresRestart: false, "Do you want to sell Ammo?");
        SellKnivesCfg =   ConfigHelper.Bind("Sell Settings", "SellKnives",   defaultValue: false, requiresRestart: false, "Do you want to sell Kitchen knives?");
        SellPicklesCfg =  ConfigHelper.Bind("Sell Settings", "SellPickles",  defaultValue: true,  requiresRestart: false, "Do you want to sell Jar of pickles?");

        // Advanced Sell Settings
        SellScrapWorthZeroCfg =   ConfigHelper.Bind("Advanced Sell Settings", "SellScrapWorthZero",   defaultValue: false,                                                              requiresRestart: false, "Do you want to sell scrap worth zero?");
        OnlySellScrapOnFloorCfg = ConfigHelper.Bind("Advanced Sell Settings", "OnlySellScrapOnFloor", defaultValue: false,                                                              requiresRestart: false, "Do you want to sell scrap that is only on the floor?");
        PrioritySellListCfg =     ConfigHelper.Bind("Advanced Sell Settings", "PrioritySellList",     defaultValue: "Tragedy, Comedy, Whoopie cushion, Easter egg, Clock, Soccer ball", requiresRestart: false, GetPrioritySellListDescription());
        DontSellListCfg =         ConfigHelper.Bind("Advanced Sell Settings", "DontSellList",         defaultValue: "",                                                                 requiresRestart: false, GetDontSellListDescription());
        SellListCfg =             ConfigHelper.Bind("Advanced Sell Settings", "SellList",             defaultValue: "Whoopie cushion, Easter egg, Tragedy, Comedy",                     requiresRestart: false, GetSellListDescription());

        // Terminal Settings
        OverrideWelcomeMessageCfg = ConfigHelper.Bind("Terminal Settings", "OverrideWelcomeMessage", defaultValue: true, requiresRestart: false, "Overrides the terminal welcome message to add additional info.");
        OverrideHelpMessageCfg =    ConfigHelper.Bind("Terminal Settings", "OverrideHelpMessage",    defaultValue: true, requiresRestart: false, "Overrides the terminal help message to add additional info.");
        ShowFoundItemsCfg =         ConfigHelper.Bind("Terminal Settings", "ShowFoundItems",         defaultValue: true, requiresRestart: false, "Show found items on the confirmation screen.");
        SortFoundItemsPriceCfg =    ConfigHelper.Bind("Terminal Settings", "SortFoundItemsPrice",    defaultValue: true, requiresRestart: false, "Sorts found items from most to least expensive.");
        AlignFoundItemsPriceCfg =   ConfigHelper.Bind("Terminal Settings", "AlignFoundItemsPrice",   defaultValue: true, requiresRestart: false, "Align all prices of found items.");

        // Misc Settings
        SpeakInShipCfg =         ConfigHelper.Bind("Misc Settings", "SpeakInShip",         defaultValue: true, requiresRestart: false, "The Company will speak inside your ship after selling from the terminal.");
        RareVoiceLineChanceCfg = ConfigHelper.Bind("Misc Settings", "RareVoiceLineChance", defaultValue: 5f,   requiresRestart: false, "The percent chance the Company will say a rare microphone voice line after selling.");
        ShowQuotaWarningCfg =    ConfigHelper.Bind("Misc Settings", "ShowQuotaWarning",    defaultValue: true, requiresRestart: false, "If enabled, will show a warning when you try to pull the ship's lever when the quota hasn't been fulfilled at the Company building with 0 days left.");

        // Scrap Eater Settings
        ScrapEaterChanceCfg =      ConfigHelper.Bind("Scrap Eater Settings", "ScrapEaterChance",      defaultValue: 75, requiresRestart: false, "The percent chance a scrap eater will spawn?!",                  new AcceptableValueRange<int>(0, 100));
        OctolarSpawnWeightCfg =    ConfigHelper.Bind("Scrap Eater Settings", "OctolarSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Octolar will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        TakeySpawnWeightCfg =      ConfigHelper.Bind("Scrap Eater Settings", "TakeySpawnWeight",      defaultValue: 1,  requiresRestart: false, "The spawn chance weight Takey will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
        MaxwellSpawnWeightCfg =    ConfigHelper.Bind("Scrap Eater Settings", "MaxwellSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Maxwell will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        YippeeSpawnWeightCfg =     ConfigHelper.Bind("Scrap Eater Settings", "YippeeSpawnWeight",     defaultValue: 1,  requiresRestart: false, "The spawn chance weight Yippee will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        CookieFumoSpawnWeightCfg = ConfigHelper.Bind("Scrap Eater Settings", "CookieFumoSpawnWeight", defaultValue: 1,  requiresRestart: false, "The spawn chance weight Cookie Fumo will spawn?! (scrap eater)", new AcceptableValueRange<int>(0, 100));
        PsychoSpawnWeightCfg =     ConfigHelper.Bind("Scrap Eater Settings", "PsychoSpawnWeight",     defaultValue: 1,  requiresRestart: false, "The spawn chance weight Psycho will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        ZombiesSpawnWeightCfg =    ConfigHelper.Bind("Scrap Eater Settings", "ZombiesSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Zombies will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        WolfySpawnWeightCfg =      ConfigHelper.Bind("Scrap Eater Settings", "WolfySpawnWeight",      defaultValue: 1,  requiresRestart: false, "The spawn chance weight Wolfy will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
    }

    private string GetPrioritySellListDescription()
    {
        string message = "Array of item names to prioritize when selling.\n";
        message += "Use the `edit config` command to easily edit the `PrioritySellList` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Each entry should be separated by a comma.\n";
        message += "Item names are not case-sensitive but, spaces do matter.";

        return message;
    }

    private string GetDontSellListDescription()
    {
        string message = "Array of item names to not sell.\n";
        message += "Use the `edit config` command to easily edit the `DontSellList` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Each entry should be separated by a comma.\n";
        message += "Item names are not case-sensitive but, spaces do matter.";

        return message;
    }

    private string GetSellListDescription()
    {
        string message = "Array of item names to sell when using the `sell list` command.\n";
        message += "Use the `edit config` command to easily edit the `SellList` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Each entry should be separated by a comma.\n";
        message += "Item names are not case-sensitive but, spaces do matter.";

        return message;
    }

    private void MigrateOldConfigSettings()
    {
        foreach (var entry in ConfigHelper.GetOrphanedConfigEntries())
        {
            MigrateOldConfigSetting(entry.Key.Section, entry.Key.Key, entry.Value);
        }
    }

    private void MigrateOldConfigSetting(string section, string key, string value)
    {
        if (section == "Sell Settings")
        {
            switch (key)
            {
                case "sellGifts":
                    ConfigHelper.SetConfigEntryValue(SellGiftsCfg, value);
                    return;
                case "sellShotguns":
                    ConfigHelper.SetConfigEntryValue(SellShotgunsCfg, value);
                    return;
                case "sellAmmo":
                    ConfigHelper.SetConfigEntryValue(SellAmmoCfg, value);
                    return;
                case "sellKnives":
                    ConfigHelper.SetConfigEntryValue(SellKnivesCfg, value);
                    return;
                case "sellPickles":
                    ConfigHelper.SetConfigEntryValue(SellPicklesCfg, value);
                    return;
            }
        }

        if (section == "Advanced Sell Settings")
        {
            switch (key)
            {
                case "sellScrapWorthZero":
                    ConfigHelper.SetConfigEntryValue(SellScrapWorthZeroCfg, value);
                    return;
                case "onlySellScrapOnFloor":
                    ConfigHelper.SetConfigEntryValue(OnlySellScrapOnFloorCfg, value);
                    return;
            }

            try
            {
                if (key.Equals("DontSellListJson", System.StringComparison.OrdinalIgnoreCase))
                {
                    ConfigHelper.SetConfigEntryValue(DontSellListCfg, string.Join(", ", JsonConvert.DeserializeObject<string[]>(value.Replace("\\", ""))));
                }

                if (key.Equals("SellListJson", System.StringComparison.OrdinalIgnoreCase))
                {
                    ConfigHelper.SetConfigEntryValue(SellListCfg, string.Join(", ", JsonConvert.DeserializeObject<string[]>(value.Replace("\\", ""))));
                }
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        if (section == "Terminal Settings")
        {
            switch (key)
            {
                case "overrideWelcomeMessage":
                    ConfigHelper.SetConfigEntryValue(OverrideWelcomeMessageCfg, value);
                    return;
                case "overrideHelpMessage":
                    ConfigHelper.SetConfigEntryValue(OverrideHelpMessageCfg, value);
                    return;
                case "showFoundItems":
                    ConfigHelper.SetConfigEntryValue(ShowFoundItemsCfg, value);
                    return;
                case "sortFoundItemsPrice":
                    ConfigHelper.SetConfigEntryValue(SortFoundItemsPriceCfg, value);
                    return;
                case "alignFoundItemsPrice":
                    ConfigHelper.SetConfigEntryValue(AlignFoundItemsPriceCfg, value);
                    return;
            }
        }

        if (section == "Misc Settings")
        {
            switch (key)
            {
                case "speakInShip":
                    ConfigHelper.SetConfigEntryValue(SpeakInShipCfg, value);
                    return;
                case "rareVoiceLineChance":
                    ConfigHelper.SetConfigEntryValue(RareVoiceLineChanceCfg, value);
                    return;
            }
        }

        if (section == "Scrap Eater Settings")
        {
            switch (key)
            {
                case "scrapEaterChance":
                    ConfigHelper.SetConfigEntryValue(ScrapEaterChanceCfg, value);
                    return;
                case "octolarSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(OctolarSpawnWeightCfg, value);
                    return;
                case "takeySpawnWeight":
                    ConfigHelper.SetConfigEntryValue(TakeySpawnWeightCfg, value);
                    return;
                case "maxwellSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(MaxwellSpawnWeightCfg, value);
                    return;
                case "yippeeSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(YippeeSpawnWeightCfg, value);
                    return;
                case "cookieFumoSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(CookieFumoSpawnWeightCfg, value);
                    return;
                case "psychoSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(PsychoSpawnWeightCfg, value);
                    return;
                case "zombiesSpawnWeight":
                    ConfigHelper.SetConfigEntryValue(ZombiesSpawnWeightCfg, value);
                    return;
            }
        }
    }

    internal void ResetToDefault()
    {
        var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            // Check if the field is a ConfigEntry
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
            {
                // Get the ConfigEntry instance
                var configEntry = field.GetValue(this);

                if (configEntry != null)
                {
                    // Use reflection to access and set the Value property to the DefaultValue
                    var valueProperty = field.FieldType.GetProperty("Value");
                    var defaultValueProperty = field.FieldType.GetProperty("DefaultValue");

                    if (valueProperty != null && defaultValueProperty != null)
                    {
                        // Set Value to DefaultValue
                        var defaultValue = defaultValueProperty.GetValue(configEntry);
                        valueProperty.SetValue(configEntry, defaultValue);
                    }
                }
            }
        }

        Plugin.Logger.LogInfo("Reset all config settings to their default value.");

        SyncedConfigsChanged();
    }

    internal void TrySetCustomValues()
    {
        if (SteamUtils.IsLocalClient(PlayerName.Insym)) return;

        TrySetCustomValuesForThorlar();
        TrySetCustomValuesForTakerst();

        // Reset ScrapEaterChance for Insym's modpack if not Insym.

        if (ScrapEaterChance != 0) return;

        if (DontSellList.Length == 1 && DontSellList[0].Equals("gold bar", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!ModpackSaveSystem.ReadValue("ResetScrapEaterChance", false))
            {
                ScrapEaterChance = (int)ScrapEaterChanceCfg.DefaultValue;

                ModpackSaveSystem.WriteValue("ResetScrapEaterChance", true);
            }
        }
    }

    private void TrySetCustomValuesForThorlar()
    {
        if (!SteamUtils.IsLocalClient(PlayerName.Thorlar)) return;

        if (TakeySpawnWeight == 1 && !ModpackSaveSystem.ReadValue("RemovedTakeyScrapEaterSpawnWeight", false))
        {
            TakeySpawnWeight = 0;

            ModpackSaveSystem.WriteValue("RemovedTakeyScrapEaterSpawnWeight", true);
        }
    }

    private void TrySetCustomValuesForTakerst()
    {
        if (!SteamUtils.IsLocalClient(PlayerName.Takerst)) return;
        
        if (!Utils.ArrayContains(DontSellList, "Smol Takey"))
        {
            List<string> array = DontSellList.ToList();
            array.Add("Smol Takey");
            DontSellListCfg.Value = JsonConvert.SerializeObject(array);
        }

        if (!Utils.ArrayContains(DontSellList, "Takey Box"))
        {
            List<string> array = DontSellList.ToList();
            array.Add("Takey Box");
            DontSellListCfg.Value = JsonConvert.SerializeObject(array);
        }

        if (!Utils.ArrayContains(DontSellList, "Takey Mug"))
        {
            List<string> array = DontSellList.ToList();
            array.Add("Takey Mug");
            DontSellListCfg.Value = JsonConvert.SerializeObject(array);
        }
    }

    internal void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        _hostConfigData = syncedConfigData;
    }

    private void SyncedConfigsChanged()
    {
        if (!NetworkUtils.IsServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
