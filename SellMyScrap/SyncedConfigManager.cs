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
    #endregion

    #region Config Setting Get/Set Properties
    // General Settings
    internal bool ExtendedLogging { get { return ExtendedLoggingCfg.Value; } set => ExtendedLoggingCfg.Value = value; }

    // Sell Settings (Synced)
    internal bool SellGifts
    { 
        get
        {
            return _hostConfigData == null ? SellGiftsCfg.Value : _hostConfigData.SellGifts;
        }
        set
        {
            SellGiftsCfg.Value = value;
            SyncedConfigsChanged();
        }
    }
    
    internal bool SellShotguns
    { 
        get
        {
            return _hostConfigData == null ? SellShotgunsCfg.Value : _hostConfigData.SellShotguns;
        }
        set
        {
            SellShotgunsCfg.Value = value;
            SyncedConfigsChanged();
        }
    }
    
    internal bool SellAmmo 
    {
        get 
        {
            return _hostConfigData == null ? SellAmmoCfg.Value : _hostConfigData.SellAmmo;
        }
        set
        {
            SellAmmoCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool SellKnives
    {
        get
        {
            return _hostConfigData == null ? SellKnivesCfg.Value : _hostConfigData.SellKnives;
        }
        set
        {
            SellKnivesCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool SellPickles
    { 
        get
        {
            return _hostConfigData == null ? SellPicklesCfg.Value : _hostConfigData.SellPickles;
        }
        set
        {
            SellPicklesCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    // Advanced Sell Settings (Synced)
    internal bool SellScrapWorthZero
    {
        get
        {
            return _hostConfigData == null ? SellScrapWorthZeroCfg.Value : _hostConfigData.SellScrapWorthZero;
        }
        set
        {
            SellScrapWorthZeroCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal bool OnlySellScrapOnFloor
    {
        get
        {
            return _hostConfigData == null ? OnlySellScrapOnFloorCfg.Value : _hostConfigData.OnlySellScrapOnFloor;
        }
        set
        {
            OnlySellScrapOnFloorCfg.Value = value;
            SyncedConfigsChanged();
        }
    }

    internal string[] DontSellList
    { 
        get
        {
            string text = DontSellListCfg.Value;

            if (_hostConfigData != null)
            {
                text = _hostConfigData.DontSellList;
            }

            if (string.IsNullOrEmpty(text))
            {
                return [];
            }

            try
            {
                return JsonConvert.DeserializeObject<string[]>(text);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to deserialize dontSellList config setting.\n\n{e}");
                return [];
            }
        }
        set
        {
            try
            {
                DontSellListCfg.Value = JsonConvert.SerializeObject(value);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to serialize dontSellList config setting.\n\n{e}");
                DontSellListCfg.Value = JsonConvert.SerializeObject(new string[0]);
            }
            
            SyncedConfigsChanged();
        }
    }

    internal string[] SellList
    {
        get
        {
            string text = SellListCfg.Value;

            if (_hostConfigData != null)
            {
                text = _hostConfigData.SellList;
            }

            if (string.IsNullOrEmpty(text))
            {
                return [];
            }

            try
            {
                return JsonConvert.DeserializeObject<string[]>(text);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to deserialize sellList config setting.\n\n{e}");
                return [];
            }
        }
        set
        {
            try
            {
                SellListCfg.Value = JsonConvert.SerializeObject(value);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to serialize sellList config setting.\n\n{e}");
                SellListCfg.Value = JsonConvert.SerializeObject(new string[0]);
            }

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
    #endregion

    public SyncedConfigManager()
    {
        BindConfigs();
        MigrateOldConfigSettings();
        ClearUnusedEntries();
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
        SellScrapWorthZeroCfg =   ConfigHelper.Bind("Advanced Sell Settings", "SellScrapWorthZero",   defaultValue: false, requiresRestart: false, "Do you want to sell scrap worth zero?");
        OnlySellScrapOnFloorCfg = ConfigHelper.Bind("Advanced Sell Settings", "OnlySellScrapOnFloor", defaultValue: false, requiresRestart: false, "Do you want to sell scrap that is only on the floor?");
        DontSellListCfg =         ConfigHelper.Bind("Advanced Sell Settings", "DontSellList",         defaultValue: JsonConvert.SerializeObject(new string[0]),                                                         requiresRestart: false, GetDontSellListDescription());
        SellListCfg =             ConfigHelper.Bind("Advanced Sell Settings", "SellList",             defaultValue: JsonConvert.SerializeObject(new string[] { "Whoopie cushion", "Easter egg", "Tragedy", "Comedy" }), requiresRestart: false, GetSellListDescription());

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
    }

    private string GetDontSellListDescription()
    {
        string message = "Array of item names to not sell.\n";
        message += "Use the `edit config` command to easily edit the `dontSellList` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Each entry should be separated by a comma\n";
        message += "Item names are not case-sensitive but, spaces do matter.\n";
        message += "Example value: [\"Maxwell\", \"Cookie Fumo\", \"Blahaj\", \"Octolar Plush\", \"Smol Takey\", \"Dusty Plush\"]";

        return message;
    }

    private string GetSellListDescription()
    {
        string message = "Array of item names to sell when using the `sell list` command.\n";
        message += "Use the `edit config` command to easily edit the `sellList` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Each entry should be separated by a comma\n";
        message += "Item names are not case-sensitive but, spaces do matter.\n";
        message += "Example value: [\"Whoopie cushion\", \"Easter egg\", \"Tragedy\", \"Comedy\"]";

        return message;
    }

    private void MigrateOldConfigSettings()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);

        foreach (var entry in orphanedEntries)
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
                    SellGiftsCfg.Value = bool.Parse(value);
                    break;
                case "sellShotguns":
                    SellShotgunsCfg.Value = bool.Parse(value);
                    break;
                case "sellAmmo":
                    SellAmmoCfg.Value = bool.Parse(value);
                    break;
                case "sellKnives":
                    SellKnivesCfg.Value = bool.Parse(value);
                    break;
                case "sellPickles":
                    SellPicklesCfg.Value = bool.Parse(value);
                    break;
            }
        }

        if (section == "Advanced Sell Settings")
        {
            switch (key)
            {
                case "sellScrapWorthZero":
                    SellScrapWorthZeroCfg.Value = bool.Parse(value);
                    break;
                case "onlySellScrapOnFloor":
                    OnlySellScrapOnFloorCfg.Value = bool.Parse(value);
                    break;
                case "dontSellListJson":
                    DontSellListCfg.Value = value.Replace("\\", string.Empty);
                    break;
                case "sellListJson":
                    SellListCfg.Value = value.Replace("\\", string.Empty);
                    break;
                case "DontSellListJson":
                    DontSellListCfg.Value = value.Replace("\\", string.Empty);
                    break;
                case "SellListJson":
                    SellListCfg.Value = value.Replace("\\", string.Empty);
                    break;
            }
        }

        if (section == "Terminal Settings")
        {
            switch (key)
            {
                case "overrideWelcomeMessage":
                    OverrideWelcomeMessageCfg.Value = bool.Parse(value);
                    break;
                case "overrideHelpMessage":
                    OverrideHelpMessageCfg.Value = bool.Parse(value);
                    break;
                case "showFoundItems":
                    ShowFoundItemsCfg.Value = bool.Parse(value);
                    break;
                case "sortFoundItemsPrice":
                    SortFoundItemsPriceCfg.Value = bool.Parse(value);
                    break;
                case "alignFoundItemsPrice":
                    AlignFoundItemsPriceCfg.Value = bool.Parse(value);
                    break;
            }
        }

        if (section == "Misc Settings")
        {
            switch (key)
            {
                case "speakInShip":
                    SpeakInShipCfg.Value = bool.Parse(value);
                    break;
                case "rareVoiceLineChance":
                    RareVoiceLineChanceCfg.Value = float.Parse(value);
                    break;
            }
        }

        if (section == "Scrap Eater Settings")
        {
            switch (key)
            {
                case "scrapEaterChance":
                    ScrapEaterChanceCfg.Value = int.Parse(value);
                    break;
                case "octolarSpawnWeight":
                    OctolarSpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "takeySpawnWeight":
                    TakeySpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "maxwellSpawnWeight":
                    MaxwellSpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "yippeeSpawnWeight":
                    YippeeSpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "cookieFumoSpawnWeight":
                    CookieFumoSpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "psychoSpawnWeight":
                    PsychoSpawnWeightCfg.Value = int.Parse(value);
                    break;
                case "zombiesSpawnWeight":
                    ZombiesSpawnWeightCfg.Value = int.Parse(value);
                    break;
            }
        }
    }

    private void ClearUnusedEntries()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
        configFile.Save(); // Save the config file to save these changes
    }

    internal void ResetToDefault()
    {
        // Sell Settings
        SellGiftsCfg.Value = (bool)SellGiftsCfg.DefaultValue;
        SellShotgunsCfg.Value = (bool)SellShotgunsCfg.DefaultValue;
        SellAmmoCfg.Value = (bool)SellAmmoCfg.DefaultValue;
        SellKnivesCfg.Value = (bool)SellKnivesCfg.DefaultValue;
        SellPicklesCfg.Value = (bool)SellPicklesCfg.DefaultValue;

        // Advanced Sell Settings
        SellScrapWorthZeroCfg.Value = (bool)SellScrapWorthZeroCfg.DefaultValue;
        OnlySellScrapOnFloorCfg.Value = (bool)OnlySellScrapOnFloorCfg.DefaultValue;
        DontSellListCfg.Value = (string)DontSellListCfg.DefaultValue;
        SellListCfg.Value = (string)SellListCfg.DefaultValue;

        // Terminal Settings
        OverrideWelcomeMessageCfg.Value = (bool)OverrideWelcomeMessageCfg.DefaultValue;
        OverrideHelpMessageCfg.Value = (bool)OverrideHelpMessageCfg.DefaultValue;
        ShowFoundItemsCfg.Value = (bool)ShowFoundItemsCfg.DefaultValue;
        SortFoundItemsPriceCfg.Value = (bool)SortFoundItemsPriceCfg.DefaultValue;
        AlignFoundItemsPriceCfg.Value = (bool)AlignFoundItemsPriceCfg.DefaultValue;

        // Misc Settings
        SpeakInShipCfg.Value = (bool)SpeakInShipCfg.DefaultValue;
        RareVoiceLineChanceCfg.Value = (float)RareVoiceLineChanceCfg.DefaultValue;
        ShowQuotaWarningCfg.Value = (bool)ShowQuotaWarningCfg.DefaultValue;

        // Scrap Eater Settings
        ScrapEaterChanceCfg.Value = (int)ScrapEaterChanceCfg.DefaultValue;
        OctolarSpawnWeightCfg.Value = (int)OctolarSpawnWeightCfg.DefaultValue;
        TakeySpawnWeightCfg.Value = (int)TakeySpawnWeightCfg.DefaultValue;
        MaxwellSpawnWeightCfg.Value = (int)MaxwellSpawnWeightCfg.DefaultValue;
        YippeeSpawnWeightCfg.Value = (int)YippeeSpawnWeightCfg.DefaultValue;
        CookieFumoSpawnWeightCfg.Value = (int)CookieFumoSpawnWeightCfg.DefaultValue;
        PsychoSpawnWeightCfg.Value = (int)PsychoSpawnWeightCfg.DefaultValue;
        ZombiesSpawnWeightCfg.Value = (int)ZombiesSpawnWeightCfg.DefaultValue;

        Plugin.logger.LogInfo("Reset all config settings to their default value.");

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
