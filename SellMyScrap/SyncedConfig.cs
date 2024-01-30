using BepInEx.Configuration;
using Newtonsoft.Json;

namespace com.github.zehsteam.SellMyScrap;

/// <summary>
/// This will hold and manage all configs used by the config manager
/// </summary>
public class SyncedConfig
{
    // Sell Settings
    private ConfigEntry<bool> SellGiftsCfg;
    private ConfigEntry<bool> SellShotgunsCfg;
    private ConfigEntry<bool> SellAmmoCfg;
    private ConfigEntry<bool> SellHomemadeFlashbangCfg;
    private ConfigEntry<bool> SellPicklesCfg;

    // Advanced Sell Settings
    private ConfigEntry<string> DontSellListJsonCfg;

    // Confirmation Settings
    private ConfigEntry<bool> ShowFoundItemsCfg;
    private ConfigEntry<int> ShowFoundItemsLimitCfg;
    private ConfigEntry<bool> SortFoundItemsCfg;
    private ConfigEntry<bool> AlignFoundItemsPriceCfg;

    // Sell Settings
    internal bool SellGifts { get { return SellGiftsCfg.Value; } set => SellGiftsCfg.Value = value; }
    internal bool SellShotguns { get { return SellShotgunsCfg.Value; } set => SellShotgunsCfg.Value = value; }
    internal bool SellAmmo { get { return SellAmmoCfg.Value; } set => SellAmmoCfg.Value = value; }
    internal bool SellHomemadeFlashbang { get { return SellHomemadeFlashbangCfg.Value; } set => SellHomemadeFlashbangCfg.Value = value; }
    internal bool SellPickles { get { return SellPicklesCfg.Value; } set => SellPicklesCfg.Value = value; }

    // Advanced Sell Settings
    internal string[] DontSellListJson
    { 
        get
        {
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

    public void RebindConfigs(SyncedConfigData newConfigData)
    {
        // Sell Settings
        SellGiftsCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Sell Settings", "sellGifts"),
            newConfigData.sellGifts,
            new ConfigDescription("Do you want to sell Gifts?")
        );
        SellShotgunsCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Sell Settings", "sellShotguns"),
            newConfigData.sellShotguns,
            new ConfigDescription("Do you want to sell Shotguns?")
        );
        SellAmmoCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Sell Settings", "sellAmmo"),
            newConfigData.sellAmmo,
            new ConfigDescription("Do you want to sell Ammo?")
        );
        SellHomemadeFlashbangCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Sell Settings", "sellHomemadeFlashbang"),
            newConfigData.sellHomemadeFlashbangs,
            new ConfigDescription("Do you want to sell Homemade flashbangs?")
        );
        SellPicklesCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Sell Settings", "sellPickles"),
            newConfigData.sellPickles,
            new ConfigDescription("Do you want to sell Jar of pickles?")
        );

        // Advanced Sell Settings
        string dontSellListJsonCfgDescription = "JSON array of item names to not sell.\n";
        dontSellListJsonCfgDescription += "Item names are not case-sensitive.\n";
        dontSellListJsonCfgDescription += "https://www.w3schools.com/js/js_json_arrays.asp\n\n";
        dontSellListJsonCfgDescription += "Example: [\"Gift\", \"Shotgun\", \"Ammo\"]\n";
        DontSellListJsonCfg = SellMyScrapBase.Instance.Config.Bind(
            new ConfigDefinition("Advanced Sell Settings", "dontSellListJson"),
            newConfigData.dontSellListJson,
            new ConfigDescription(dontSellListJsonCfgDescription)
        );

        // Confirmation Settings
        ShowFoundItemsCfg = SellMyScrapBase.Instance.Config.Bind(new ConfigDefinition("Confirmation Settings", "showFoundItems"), true, new ConfigDescription("Show found items on the confirmation screen."));
        ShowFoundItemsLimitCfg = SellMyScrapBase.Instance.Config.Bind(new ConfigDefinition("Confirmation Settings", "showFoundItemsLimit"), 100, new ConfigDescription("Won't show founds items if the total item count is over the limit."));
        SortFoundItemsCfg = SellMyScrapBase.Instance.Config.Bind(new ConfigDefinition("Confirmation Settings", "sortFoundItems"), true, new ConfigDescription("Sorts found items from most to least expensive on the confirmation screen. This might cost more performance."));
        AlignFoundItemsPriceCfg = SellMyScrapBase.Instance.Config.Bind(new ConfigDefinition("Confirmation Settings", "alignFoundItemsPrice"), true, new ConfigDescription("Align all prices of found items on the confirmation screen. This might cost more performance."));
    }
}
