using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap;

public class ConfigHelper
{
    // Config Items
    private static List<ConfigItem> sellConfigItems = new List<ConfigItem>();
    private static List<ConfigItem> advancedSellConfigItems = new List<ConfigItem>();
    private static List<ConfigItem> terminalConfigItems = new List<ConfigItem>();
    private static List<ConfigItem> miscConfigItems = new List<ConfigItem>();
    private static List<ConfigItem> scrapEaterConfigItems = new List<ConfigItem>();

    private static List<ConfigItem> allConfigItems
    {
        get
        {
            return [
                .. sellConfigItems,
                .. advancedSellConfigItems,
                .. terminalConfigItems,
                .. miscConfigItems,
                .. scrapEaterConfigItems,
            ];
        }
    }

    internal static void Initialize()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        sellConfigItems = [
            new ConfigItem("sellGifts",    typeof(bool), isHostOnly: true, value => { configManager.SellGifts =    bool.Parse(value); }, () => { return configManager.SellGifts.ToString();    }),
            new ConfigItem("sellShotguns", typeof(bool), isHostOnly: true, value => { configManager.SellShotguns = bool.Parse(value); }, () => { return configManager.SellShotguns.ToString(); }),
            new ConfigItem("sellAmmo",     typeof(bool), isHostOnly: true, value => { configManager.SellAmmo =     bool.Parse(value); }, () => { return configManager.SellAmmo.ToString();     }),
            new ConfigItem("sellKnives",   typeof(bool), isHostOnly: true, value => { configManager.SellKnives =   bool.Parse(value); }, () => { return configManager.SellKnives.ToString();   }),
            new ConfigItem("sellPickles",  typeof(bool), isHostOnly: true, value => { configManager.SellPickles =  bool.Parse(value); }, () => { return configManager.SellPickles.ToString();  }),
        ];

        advancedSellConfigItems = [
            new ConfigItem("sellScrapWorthZero",   typeof(bool),     isHostOnly: true, value => { configManager.SellScrapWorthZero =   bool.Parse(value); }, () => { return configManager.SellScrapWorthZero.ToString();   }),
            new ConfigItem("onlySellScrapOnFloor", typeof(bool),     isHostOnly: true, value => { configManager.OnlySellScrapOnFloor = bool.Parse(value); }, () => { return configManager.OnlySellScrapOnFloor.ToString(); }),
            new ConfigItem("dontSellListJson",     typeof(string[]), isHostOnly: true, value => { configManager.DontSellListJson = JsonConvert.DeserializeObject<string[]>(value); }, () => { return JsonConvert.SerializeObject(configManager.DontSellListJson); }),
            new ConfigItem("sellListJson",         typeof(string[]), isHostOnly: true, value => { configManager.SellListJson = JsonConvert.DeserializeObject<string[]>(value); }, () => { return JsonConvert.SerializeObject(configManager.SellListJson); }),
        ];

        terminalConfigItems = [
            new ConfigItem("overrideWelcomeMessage", typeof(bool), isHostOnly: false, value => { configManager.OverrideWelcomeMessage = bool.Parse(value); }, () => { return configManager.OverrideWelcomeMessage.ToString(); }),
            new ConfigItem("overrideHelpMessage",    typeof(bool), isHostOnly: false, value => { configManager.OverrideHelpMessage =    bool.Parse(value); }, () => { return configManager.OverrideHelpMessage.ToString();    }),
            new ConfigItem("showFoundItems",         typeof(bool), isHostOnly: false, value => { configManager.ShowFoundItems =         bool.Parse(value); }, () => { return configManager.ShowFoundItems.ToString();         }),
            new ConfigItem("sortFoundItemsPrice",    typeof(bool), isHostOnly: false, value => { configManager.SortFoundItemsPrice =    bool.Parse(value); }, () => { return configManager.SortFoundItemsPrice.ToString();    }),
            new ConfigItem("alignFoundItemsPrice",   typeof(bool), isHostOnly: false, value => { configManager.AlignFoundItemsPrice =   bool.Parse(value); }, () => { return configManager.AlignFoundItemsPrice.ToString();   }),
        ];

        miscConfigItems = [
            new ConfigItem("speakInShip",               typeof(bool), isHostOnly: false, value => { configManager.SpeakInShip =               bool.Parse(value); }, () => { return configManager.SpeakInShip.ToString();               }),
        ];

        scrapEaterConfigItems = [
            new ConfigItem("scrapEaterChance",      typeof(int), isHostOnly: true, value => { configManager.ScrapEaterChance =      int.Parse(value); }, () => { return configManager.ScrapEaterChance.ToString();      }),
            new ConfigItem("octolarSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.OctolarSpawnWeight =    int.Parse(value); }, () => { return configManager.OctolarSpawnWeight.ToString();    }),
            new ConfigItem("takeySpawnWeight",      typeof(int), isHostOnly: true, value => { configManager.TakeySpawnWeight =      int.Parse(value); }, () => { return configManager.TakeySpawnWeight.ToString();      }),
            new ConfigItem("maxwellSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.MaxwellSpawnWeight =    int.Parse(value); }, () => { return configManager.MaxwellSpawnWeight.ToString();    }),
            new ConfigItem("yippeeSpawnWeight",     typeof(int), isHostOnly: true, value => { configManager.YippeeSpawnWeight =     int.Parse(value); }, () => { return configManager.YippeeSpawnWeight.ToString();     }),
            new ConfigItem("cookieFumoSpawnWeight", typeof(int), isHostOnly: true, value => { configManager.CookieFumoSpawnWeight = int.Parse(value); }, () => { return configManager.CookieFumoSpawnWeight.ToString(); }),
            new ConfigItem("psychoSpawnWeight",     typeof(int), isHostOnly: true, value => { configManager.PsychoSpawnWeight =     int.Parse(value); }, () => { return configManager.PsychoSpawnWeight.ToString();     }),
            new ConfigItem("zombiesSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.ZombiesSpawnWeight =    int.Parse(value); }, () => { return configManager.ZombiesSpawnWeight.ToString();    }),
        ];
    }

    internal static bool TrySetConfigValue(string key, string value, out ConfigItem configItem, out string parsedValue)
    {
        parsedValue = string.Empty;

        configItem = GetConfigItem(key);
        if (configItem == null) return false;

        if (configItem.isHostOnly && !Plugin.IsHostOrServer) return false;

        if (configItem.type == typeof(bool))
        {
            return TrySetBoolConfigValue(configItem, value, out parsedValue);
        }

        if (configItem.type == typeof(int))
        {
            return TrySetIntConfigValue(configItem, value, out parsedValue);
        }

        return false;
    }

    private static bool TrySetBoolConfigValue(ConfigItem configItem, string value, out string parsedValue)
    {
        parsedValue = string.Empty;

        if (bool.TryParse(value, out bool parsedBool))
        {
            configItem.SetValue(value);
            parsedValue = parsedBool.ToString();

            return true;
        }

        return false;
    }

    private static bool TrySetIntConfigValue(ConfigItem configItem, string value, out string parsedValue)
    {
        parsedValue = string.Empty;

        if (int.TryParse(value, out int parsedInteger))
        {
            configItem.SetValue(value);
            parsedValue = parsedInteger.ToString();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Link your scrap eater spawnWeight config setting to the SellMyScrap terminal config editor.
    /// </summary>
    /// <param name="key">spawnWeight config setting key.</param>
    /// <param name="SetValue">Action for setting your spawnWeight config setting value.</param>
    /// <param name="GetValue">Func for getting your spawnWeight config setting value.</param>
    public static void AddScrapEaterConfigItem(string key, Action<string> SetValue, Func<string> GetValue)
    {
        scrapEaterConfigItems.Add(new ConfigItem(key, typeof(int), isHostOnly: true, SetValue, GetValue));
    }

    private static ConfigItem GetConfigItem(string key)
    {
        foreach (ConfigItem configItem in allConfigItems)
        {
            if (configItem.key.ToLower() == key.ToLower())
            {
                return configItem;
            }
        }

        return null;
    }

    internal static string GetConfigSettingsMessage()
    {
        string message = string.Empty;
        message += GetConfigItemListMessage("[Sell Settings]", sellConfigItems, syncedWithHost: true);
        message += GetConfigItemListMessage("[Advanced Sell Settings]", advancedSellConfigItems, syncedWithHost: true);
        message += GetConfigItemListMessage("[Terminal Settings]", terminalConfigItems);
        message += GetConfigItemListMessage("[Misc Settings]", miscConfigItems);
        message += GetConfigItemListMessage("[Scrap Eater Settings]", scrapEaterConfigItems, hostOnly: true);

        return message.Trim();
    }

    private static string GetConfigItemListMessage(string header, List<ConfigItem> list, bool syncedWithHost = false, bool hostOnly = false)
    {
        string[] keys = list.Select(item => item.key).ToArray();
        int maxLength = Utils.GetLongestStringFromArray(keys).Length + 1;

        string additionalHeaderMessage = string.Empty;

        if (syncedWithHost && !Plugin.IsHostOrServer)
        {
            additionalHeaderMessage = " (Synced with host)";
        }

        if (hostOnly && !Plugin.IsHostOrServer)
        {
            additionalHeaderMessage = " (Host only)";
        }

        string message = $"{header}{additionalHeaderMessage}\n";

        list.ForEach(configItem =>
        {
            if (configItem.GetValue == null)
            {
                Plugin.logger.LogError($"Error: Func<string> GetValue() for ConfigItem key: \"{configItem.key}\" could not be found!");
                return;
            }

            message += $"{Utils.GetStringWithSpacingInBetween($"{configItem.key}:", configItem.GetValue(), maxLength)}\n";
        });

        return $"{message.Trim()}\n\n";
    }
}

public class ConfigItem
{
    public string key;
    public Type type;
    public bool isHostOnly;
    public Action<string> SetValue;
    public Func<string> GetValue;

    public ConfigItem(string key, Type type, bool isHostOnly, Action<string> SetValue, Func<string> GetValue)
    {
        this.key = key;
        this.type = type;
        this.isHostOnly = isHostOnly;
        this.SetValue = SetValue;
        this.GetValue = GetValue;
    }
}
