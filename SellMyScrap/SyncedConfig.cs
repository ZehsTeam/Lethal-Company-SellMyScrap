using BepInEx.Configuration;
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
    private ConfigEntry<string> DontSellListJsonCfg;

    // Confirmation Settings
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<int> ShowFoundItemsLimitCfg;
    private ConfigEntry<bool> SortFoundItemsCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;

    // Sell Settings (Synced)
    internal bool SellGifts
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellGifts;

            return SellGiftsCfg.Value;
        } 
        set => SellGiftsCfg.Value = value;
    }
    
    internal bool SellShotguns
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellShotguns;

            return SellShotgunsCfg.Value;
        } 
        set => SellShotgunsCfg.Value = value;
    }
    
    internal bool SellAmmo 
    {
        get 
        {
            if (hostConfigData != null) return hostConfigData.sellAmmo;

            return SellAmmoCfg.Value;
        }
        set => SellAmmoCfg.Value = value;
    }
    
    internal bool SellPickles
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellPickles;
            
            return SellPicklesCfg.Value;
        }
        set => SellPicklesCfg.Value = value;
    }

    // Advanced Sell Settings (Synced)
    internal string[] DontSellListJson
    { 
        get
        {
            if (hostConfigData != null) return JsonConvert.DeserializeObject<string[]>(hostConfigData.dontSellListJson);

            return JsonConvert.DeserializeObject<string[]>(DontSellListJsonCfg.Value);
        }
        set
        {
            DontSellListJsonCfg.Value = JsonConvert.SerializeObject(value);
        }
    }

    // Confirmation Settings
    internal bool ShowFoundItems { get { return ShowFoundItemsCfg.Value; } set => ShowFoundItemsCfg.Value = value; }
    internal int ShowFoundItemsLimit { get { return ShowFoundItemsLimitCfg.Value; } set => ShowFoundItemsLimitCfg.Value = value; }
    internal bool SortFoundItems { get { return SortFoundItemsCfg.Value; } set => SortFoundItemsCfg.Value = value; }
    internal bool AlignFoundItemsPrice { get { return AlignFoundItemsPriceCfg.Value; } set => AlignFoundItemsPriceCfg.Value = value; }

    // Misc Settings
    internal bool SpeakInShip { get { return SpeakInShipCfg.Value; } set => SpeakInShipCfg.Value = value; }
    internal bool OverrideWelcomeMessage { get { return OverrideWelcomeMessageCfg.Value; } set => OverrideWelcomeMessageCfg.Value = value; }
    internal bool OverrideHelpMessage { get { return OverrideHelpMessageCfg.Value; } set => OverrideHelpMessageCfg.Value = value; }

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
        string dontSellListJsonCfgDescription = "JSON array of item names to not sell.\n";
        dontSellListJsonCfgDescription += "Item names are not case-sensitive.\n";
        dontSellListJsonCfgDescription += "https://www.w3schools.com/js/js_json_arrays.asp\n\n";
        dontSellListJsonCfgDescription += "Example: [\"Gift\", \"Shotgun\", \"Ammo\"]\n";
        DontSellListJsonCfg = config.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            "[]",
            new ConfigDescription(dontSellListJsonCfgDescription)
        );

        SellMyScrapBase.Instance.UpdateCachedDontSellList(DontSellListJson);

        // Confirmation Settings
        ShowFoundItemsCfg = config.Bind(
            new ConfigDefinition("Confirmation Settings", "showFoundItems"),
            true,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        ShowFoundItemsLimitCfg = config.Bind(
            new ConfigDefinition("Confirmation Settings", "showFoundItemsLimit"),
            100,
            new ConfigDescription("Won't show founds items if the total item count is over the limit.")
        );
        SortFoundItemsCfg = config.Bind(
            new ConfigDefinition("Confirmation Settings", "sortFoundItems"),
            true,
            new ConfigDescription("Sorts found items from most to least expensive on the confirmation screen. This might cost more performance.")
        );
        AlignFoundItemsPriceCfg = config.Bind(
            new ConfigDefinition("Confirmation Settings", "alignFoundItemsPrice"),
            true,
            new ConfigDescription("Align all prices of found items on the confirmation screen. This might cost more performance.")
        );

        // Misc Settings
        SpeakInShipCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "speakInShip"),
            true,
            new ConfigDescription("The Company will speak inside your ship after selling.")
        );
        OverrideWelcomeMessageCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "overrideWelcomeMessage"),
            true,
            new ConfigDescription("Overrides the terminal welcome message to add some additional info.")
        );
        OverrideHelpMessageCfg = config.Bind(
            new ConfigDefinition("Misc Settings", "overrideHelpMessage"),
            true,
            new ConfigDescription("Overrides the terminal help message to add some additional info.")
        );
    }

    public void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        hostConfigData = syncedConfigData;

        SellMyScrapBase.Instance.UpdateCachedDontSellList(DontSellListJson);
    }
}
