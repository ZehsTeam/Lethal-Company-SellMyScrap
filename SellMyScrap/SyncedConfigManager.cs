using BepInEx.Configuration;
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
    // Sell Settings (Synced)
    private ConfigEntry<bool> SellGiftsCfg;
    private ConfigEntry<bool> SellShotgunsCfg;
    private ConfigEntry<bool> SellAmmoCfg;
    private ConfigEntry<bool> SellKnivesCfg;
    private ConfigEntry<bool> SellPicklesCfg;

    // Advanced Sell Settings (Synced)
    private ConfigEntry<bool> SellScrapWorthZeroCfg;
    private ConfigEntry<bool> OnlySellScrapOnFloorCfg;
    private ConfigEntry<string> DontSellListJsonCfg;
    private ConfigEntry<string> SellListJsonCfg;

    // Terminal Settings
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<bool> SortFoundItemsPriceCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;
    private ConfigEntry<float> RareVoiceLineChanceCfg;

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

    internal string[] DontSellListJson
    { 
        get
        {
            string text = DontSellListJsonCfg.Value;

            if (_hostConfigData != null)
            {
                text = _hostConfigData.DontSellListJson;
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
                Plugin.logger.LogError($"Failed to deserialize dontSellListJson config setting.\n\n{e}");
                return [];
            }
        }
        set
        {
            try
            {
                DontSellListJsonCfg.Value = JsonConvert.SerializeObject(value);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to serialize dontSellListJson config setting.\n\n{e}");
                DontSellListJsonCfg.Value = JsonConvert.SerializeObject(new string[0]);
            }
            
            SyncedConfigsChanged();
        }
    }

    internal string[] SellListJson
    {
        get
        {
            string text = SellListJsonCfg.Value;

            if (_hostConfigData != null)
            {
                text = _hostConfigData.SellListJson;
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
                Plugin.logger.LogError($"Failed to deserialize sellListJson config setting.\n\n{e}");
                return [];
            }
        }
        set
        {
            try
            {
                SellListJsonCfg.Value = JsonConvert.SerializeObject(value);
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError($"Failed to serialize sellListJson config setting.\n\n{e}");
                SellListJsonCfg.Value = JsonConvert.SerializeObject(new string[0]);
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
        ConfigFile configFile = Plugin.Instance.Config;

        // Sell Settings
        SellGiftsCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "SellGifts"),
            false,
            new ConfigDescription("Do you want to sell Gifts?")
        );
        SellShotgunsCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "SellShotguns"),
            false,
            new ConfigDescription("Do you want to sell Shotguns?")
        );
        SellAmmoCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "SellAmmo"),
            false,
            new ConfigDescription("Do you want to sell Ammo?")
        );
        SellKnivesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "SellKnives"),
            false,
            new ConfigDescription("Do you want to sell Kitchen knives?")
        );
        SellPicklesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "SellPickles"),
            true,
            new ConfigDescription("Do you want to sell Jar of pickles?")
        );

        // Advanced Sell Settings
        SellScrapWorthZeroCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "SellScrapWorthZero"),
            false,
            new ConfigDescription("Do you want to sell scrap worth zero?")
        );
        OnlySellScrapOnFloorCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "OnlySellScrapOnFloor"),
            false,
            new ConfigDescription("Do you want to sell scrap that is only on the floor?")
        );
        DontSellListJsonCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "DontSellListJson"),
            JsonConvert.SerializeObject(new string[0]),
            new ConfigDescription(GetDontSellListJsonDescription())
        );
        SellListJsonCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "SellListJson"),
            JsonConvert.SerializeObject(new string[] { "Whoopie cushion", "Easter egg", "Tragedy", "Comedy" }),
            new ConfigDescription(GetSellListJsonDescription())
        );

        // Terminal Settings
        OverrideWelcomeMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "OverrideWelcomeMessage"),
            true,
            new ConfigDescription("Overrides the terminal welcome message to add additional info.")
        );
        OverrideHelpMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "OverrideHelpMessage"),
            true,
            new ConfigDescription("Overrides the terminal help message to add additional info.")
        );
        ShowFoundItemsCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "ShowFoundItems"),
            true,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        SortFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "SortFoundItemsPrice"),
            true,
            new ConfigDescription("Sorts found items from most to least expensive.")
        );
        AlignFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "AlignFoundItemsPrice"),
            true,
            new ConfigDescription("Align all prices of found items.")
        );

        // Misc Settings
        SpeakInShipCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "SpeakInShip"),
            true,
            new ConfigDescription("The Company will speak inside your ship after selling from the terminal.")
        );
        RareVoiceLineChanceCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "RareVoiceLineChance"),
            5f,
            new ConfigDescription("The percent chance the Company will say a rare microphone voice line after selling.")
        );

        // Scrap Eater Settings
        ScrapEaterChanceCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "ScrapEaterChance"),
            75,
            new ConfigDescription("The percent chance a scrap eater will spawn?!",
            new AcceptableValueRange<int>(0, 100))
        );
        OctolarSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "OctolarSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Octolar will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        TakeySpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "TakeySpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Takey will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        MaxwellSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "MaxwellSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Maxwell will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        YippeeSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "YippeeSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Yippee will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        CookieFumoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "CookieFumoSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Cookie Fumo will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        PsychoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "PsychoSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Psycho will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        ZombiesSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "ZombiesSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Zombies will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
    }

    private string GetDontSellListJsonDescription()
    {
        string message = "JSON array of item names to not sell.\n";
        message += "Use the `edit config` command to easily edit the `dontSellListJson` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Item names are not case-sensitive but, spaces do matter.\n";
        message += "https://www.w3schools.com/js/js_json_arrays.asp\n";
        message += "Example value: [\"Maxwell\", \"Cookie Fumo\", \"Blahaj\", \"Octolar Plush\", \"Smol Takey\", \"Dusty Plush\"]";

        return message;
    }

    private string GetSellListJsonDescription()
    {
        string message = "JSON array of item names to sell when using the `sell list` command.\n";
        message += "Use the `edit config` command to easily edit the `sellListJson` config setting from the terminal.\n";
        message += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        message += "Item names are not case-sensitive but, spaces do matter.\n";
        message += "https://www.w3schools.com/js/js_json_arrays.asp\n";

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
                    DontSellListJsonCfg.Value = value.Replace("\\", string.Empty);
                    break;
                case "sellListJson":
                    SellListJsonCfg.Value = value.Replace("\\", string.Empty);
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
        DontSellListJsonCfg.Value = (string)DontSellListJsonCfg.DefaultValue;
        SellListJsonCfg.Value = (string)SellListJsonCfg.DefaultValue;

        // Terminal Settings
        OverrideWelcomeMessageCfg.Value = (bool)OverrideWelcomeMessageCfg.DefaultValue;
        OverrideHelpMessageCfg.Value = (bool)OverrideHelpMessageCfg.DefaultValue;
        ShowFoundItemsCfg.Value = (bool)ShowFoundItemsCfg.DefaultValue;
        SortFoundItemsPriceCfg.Value = (bool)SortFoundItemsPriceCfg.DefaultValue;
        AlignFoundItemsPriceCfg.Value = (bool)AlignFoundItemsPriceCfg.DefaultValue;

        // Misc Settings
        SpeakInShipCfg.Value = (bool)SpeakInShipCfg.DefaultValue;
        RareVoiceLineChanceCfg.Value = (float)RareVoiceLineChanceCfg.DefaultValue;

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
        if (SteamUtils.IsLocalPlayerInsym()) return;

        TrySetCustomValuesForThorlar();
        TrySetCustomValuesForTakerst();

        // Reset ScrapEaterChance for Insym's modpack if not Insym.

        if (ScrapEaterChance != 0) return;

        if (DontSellListJson.Length == 1 && DontSellListJson[0].Equals("gold bar", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!(bool)ModpackSaveSystem.ReadValue("ResetScrapEaterChance", false))
            {
                ScrapEaterChance = (int)ScrapEaterChanceCfg.DefaultValue;

                ModpackSaveSystem.WriteValue("ResetScrapEaterChance", true);
            }
        }
    }

    private void TrySetCustomValuesForThorlar()
    {
        if (!SteamUtils.IsLocalPlayerThorlar()) return;

        if (TakeySpawnWeight == 1 && !(bool)ModpackSaveSystem.ReadValue("RemovedTakeyScrapEaterSpawnWeight", false))
        {
            TakeySpawnWeight = 0;

            ModpackSaveSystem.WriteValue("RemovedTakeyScrapEaterSpawnWeight", true);
        }
    }

    private void TrySetCustomValuesForTakerst()
    {
        if (!SteamUtils.IsLocalPlayerTakerst()) return;
        
        if (!Utils.ArrayContains(DontSellListJson, "Smol Takey"))
        {
            List<string> array = DontSellListJson.ToList();
            array.Add("Smol Takey");
            DontSellListJsonCfg.Value = JsonConvert.SerializeObject(array);
        }
    }

    internal void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        _hostConfigData = syncedConfigData;
    }

    private void SyncedConfigsChanged()
    {
        if (!Plugin.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
