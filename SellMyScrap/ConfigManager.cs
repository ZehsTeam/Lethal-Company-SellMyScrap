using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Extensions;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Linq;
using System.Text;

namespace com.github.zehsteam.SellMyScrap;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // General
    public static ConfigEntry<bool> ExtendedLogging { get; private set; }

    // Sell
    public static SyncedConfigEntry<bool> SellGifts { get; private set; }
    public static SyncedConfigEntry<bool> SellShotguns { get; private set; }
    public static SyncedConfigEntry<bool> SellAmmo { get; private set; }
    public static SyncedConfigEntry<bool> SellKnives { get; private set; }
    public static SyncedConfigEntry<bool> SellPickles { get; private set; }

    // Advanced Sell
    public static SyncedConfigEntry<bool> SellScrapWorthZero { get; private set; }
    public static SyncedConfigEntry<bool> OnlySellScrapOnFloor { get; private set; }
    public static SyncedConfigEntry<string> PrioritySellList { get; private set; }
    public static SyncedConfigEntry<string> DontSellList { get; private set; }
    public static SyncedConfigEntry<string> SellList { get; private set; }
    
    public static string[] PrioritySellListArray
    {
        get => CollectionExtensions.StringToCollection<string>(PrioritySellList.Value).ToArray();
        set => PrioritySellList.Value = CollectionExtensions.CollectionToString(value);
    }
    
    public static string[] DontSellListArray
    {
        get => CollectionExtensions.StringToCollection<string>(DontSellList.Value).ToArray();
        set => DontSellList.Value = CollectionExtensions.CollectionToString(value);
    }

    public static string[] SellListArray
    {
        get => CollectionExtensions.StringToCollection<string>(SellList.Value).ToArray();
        set => SellList.Value = CollectionExtensions.CollectionToString(value);
    }

    // Terminal
    public static ConfigEntry<bool> OverrideWelcomeMessage { get; private set; }
    public static ConfigEntry<bool> OverrideHelpMessage { get; private set; }
    public static ConfigEntry<bool> ShowFoundItems { get; private set; }
    public static ConfigEntry<bool> SortFoundItemsPrice { get; private set; }
    public static ConfigEntry<bool> AlignFoundItemsPrice { get; private set; }

    // Misc
    public static ConfigEntry<bool> SpeakInShip { get; private set; }
    public static ConfigEntry<float> RareVoiceLineChance { get; private set; }
    public static ConfigEntry<bool> ShowQuotaWarning { get; private set; }

    // Scrap Eater
    public static ConfigEntry<int> ScrapEaterChance { get; private set; }
    public static ConfigEntry<int> OctolarSpawnWeight { get; private set; }
    public static ConfigEntry<int> TakeySpawnWeight { get; private set; }
    public static ConfigEntry<int> MaxwellSpawnWeight { get; private set; }
    public static ConfigEntry<int> YippeeSpawnWeight { get; private set; }
    public static ConfigEntry<int> CookieFumoSpawnWeight { get; private set; }
    public static ConfigEntry<int> PsychoSpawnWeight { get; private set; }
    public static ConfigEntry<int> ZombiesSpawnWeight { get; private set; }
    public static ConfigEntry<int> WolfySpawnWeight { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        BindConfigs();
    }

    private static void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // General
        ExtendedLogging = ConfigHelper.Bind("General", "ExtendedLogging", defaultValue: false, requiresRestart: false, "Enable extended logging.");

        // Sell
        SellGifts =    ConfigHelper.BindSynced("Sell", "SellGifts",    defaultValue: false, "Do you want to sell Gifts?");
        SellShotguns = ConfigHelper.BindSynced("Sell", "SellShotguns", defaultValue: false, "Do you want to sell Shotguns?");
        SellAmmo =     ConfigHelper.BindSynced("Sell", "SellAmmo",     defaultValue: false, "Do you want to sell Ammo?");
        SellKnives =   ConfigHelper.BindSynced("Sell", "SellKnives",   defaultValue: false, "Do you want to sell Kitchen knives?");
        SellPickles =  ConfigHelper.BindSynced("Sell", "SellPickles",  defaultValue: true,  "Do you want to sell Jar of pickles?");

        // Advanced Sell
        SellScrapWorthZero =   ConfigHelper.BindSynced("Advanced Sell", "SellScrapWorthZero",   defaultValue: false,                                                              "Do you want to sell scrap worth zero?");
        OnlySellScrapOnFloor = ConfigHelper.BindSynced("Advanced Sell", "OnlySellScrapOnFloor", defaultValue: false,                                                              "Do you want to sell scrap that is only on the floor?");
        PrioritySellList =     ConfigHelper.BindSynced("Advanced Sell", "PrioritySellList",     defaultValue: "Tragedy, Comedy, Whoopie cushion, Easter egg, Clock, Soccer ball", GetPrioritySellListDescription());
        DontSellList =         ConfigHelper.BindSynced("Advanced Sell", "DontSellList",         defaultValue: "",                                                                 GetDontSellListDescription());
        SellList =             ConfigHelper.BindSynced("Advanced Sell", "SellList",             defaultValue: "Whoopie cushion, Easter egg, Tragedy, Comedy",                     GetSellListDescription());

        // Terminal
        OverrideWelcomeMessage = ConfigHelper.Bind("Terminal", "OverrideWelcomeMessage", defaultValue: true, requiresRestart: false, "Overrides the terminal welcome message to add additional info.");
        OverrideHelpMessage =    ConfigHelper.Bind("Terminal", "OverrideHelpMessage",    defaultValue: true, requiresRestart: false, "Overrides the terminal help message to add additional info.");
        ShowFoundItems =         ConfigHelper.Bind("Terminal", "ShowFoundItems",         defaultValue: true, requiresRestart: false, "Show found items on the confirmation screen.");
        SortFoundItemsPrice =    ConfigHelper.Bind("Terminal", "SortFoundItemsPrice",    defaultValue: true, requiresRestart: false, "Sorts found items from most to least expensive.");
        AlignFoundItemsPrice =   ConfigHelper.Bind("Terminal", "AlignFoundItemsPrice",   defaultValue: true, requiresRestart: false, "Align all prices of found items.");

        // Misc
        SpeakInShip =         ConfigHelper.Bind("Misc", "SpeakInShip",         defaultValue: true, requiresRestart: false, "The Company will speak inside your ship after selling from the terminal.");
        RareVoiceLineChance = ConfigHelper.Bind("Misc", "RareVoiceLineChance", defaultValue: 5f,   requiresRestart: false, "The percent chance the Company will say a rare microphone voice line after selling.", new AcceptableValueRange<float>(0.0f, 100.0f));
        ShowQuotaWarning =    ConfigHelper.Bind("Misc", "ShowQuotaWarning",    defaultValue: true, requiresRestart: false, "If enabled, will show a warning when you try to pull the ship's lever when the quota hasn't been fulfilled at the Company building with 0 days left.");

        // Scrap Eater
        ScrapEaterChance =      ConfigHelper.Bind("Scrap Eater", "ScrapEaterChance",      defaultValue: 0, requiresRestart: false, "The percent chance a scrap eater will spawn?!",                  new AcceptableValueRange<int>(0, 100));
        OctolarSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "OctolarSpawnWeight",    defaultValue: 1, requiresRestart: false, "The spawn chance weight Octolar will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        TakeySpawnWeight =      ConfigHelper.Bind("Scrap Eater", "TakeySpawnWeight",      defaultValue: 1, requiresRestart: false, "The spawn chance weight Takey will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
        MaxwellSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "MaxwellSpawnWeight",    defaultValue: 1, requiresRestart: false, "The spawn chance weight Maxwell will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        YippeeSpawnWeight =     ConfigHelper.Bind("Scrap Eater", "YippeeSpawnWeight",     defaultValue: 1, requiresRestart: false, "The spawn chance weight Yippee will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        CookieFumoSpawnWeight = ConfigHelper.Bind("Scrap Eater", "CookieFumoSpawnWeight", defaultValue: 1, requiresRestart: false, "The spawn chance weight Cookie Fumo will spawn?! (scrap eater)", new AcceptableValueRange<int>(0, 100));
        PsychoSpawnWeight =     ConfigHelper.Bind("Scrap Eater", "PsychoSpawnWeight",     defaultValue: 1, requiresRestart: false, "The spawn chance weight Psycho will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        ZombiesSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "ZombiesSpawnWeight",    defaultValue: 1, requiresRestart: false, "The spawn chance weight Zombies will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        WolfySpawnWeight =      ConfigHelper.Bind("Scrap Eater", "WolfySpawnWeight",      defaultValue: 1, requiresRestart: false, "The spawn chance weight Wolfy will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
    }

    private static string GetPrioritySellListDescription()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Array of item names to prioritize when selling.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");
        return builder.ToString();
    }

    private static string GetDontSellListDescription()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Array of item names to not sell.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");
        return builder.ToString();
    }

    private static string GetSellListDescription()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Array of item names to sell when using the `sell list` command.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");
        return builder.ToString();
    }
}
