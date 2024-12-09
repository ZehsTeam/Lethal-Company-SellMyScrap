using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Dependencies.LethalConfigProxy;
using com.github.zehsteam.SellMyScrap.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal class ConfigManager
{
    // General
    public ConfigEntry<bool> ExtendedLogging { get; private set; }

    // Sell
    public SyncedConfigEntry<bool> SellGifts { get; private set; }
    public SyncedConfigEntry<bool> SellShotguns { get; private set; }
    public SyncedConfigEntry<bool> SellAmmo { get; private set; }
    public SyncedConfigEntry<bool> SellKnives { get; private set; }
    public SyncedConfigEntry<bool> SellPickles { get; private set; }

    // Advanced Sell
    public SyncedConfigEntry<bool> SellScrapWorthZero { get; private set; }
    public SyncedConfigEntry<bool> OnlySellScrapOnFloor { get; private set; }
    public SyncedConfigEntry<string> PrioritySellList { get; private set; }
    public SyncedConfigEntry<string> DontSellList { get; private set; }
    public SyncedConfigEntry<string> SellList { get; private set; }
    
    public string[] PrioritySellListArray
    {
        get
        {
            return Utils.StringToArray<string>(PrioritySellList.Value);
        }
        set
        {
            PrioritySellList.Value = Utils.ArrayToString(value);
        }
    }
    
    public string[] DontSellListArray
    {
        get
        {
            return Utils.StringToArray<string>(DontSellList.Value);
        }
        set
        {
            DontSellList.Value = Utils.ArrayToString(value);
        }
    }

    public string[] SellListArray
    {
        get
        {
            return Utils.StringToArray<string>(SellList.Value);
        }
        set
        {
            SellList.Value = Utils.ArrayToString(value);
        }
    }

    // Terminal
    public ConfigEntry<bool> OverrideWelcomeMessage { get; private set; }
    public ConfigEntry<bool> OverrideHelpMessage { get; private set; }
    public ConfigEntry<bool> ShowFoundItems { get; private set; }
    public ConfigEntry<bool> SortFoundItemsPrice { get; private set; }
    public ConfigEntry<bool> AlignFoundItemsPrice { get; private set; }

    // Misc
    public ConfigEntry<bool> SpeakInShip { get; private set; }
    public ConfigEntry<float> RareVoiceLineChance { get; private set; }
    public ConfigEntry<bool> ShowQuotaWarning { get; private set; }

    // Scrap Eater
    public ConfigEntry<int> ScrapEaterChance { get; private set; }
    public ConfigEntry<int> OctolarSpawnWeight { get; private set; }
    public ConfigEntry<int> TakeySpawnWeight { get; private set; }
    public ConfigEntry<int> MaxwellSpawnWeight { get; private set; }
    public ConfigEntry<int> YippeeSpawnWeight { get; private set; }
    public ConfigEntry<int> CookieFumoSpawnWeight { get; private set; }
    public ConfigEntry<int> PsychoSpawnWeight { get; private set; }
    public ConfigEntry<int> ZombiesSpawnWeight { get; private set; }
    public ConfigEntry<int> WolfySpawnWeight { get; private set; }

    public ConfigManager()
    {
        BindConfigs();
        MigrateOldConfigSettings();
        ConfigHelper.ClearUnusedEntries();
    }
    
    private void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // General
        ExtendedLogging = ConfigHelper.Bind("General", "ExtendedLogging", defaultValue: false, requiresRestart: false, "Enable extended logging.");

        // Sell
        SellGifts =    new SyncedConfigEntry<bool>("Sell", "SellGifts",    defaultValue: false, "Do you want to sell Gifts?");
        SellShotguns = new SyncedConfigEntry<bool>("Sell", "SellShotguns", defaultValue: false, "Do you want to sell Shotguns?");
        SellAmmo =     new SyncedConfigEntry<bool>("Sell", "SellAmmo",     defaultValue: false, "Do you want to sell Ammo?");
        SellKnives =   new SyncedConfigEntry<bool>("Sell", "SellKnives",   defaultValue: false, "Do you want to sell Kitchen knives?");
        SellPickles =  new SyncedConfigEntry<bool>("Sell", "SellPickles",  defaultValue: true,  "Do you want to sell Jar of pickles?");

        // Advanced Sell
        SellScrapWorthZero =   new SyncedConfigEntry<bool>(  "Advanced Sell", "SellScrapWorthZero",   defaultValue: false,                                                              "Do you want to sell scrap worth zero?");
        OnlySellScrapOnFloor = new SyncedConfigEntry<bool>(  "Advanced Sell", "OnlySellScrapOnFloor", defaultValue: false,                                                              "Do you want to sell scrap that is only on the floor?");
        PrioritySellList =     new SyncedConfigEntry<string>("Advanced Sell", "PrioritySellList",     defaultValue: "Tragedy, Comedy, Whoopie cushion, Easter egg, Clock, Soccer ball", GetPrioritySellListDescription());
        DontSellList =         new SyncedConfigEntry<string>("Advanced Sell", "DontSellList",         defaultValue: "",                                                                 GetDontSellListDescription());
        SellList =             new SyncedConfigEntry<string>("Advanced Sell", "SellList",             defaultValue: "Whoopie cushion, Easter egg, Tragedy, Comedy",                     GetSellListDescription());

        // Terminal
        OverrideWelcomeMessage = ConfigHelper.Bind("Terminal", "OverrideWelcomeMessage", defaultValue: true, requiresRestart: false, "Overrides the terminal welcome message to add additional info.");
        OverrideHelpMessage =    ConfigHelper.Bind("Terminal", "OverrideHelpMessage",    defaultValue: true, requiresRestart: false, "Overrides the terminal help message to add additional info.");
        ShowFoundItems =         ConfigHelper.Bind("Terminal", "ShowFoundItems",         defaultValue: true, requiresRestart: false, "Show found items on the confirmation screen.");
        SortFoundItemsPrice =    ConfigHelper.Bind("Terminal", "SortFoundItemsPrice",    defaultValue: true, requiresRestart: false, "Sorts found items from most to least expensive.");
        AlignFoundItemsPrice =   ConfigHelper.Bind("Terminal", "AlignFoundItemsPrice",   defaultValue: true, requiresRestart: false, "Align all prices of found items.");

        // Misc
        SpeakInShip =         ConfigHelper.Bind("Misc", "SpeakInShip",         defaultValue: true, requiresRestart: false, "The Company will speak inside your ship after selling from the terminal.");
        RareVoiceLineChance = ConfigHelper.Bind("Misc", "RareVoiceLineChance", defaultValue: 5f,   requiresRestart: false, "The percent chance the Company will say a rare microphone voice line after selling.");
        ShowQuotaWarning =    ConfigHelper.Bind("Misc", "ShowQuotaWarning",    defaultValue: true, requiresRestart: false, "If enabled, will show a warning when you try to pull the ship's lever when the quota hasn't been fulfilled at the Company building with 0 days left.");

        // Scrap Eater
        ScrapEaterChance =      ConfigHelper.Bind("Scrap Eater", "ScrapEaterChance",      defaultValue: 75, requiresRestart: false, "The percent chance a scrap eater will spawn?!",                  new AcceptableValueRange<int>(0, 100));
        OctolarSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "OctolarSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Octolar will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        TakeySpawnWeight =      ConfigHelper.Bind("Scrap Eater", "TakeySpawnWeight",      defaultValue: 1,  requiresRestart: false, "The spawn chance weight Takey will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
        MaxwellSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "MaxwellSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Maxwell will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        YippeeSpawnWeight =     ConfigHelper.Bind("Scrap Eater", "YippeeSpawnWeight",     defaultValue: 1,  requiresRestart: false, "The spawn chance weight Yippee will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        CookieFumoSpawnWeight = ConfigHelper.Bind("Scrap Eater", "CookieFumoSpawnWeight", defaultValue: 1,  requiresRestart: false, "The spawn chance weight Cookie Fumo will spawn?! (scrap eater)", new AcceptableValueRange<int>(0, 100));
        PsychoSpawnWeight =     ConfigHelper.Bind("Scrap Eater", "PsychoSpawnWeight",     defaultValue: 1,  requiresRestart: false, "The spawn chance weight Psycho will spawn?! (scrap eater)",      new AcceptableValueRange<int>(0, 100));
        ZombiesSpawnWeight =    ConfigHelper.Bind("Scrap Eater", "ZombiesSpawnWeight",    defaultValue: 1,  requiresRestart: false, "The spawn chance weight Zombies will spawn?! (scrap eater)",     new AcceptableValueRange<int>(0, 100));
        WolfySpawnWeight =      ConfigHelper.Bind("Scrap Eater", "WolfySpawnWeight",      defaultValue: 1,  requiresRestart: false, "The spawn chance weight Wolfy will spawn?! (scrap eater)",       new AcceptableValueRange<int>(0, 100));
    }

    private string GetPrioritySellListDescription()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Array of item names to prioritize when selling.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");

        return builder.ToString();
    }

    private string GetDontSellListDescription()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Array of item names to not sell.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");

        return builder.ToString();
    }

    private string GetSellListDescription()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Array of item names to sell when using the `sell list` command.");
        builder.AppendLine("Each entry should be separated by a comma.");
        builder.AppendLine("Item names are not case-sensitive but, spaces do matter.");

        return builder.ToString();
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
        System.StringComparison comparisonType = System.StringComparison.OrdinalIgnoreCase;

        if (section.Equals("Sell Settings", comparisonType))
        {
            if (key.Equals("SellGifts", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellGifts, value);
                return;
            }

            if (key.Equals("SellShotguns", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellShotguns, value);
                return;
            }

            if (key.Equals("SellAmmo", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellAmmo, value);
                return;
            }

            if (key.Equals("SellKnives", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellKnives, value);
                return;
            }

            if (key.Equals("SellPickles", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellPickles, value);
                return;
            }
        }

        if (section.Equals("Advanced Sell Settings", comparisonType))
        {
            if (key.Equals("SellScrapWorthZero", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SellScrapWorthZero, value);
                return;
            }

            if (key.Equals("OnlySellScrapOnFloor", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(OnlySellScrapOnFloor, value);
                return;
            }

            if (Utils.StringEquals(key, ["PrioritySellList", "PrioritySellListJson"], matchCase: false))
            {
                ConfigHelper.SetConfigEntryValue(PrioritySellList, value.Replace("\\", ""));
            }

            if (Utils.StringEquals(key, ["DontSellList", "DontSellListJson"], matchCase: false))
            {
                ConfigHelper.SetConfigEntryValue(DontSellList, value.Replace("\\", ""));
            }

            if (Utils.StringEquals(key, ["SellList", "SellListJson"], matchCase: false))
            {
                ConfigHelper.SetConfigEntryValue(SellList, value.Replace("\\", ""));
            }
        }

        if (section.Equals("Terminal Settings", comparisonType))
        {
            if (key.Equals("OverrideWelcomeMessage", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(OverrideWelcomeMessage, value);
                return;
            }

            if (key.Equals("OverrideHelpMessage", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(OverrideHelpMessage, value);
                return;
            }

            if (key.Equals("ShowFoundItems", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(ShowFoundItems, value);
                return;
            }

            if (key.Equals("SortFoundItemsPrice", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SortFoundItemsPrice, value);
                return;
            }

            if (key.Equals("AlignFoundItemsPrice", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(AlignFoundItemsPrice, value);
                return;
            }
        }

        if (section.Equals("Misc Settings", comparisonType))
        {
            if (key.Equals("SpeakInShip", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(SpeakInShip, value);
                return;
            }

            if (key.Equals("RareVoiceLineChance", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(RareVoiceLineChance, value);
                return;
            }
        }

        if (section.Equals("Scrap Eater Settings", comparisonType))
        {
            if (key.Equals("ScrapEaterChance", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(ScrapEaterChance, value);
                return;
            }

            if (key.Equals("OctolarSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(OctolarSpawnWeight, value);
                return;
            }

            if (key.Equals("TakeySpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(TakeySpawnWeight, value);
                return;
            }

            if (key.Equals("MaxwellSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(MaxwellSpawnWeight, value);
                return;
            }

            if (key.Equals("YippeeSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(YippeeSpawnWeight, value);
                return;
            }

            if (key.Equals("CookieFumoSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(CookieFumoSpawnWeight, value);
                return;
            }

            if (key.Equals("PsychoSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(PsychoSpawnWeight, value);
                return;
            }

            if (key.Equals("ZombiesSpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(ZombiesSpawnWeight, value);
                return;
            }

            if (key.Equals("WolfySpawnWeight", comparisonType))
            {
                ConfigHelper.SetConfigEntryValue(WolfySpawnWeight, value);
                return;
            }
        }
    }

    internal void TrySetCustomValues()
    {
        if (SteamUtils.IsLocalClient(PlayerName.Insym)) return;

        TrySetCustomValuesForThorlar();
        TrySetCustomValuesForTakerst();

        // Reset ScrapEaterChance for Insym's modpack if not Insym.

        if (ScrapEaterChance.Value != 0) return;

        if (DontSellListArray.Length == 1 && DontSellListArray[0].Equals("gold bar", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!ModpackSaveSystem.ReadValue("ResetScrapEaterChance", false))
            {
                ScrapEaterChance.Value = (int)ScrapEaterChance.DefaultValue;

                ModpackSaveSystem.WriteValue("ResetScrapEaterChance", true);
            }
        }
    }

    private void TrySetCustomValuesForThorlar()
    {
        if (!SteamUtils.IsLocalClient(PlayerName.Thorlar)) return;

        if (TakeySpawnWeight.Value > 0 && !ModpackSaveSystem.ReadValue("RemovedTakeyScrapEaterSpawnWeight", false))
        {
            TakeySpawnWeight.Value = 0;

            ModpackSaveSystem.WriteValue("RemovedTakeyScrapEaterSpawnWeight", true);
        }
    }

    private void TrySetCustomValuesForTakerst()
    {
        if (!SteamUtils.IsLocalClient(PlayerName.Takerst)) return;
        
        if (!Utils.ArrayContains(DontSellListArray, "Smol Takey"))
        {
            List<string> array = DontSellListArray.ToList();
            array.Add("Smol Takey");
            DontSellListArray = array.ToArray();
        }

        if (!Utils.ArrayContains(DontSellListArray, "Takey Box"))
        {
            List<string> array = DontSellListArray.ToList();
            array.Add("Takey Box");
            DontSellListArray = array.ToArray();
        }

        if (!Utils.ArrayContains(DontSellListArray, "Takey Mug"))
        {
            List<string> array = DontSellListArray.ToList();
            array.Add("Takey Mug");
            DontSellListArray = array.ToArray();
        }
    }
}
