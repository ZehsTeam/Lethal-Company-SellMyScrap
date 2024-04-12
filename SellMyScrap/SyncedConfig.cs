using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace com.github.zehsteam.SellMyScrap;

public class SyncedConfig
{
    #region Config Setting Default Values
    // Sell Settings Defaults
    internal static bool sellGiftsDefault = false;
    internal static bool sellShotgunsDefault = false;
    internal static bool sellAmmoDefault = false;
    internal static bool sellKnivesDefault = false;
    internal static bool sellPicklesDefault = true;

    // Advanced Sell Settings Defaults
    internal static bool sellScrapWorthZeroDefault = false;
    internal static bool onlySellScrapOnFloorDefault = false;
    internal static string[] dontSellListJsonDefault = [];

    // Terminal Settings Defaults
    internal static bool overrideWelcomeMessageDefault = true;
    internal static bool overrideHelpMessageDefault = true;
    internal static bool showFoundItemsDefault = true;
    internal static bool sortFoundItemsPriceDefault = true;
    internal static bool alignFoundItemsPriceDefault = true;

    // Misc Settings Defaults
    internal static bool speakInShipDefault = true;
    internal static bool overrideSetNewProfitQuotaDefault = true;

    // Scrap Eater Settings Defaults
    internal static int ScrapEaterChanceDefault = 40;
    internal static int OctolarSpawnWeightDefault = 1;
    internal static int TakeySpawnWeightDefault = 1;
    internal static int MaxwellSpawnWeightDefault = 1;
    internal static int YippeeSpawnWeightDefault = 1;
    internal static int CookieFumoSpawnWeightDefault = 1;
    internal static int PsychoSpawnWeightDefault = 1;
    #endregion

    private SyncedConfigData hostConfigData;

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

    // Terminal Settings
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<bool> SortFoundItemsPriceCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;
    private ConfigEntry<bool> OverrideSetNewProfitQuotaCfg;

    // Scrap Eater Settings
    private ConfigEntry<int> ScrapEaterChanceCfg;
    private ConfigEntry<int> OctolarSpawnWeightCfg;
    private ConfigEntry<int> TakeySpawnWeightCfg;
    private ConfigEntry<int> MaxwellSpawnWeightCfg;
    private ConfigEntry<int> YippeeSpawnWeightCfg;
    private ConfigEntry<int> CookieFumoSpawnWeightCfg;
    private ConfigEntry<int> PsychoSpawnWeightCfg;
    #endregion

    #region Config Setting Get/Set Properties
    // Sell Settings (Synced)
    internal bool SellGifts
    { 
        get
        {
            return hostConfigData == null ? SellGiftsCfg.Value : hostConfigData.sellGifts;
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
            return hostConfigData == null ? SellShotgunsCfg.Value : hostConfigData.sellShotguns;
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
            return hostConfigData == null ? SellAmmoCfg.Value : hostConfigData.sellAmmo;
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
            return hostConfigData == null ? SellKnivesCfg.Value : hostConfigData.sellKnives;
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
            return hostConfigData == null ? SellPicklesCfg.Value : hostConfigData.sellPickles;
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
            return hostConfigData == null ? SellScrapWorthZeroCfg.Value : hostConfigData.sellScrapWorthZero;
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
            return hostConfigData == null ? OnlySellScrapOnFloorCfg.Value : hostConfigData.onlySellScrapOnFloor;
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

            if (hostConfigData != null)
            {
                text = hostConfigData.dontSellListJson;
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

    // Terminal Settings
    internal bool OverrideWelcomeMessage { get { return OverrideWelcomeMessageCfg.Value; } set => OverrideWelcomeMessageCfg.Value = value; }
    internal bool OverrideHelpMessage { get { return OverrideHelpMessageCfg.Value; } set => OverrideHelpMessageCfg.Value = value; }
    internal bool ShowFoundItems { get { return ShowFoundItemsCfg.Value; } set => ShowFoundItemsCfg.Value = value; }
    internal bool SortFoundItemsPrice { get { return SortFoundItemsPriceCfg.Value; } set => SortFoundItemsPriceCfg.Value = value; }
    internal bool AlignFoundItemsPrice { get { return AlignFoundItemsPriceCfg.Value; } set => AlignFoundItemsPriceCfg.Value = value; }

    // Misc Settings
    internal bool SpeakInShip { get { return SpeakInShipCfg.Value; } set => SpeakInShipCfg.Value = value; }
    internal bool OverrideSetNewProfitQuota { get { return OverrideSetNewProfitQuotaCfg.Value; } set => OverrideSetNewProfitQuotaCfg.Value = value; }

    // Scrap Eaters
    internal int ScrapEaterChance { get { return ScrapEaterChanceCfg.Value; } set => ScrapEaterChanceCfg.Value = value; }
    internal int OctolarSpawnWeight { get { return OctolarSpawnWeightCfg.Value; } set => OctolarSpawnWeightCfg.Value = value; }
    internal int TakeySpawnWeight { get { return TakeySpawnWeightCfg.Value; } set => TakeySpawnWeightCfg.Value = value; }
    internal int MaxwellSpawnWeight { get { return MaxwellSpawnWeightCfg.Value; } set => MaxwellSpawnWeightCfg.Value = value; }
    internal int YippeeSpawnWeight { get { return YippeeSpawnWeightCfg.Value; } set => YippeeSpawnWeightCfg.Value = value; }
    internal int CookieFumoSpawnWeight { get { return CookieFumoSpawnWeightCfg.Value; } set => CookieFumoSpawnWeightCfg.Value = value; }
    internal int PsychoSpawnWeight { get { return PsychoSpawnWeightCfg.Value; } set => PsychoSpawnWeightCfg.Value = value; }
    #endregion

    public SyncedConfig()
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
            sellGiftsDefault,
            new ConfigDescription("Do you want to sell Gifts?")
        );
        SellShotgunsCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellShotguns"),
            sellShotgunsDefault,
            new ConfigDescription("Do you want to sell Shotguns?")
        );
        SellAmmoCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellAmmo"),
            sellAmmoDefault,
            new ConfigDescription("Do you want to sell Ammo?")
        );
        SellKnivesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellKnives"),
            sellKnivesDefault,
            new ConfigDescription("Do you want to sell Kitchen knives?")
        );
        SellPicklesCfg = configFile.Bind(
            new ConfigDefinition("Sell Settings", "sellPickles"),
            sellPicklesDefault,
            new ConfigDescription("Do you want to sell Jar of pickles?")
        );

        // Advanced Sell Settings
        SellScrapWorthZeroCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "sellScrapWorthZero"),
            sellScrapWorthZeroDefault,
            new ConfigDescription("Do you want to sell scrap worth zero?")
        );
        OnlySellScrapOnFloorCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "onlySellScrapOnFloor"),
            onlySellScrapOnFloorDefault,
            new ConfigDescription("Do you want to sell scrap that is only on the floor?")
        );

        string dontSellListJsonCfgDescription = "JSON array of item names to not sell.\n";
        dontSellListJsonCfgDescription += "Use the `edit config` command to easily edit the `dontSellListJson` config setting from the terminal.\n";
        dontSellListJsonCfgDescription += "Use the `view scrap` or `view all scrap` command to see the correct item names to use.\n";
        dontSellListJsonCfgDescription += "Item names are not case-sensitive but, spaces do matter.\n";
        dontSellListJsonCfgDescription += "https://www.w3schools.com/js/js_json_arrays.asp\n";
        dontSellListJsonCfgDescription += "Example value: [\"Maxwell\", \"Cookie Fumo\", \"Octolar Plush\", \"Smol Takey\"]";
        DontSellListJsonCfg = configFile.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            JsonConvert.SerializeObject(dontSellListJsonDefault),
            new ConfigDescription(dontSellListJsonCfgDescription)
        );

        // Terminal Settings
        OverrideWelcomeMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "overrideWelcomeMessage"),
            overrideWelcomeMessageDefault,
            new ConfigDescription("Overrides the terminal welcome message to add additional info.")
        );
        OverrideHelpMessageCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "overrideHelpMessage"),
            overrideHelpMessageDefault,
            new ConfigDescription("Overrides the terminal help message to add additional info.")
        );
        ShowFoundItemsCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "showFoundItems"),
            showFoundItemsDefault,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        SortFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "sortFoundItemsPrice"),
            sortFoundItemsPriceDefault,
            new ConfigDescription("Sorts found items from most to least expensive.")
        );
        AlignFoundItemsPriceCfg = configFile.Bind(
            new ConfigDefinition("Terminal Settings", "alignFoundItemsPrice"),
            alignFoundItemsPriceDefault,
            new ConfigDescription("Align all prices of found items.")
        );

        // Misc Settings
        SpeakInShipCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "speakInShip"),
            speakInShipDefault,
            new ConfigDescription("The Company will speak inside your ship after selling from the terminal.")
        );
        OverrideSetNewProfitQuotaCfg = configFile.Bind(
            new ConfigDefinition("Misc Settings", "overrideSetNewProfitQuota"),
            overrideSetNewProfitQuotaDefault,
            new ConfigDescription("Will override the SetNewProfitQuota function in TimeOfDay.")
        );

        // Scrap Eater Settings
        ScrapEaterChanceCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "scrapEaterChance"),
            ScrapEaterChanceDefault,
            new ConfigDescription("The percent chance a scrap eater will spawn?!",
            new AcceptableValueRange<int>(0, 100))
        );
        OctolarSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "octolarSpawnWeight"),
            OctolarSpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Octolar will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        TakeySpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "takeySpawnWeight"),
            TakeySpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Takey will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        MaxwellSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "maxwellSpawnWeight"),
            MaxwellSpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Maxwell will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        YippeeSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "yippeeSpawnWeight"),
            YippeeSpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Yippee will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        CookieFumoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "cookieFumoSpawnWeight"),
            CookieFumoSpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Cookie Fumo will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
        PsychoSpawnWeightCfg = configFile.Bind(
            new ConfigDefinition("Scrap Eater Settings", "psychoSpawnWeight"),
            PsychoSpawnWeightDefault,
            new ConfigDescription("The spawn chance weight Psycho will spawn?! (scrap eater)",
            new AcceptableValueRange<int>(0, 100))
        );
    }

    internal void ResetToDefault()
    {
        // Sell Settings
        SellGiftsCfg.Value = sellGiftsDefault; 
        SellShotgunsCfg.Value = sellShotgunsDefault; 
        SellAmmoCfg.Value = sellAmmoDefault; 
        SellKnivesCfg.Value = sellKnivesDefault; 
        SellPicklesCfg.Value = sellPicklesDefault;

        // Advanced Sell Settings
        SellScrapWorthZeroCfg.Value = sellScrapWorthZeroDefault;
        OnlySellScrapOnFloorCfg.Value = onlySellScrapOnFloorDefault;
        DontSellListJsonCfg.Value = JsonConvert.SerializeObject(dontSellListJsonDefault);

        // Terminal Settings
        OverrideWelcomeMessageCfg.Value = overrideWelcomeMessageDefault;
        OverrideHelpMessageCfg.Value = overrideHelpMessageDefault;
        ShowFoundItemsCfg.Value = showFoundItemsDefault;
        SortFoundItemsPriceCfg.Value = sortFoundItemsPriceDefault;
        AlignFoundItemsPriceCfg.Value = alignFoundItemsPriceDefault;

        // Misc Settings
        SpeakInShipCfg.Value = speakInShipDefault;
        OverrideSetNewProfitQuotaCfg.Value = overrideSetNewProfitQuotaDefault;

        // Scrap Eater Settings
        ScrapEaterChanceCfg.Value = ScrapEaterChanceDefault;
        OctolarSpawnWeightCfg.Value = OctolarSpawnWeightDefault;
        TakeySpawnWeightCfg.Value = TakeySpawnWeightDefault;
        MaxwellSpawnWeightCfg.Value = MaxwellSpawnWeightDefault;
        YippeeSpawnWeightCfg.Value = YippeeSpawnWeightDefault;
        CookieFumoSpawnWeightCfg.Value = CookieFumoSpawnWeightDefault;
        PsychoSpawnWeightCfg.Value = PsychoSpawnWeightDefault;

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
        hostConfigData = syncedConfigData;
    }

    private void SyncedConfigsChanged()
    {
        if (!Plugin.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
