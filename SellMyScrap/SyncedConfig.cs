using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using Newtonsoft.Json;

namespace com.github.zehsteam.SellMyScrap;

public class SyncedConfig
{
    private SyncedConfigData hostConfigData;

    // Sell Settings (Synced)
    private ConfigEntry<bool> SellGiftsCfg;
    private ConfigEntry<bool> SellShotgunsCfg;
    private ConfigEntry<bool> SellAmmoCfg;
    private ConfigEntry<bool> SellPicklesCfg;

    // Advanced Sell Settings (Synced)
    private ConfigEntry<bool> SellScrapWorthZeroCfg;
    private ConfigEntry<bool> OnlySellScrapOnFloorCfg;
    private ConfigEntry<string> DontSellListJsonCfg;

    // Terminal Settings
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<bool> SortFoundItemsPricePriceCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;
    private ConfigEntry<int> ScrapEaterChanceCfg;
    private ConfigEntry<int> OctolarSpawnWeightCfg;
    private ConfigEntry<int> TakeySpawnWeightCfg;

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
            return hostConfigData == null ? JsonConvert.DeserializeObject<string[]>(DontSellListJsonCfg.Value) : JsonConvert.DeserializeObject<string[]>(hostConfigData.dontSellListJson);
        }
        set
        {
            DontSellListJsonCfg.Value = JsonConvert.SerializeObject(value);
            SyncedConfigsChanged();
        }
    }

    // Terminal Settings
    internal bool OverrideWelcomeMessage { get { return OverrideWelcomeMessageCfg.Value; } set => OverrideWelcomeMessageCfg.Value = value; }
    internal bool OverrideHelpMessage { get { return OverrideHelpMessageCfg.Value; } set => OverrideHelpMessageCfg.Value = value; }
    internal bool ShowFoundItems { get { return ShowFoundItemsCfg.Value; } set => ShowFoundItemsCfg.Value = value; }
    internal bool SortFoundItemsPrice { get { return SortFoundItemsPricePriceCfg.Value; } set => SortFoundItemsPricePriceCfg.Value = value; }
    internal bool AlignFoundItemsPrice { get { return AlignFoundItemsPriceCfg.Value; } set => AlignFoundItemsPriceCfg.Value = value; }

    // Misc Settings
    internal bool SpeakInShip { get { return SpeakInShipCfg.Value; } set => SpeakInShipCfg.Value = value; }
    internal int ScrapEaterChance { get { return ScrapEaterChanceCfg.Value; } set => ScrapEaterChanceCfg.Value = value; }
    internal int OctolarSpawnWeight { get { return OctolarSpawnWeightCfg.Value; } set => OctolarSpawnWeightCfg.Value = value; }
    internal int TakeySpawnWeight { get { return TakeySpawnWeightCfg.Value; } set => TakeySpawnWeightCfg.Value = value; }

    public SyncedConfig()
    {
        BindConfigs();
    }

    private void BindConfigs()
    {
        ConfigFile config = SellMyScrapBase.Instance.Config;

        // Sell Settings
        SellGiftsCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "sellGifts"),
            false,
            new ConfigDescription("Do you want to sell Gifts?")
        );
        SellShotgunsCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "sellShotguns"),
            false,
            new ConfigDescription("Do you want to sell Shotguns?")
        );
        SellAmmoCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "sellAmmo"),
            false,
            new ConfigDescription("Do you want to sell Ammo?")
        );
        SellPicklesCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "sellPickles"),
            true,
            new ConfigDescription("Do you want to sell Jar of pickles?")
        );

        // Advanced Sell Settings
        SellScrapWorthZeroCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "sellScrapWorthZero"),
            false,
            new ConfigDescription("Do you want to sell scrap worth zero?")
        );
        OnlySellScrapOnFloorCfg = config.Bind(
            new ConfigDefinition("Sell Settings", "onlySellScrapOnFloor"),
            false,
            new ConfigDescription("Do you want to sell scrap that is only on the floor?")
        );

        string dontSellListJsonCfgDescription = "JSON array of item names to not sell.\n";
        dontSellListJsonCfgDescription += "Item names are not case-sensitive and spaces do matter for item names.\n";
        dontSellListJsonCfgDescription += "https://www.w3schools.com/js/js_json_arrays.asp\n";
        dontSellListJsonCfgDescription += "Example value: [\"Maxwell\", \"Other Scrap Item\"]";
        DontSellListJsonCfg = config.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            "[]",
            new ConfigDescription(dontSellListJsonCfgDescription)
        );

        // Terminal Settings
        OverrideWelcomeMessageCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "overrideWelcomeMessage"),
            true,
            new ConfigDescription("Overrides the terminal welcome message to add additional info.")
        );
        OverrideHelpMessageCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "overrideHelpMessage"),
            true,
            new ConfigDescription("Overrides the terminal help message to add additional info.")
        );
        ShowFoundItemsCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "showFoundItems"),
            true,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        SortFoundItemsPricePriceCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "sortFoundItemsPrice"),
            true,
            new ConfigDescription("Sorts found items from most to least expensive.")
        );
        AlignFoundItemsPriceCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "alignFoundItemsPrice"),
            true,
            new ConfigDescription("Align all prices of found items.")
        );

        // Misc Settings
        SpeakInShipCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "speakInShip"),
            true,
            new ConfigDescription("The Company will speak inside your ship after selling from the terminal.")
        );
        ScrapEaterChanceCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "scrapEaterChance"),
            30,
            new ConfigDescription("The percent chance a scrap eater will spawn?!",
            new AcceptableValueRange<int>(0, 100))
        );
        OctolarSpawnWeightCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "octolarSpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Octolar will spawn?! (ScrapEater)",
            new AcceptableValueRange<int>(0, 100))
        );
        TakeySpawnWeightCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "takeySpawnWeight"),
            1,
            new ConfigDescription("The spawn chance weight Takey will spawn?! (ScrapEater)",
            new AcceptableValueRange<int>(0, 100))
        );
    }

    public void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        hostConfigData = syncedConfigData;
    }

    private void SyncedConfigsChanged()
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
