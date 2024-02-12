using BepInEx.Configuration;
using Newtonsoft.Json;
using Unity.Netcode;

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

    // Terminal Settings
    private ConfigEntry<bool> OverrideWelcomeMessageCfg;
    private ConfigEntry<bool> OverrideHelpMessageCfg;
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<int> ShowFoundItemsLimitCfg;
    private ConfigEntry<bool> SortFoundItemsCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Misc Settings
    private ConfigEntry<bool> SpeakInShipCfg;

    // Sell Settings (Synced)
    internal bool SellGifts
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellGifts;

            return SellGiftsCfg.Value;
        }
        set
        {
            SellGiftsCfg.Value = value;
            ConfigsChanged();
        }
    }
    
    internal bool SellShotguns
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellShotguns;

            return SellShotgunsCfg.Value;
        }
        set
        {
            SellShotgunsCfg.Value = value;
            ConfigsChanged();
        }
    }
    
    internal bool SellAmmo 
    {
        get 
        {
            if (hostConfigData != null) return hostConfigData.sellAmmo;

            return SellAmmoCfg.Value;
        }
        set
        {
            SellAmmoCfg.Value = value;
            ConfigsChanged();
        }
    }
    
    internal bool SellPickles
    { 
        get
        {
            if (hostConfigData != null) return hostConfigData.sellPickles;
            
            return SellPicklesCfg.Value;
        }
        set
        {
            SellPicklesCfg.Value = value;
            ConfigsChanged();
        }
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
            ConfigsChanged();
        }
    }

    // Terminal Settings
    internal bool OverrideWelcomeMessage { get { return OverrideWelcomeMessageCfg.Value; } set => OverrideWelcomeMessageCfg.Value = value; }
    internal bool OverrideHelpMessage { get { return OverrideHelpMessageCfg.Value; } set => OverrideHelpMessageCfg.Value = value; }
    internal bool ShowFoundItems { get { return ShowFoundItemsCfg.Value; } set => ShowFoundItemsCfg.Value = value; }
    internal int ShowFoundItemsLimit { get { return ShowFoundItemsLimitCfg.Value; } set => ShowFoundItemsLimitCfg.Value = value; }
    internal bool SortFoundItems { get { return SortFoundItemsCfg.Value; } set => SortFoundItemsCfg.Value = value; }
    internal bool AlignFoundItemsPrice { get { return AlignFoundItemsPriceCfg.Value; } set => AlignFoundItemsPriceCfg.Value = value; }

    // Misc Settings
    internal bool SpeakInShip { get { return SpeakInShipCfg.Value; } set => SpeakInShipCfg.Value = value; }

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
        dontSellListJsonCfgDescription += "Spaces do matter for item names.\n";
        dontSellListJsonCfgDescription += "https://www.w3schools.com/js/js_json_arrays.asp\n\n";
        dontSellListJsonCfgDescription += "Example: [\"Gift\", \"Shotgun\", \"Ammo\"]\n";
        DontSellListJsonCfg = config.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            "[]",
            new ConfigDescription(dontSellListJsonCfgDescription)
        );

        SellMyScrapBase.Instance.UpdateCachedDontSellList(DontSellListJson);

        // Terminal Settings
        OverrideWelcomeMessageCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "overrideWelcomeMessage"),
            true,
            new ConfigDescription("Overrides the terminal welcome message to add some additional info.")
        );
        OverrideHelpMessageCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "overrideHelpMessage"),
            true,
            new ConfigDescription("Overrides the terminal help message to add some additional info.")
        );
        ShowFoundItemsCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "showFoundItems"),
            true,
            new ConfigDescription("Show found items on the confirmation screen.")
        );
        ShowFoundItemsLimitCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "showFoundItemsLimit"),
            100,
            new ConfigDescription("Won't show found items if the total item count is over the limit.")
        );
        SortFoundItemsCfg = config.Bind(
            new ConfigDefinition("Terminal Settings", "sortFoundItems"),
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
    }

    public void SetHostConfigData(SyncedConfigData syncedConfigData)
    {
        hostConfigData = syncedConfigData;

        SellMyScrapBase.Instance.UpdateCachedDontSellList(DontSellListJson);
    }

    private void ConfigsChanged()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        PluginNetworkBehaviour.Instance.SendConfigToPlayerClientRpc(new SyncedConfigData(this));
    }
}
