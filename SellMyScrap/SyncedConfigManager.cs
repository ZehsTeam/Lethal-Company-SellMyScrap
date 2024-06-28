using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using Newtonsoft.Json;
using System.Collections.Generic;
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
            return _hostConfigData == null ? SellGiftsCfg.Value : _hostConfigData.sellGifts;
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
            return _hostConfigData == null ? SellShotgunsCfg.Value : _hostConfigData.sellShotguns;
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
            return _hostConfigData == null ? SellAmmoCfg.Value : _hostConfigData.sellAmmo;
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
            return _hostConfigData == null ? SellKnivesCfg.Value : _hostConfigData.sellKnives;
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
            return _hostConfigData == null ? SellPicklesCfg.Value : _hostConfigData.sellPickles;
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
            return _hostConfigData == null ? SellScrapWorthZeroCfg.Value : _hostConfigData.sellScrapWorthZero;
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
            return _hostConfigData == null ? OnlySellScrapOnFloorCfg.Value : _hostConfigData.onlySellScrapOnFloor;
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
                text = _hostConfigData.dontSellListJson;
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
                Plugin.logger.LogError($"Error: failed to deserialize dontSellListJson config setting.\n\n{e}");
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
                Plugin.logger.LogError($"Error: failed to serialize dontSellListJson config setting.\n\n{e}");
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
                text = _hostConfigData.sellListJson;
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
                Plugin.logger.LogError($"Error: failed to deserialize sellListJson config setting.\n\n{e}");
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
                Plugin.logger.LogError($"Error: failed to serialize sellListJson config setting.\n\n{e}");
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
        ClearUnusedEntries();
    }

    private void BindConfigs()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // Sell Settings
        SellGiftsCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellGifts"),
            false,
            new ConfigDescription("Do you want to sell Gifts?")
        );
        SellShotgunsCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellShotguns"),
            false,
            new ConfigDescription("Do you want to sell Shotguns?")
        );
        SellAmmoCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellAmmo"),
            false,
            new ConfigDescription("Do you want to sell Ammo?")
        );
        SellKnivesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellKnives"),
            false,
            new ConfigDescription("Do you want to sell Kitchen knives?")
        );
        SellPicklesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellPickles"),
            true,
            new ConfigDescription("Do you want to sell Jar of pickles?")
        );

        // Advanced Sell Settings
        SellScrapWorthZeroCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "sellScrapWorthZero"),
            false,
            new ConfigDescription("Do you want to sell scrap worth zero?")
        );
        OnlySellScrapOnFloorCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "onlySellScrapOnFloor"),
            false,
            new ConfigDescription("Do you want to sell scrap that is only on the floor?")
        );
        DontSellListJsonCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            JsonConvert.SerializeObject(new string[0]),
            new ConfigDescription(GetDontSellListJsonDescription())
        );
        SellListJsonCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "sellListJson"),
            JsonConvert.SerializeObject(new string[] { "Whoopie cushion", "Easter egg", "Tragedy", "Comedy" }),
            new ConfigDescription(GetSellListJsonDescription())
        );

        // Terminal Settings
        OverrideWelcomeMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "overrideWelcomeMessage"),
            true,
            new ConfigDescription("Overrides the terminal welcome message to add additional info.")
        );
        OverrideHelpMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "overrideHelpMessage"),
            true,
            new ConfigDescription("Overrides the terminal help message to add additional info.")
        );
        ShowFoundItemsCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "showFoundItems"),
            true,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        SortFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "sortFoundItemsPrice"),
            true,
            new ConfigDescription("Sorts found items from most to least expensive.")
        );
        AlignFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "alignFoundItemsPrice"),
            true,
            new ConfigDescription("Align all prices of found items.")
        );

        // Misc Settings
        SpeakInShipCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "speakInShip"),
            true,
            new ConfigDescription("The Company will speak inside your ship after selling from the terminal.")
        );
        RareVoiceLineChanceCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "rareVoiceLineChance"),
            5f,
            new ConfigDescription("The percent chance the Company will say a rare microphone voice line after selling.")
        );

        // Scrap Eater Settings
        ScrapEaterChanceCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "scrapEaterChance"),
            75,
            new ConfigDescription("The percent chance a scrap eater will spawn?!",
            new AcceptableValueRange<int>(0, 100))
        );
        OctolarSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "octolarSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Octolar will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        TakeySpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "takeySpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Takey will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        MaxwellSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "maxwellSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Maxwell will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        YippeeSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "yippeeSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Yippee will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        CookieFumoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "cookieFumoSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Cookie Fumo will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        PsychoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "psychoSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Psycho will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        ZombiesSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "zombiesSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Zombies will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );

        UpdateScrapEaterChance();
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

    private void UpdateScrapEaterChance()
    {
        int targetValue = (int)ScrapEaterChanceCfg.DefaultValue;

        if (ScrapEaterChance <= 0) return;

        if (!SaveSystem.SetScrapEaterChance)
        {
            if (ScrapEaterChance >= targetValue)
            {
                SaveSystem.SetScrapEaterChance = true;
                return;
            }

            ScrapEaterChance = targetValue;
            SaveSystem.SetScrapEaterChance = true;
        }
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
        DontSellListJsonCfg.Value = JsonConvert.SerializeObject(new string[0]);
        SellListJsonCfg.Value = JsonConvert.SerializeObject(new string[] { "Whoopie cushion", "Easter egg", "Tragedy", "Comedy" });

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

    private void ClearUnusedEntries()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
        configFile.Save(); // Save the config file to save these changes
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
