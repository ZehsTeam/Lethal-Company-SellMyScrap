using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap;

public class ConfigHelper
{
    private static List<ConfigItem> configItems = new List<ConfigItem>();
    private static List<ConfigItem> scrapEaterConfigItems = new List<ConfigItem>();

    public static void Initialize()
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        configItems = [
            new ConfigItem("sellGifts",              typeof(bool), isHostOnly: true,  value => { configManager.SellGifts =              bool.Parse(value); }),
            new ConfigItem("sellShotguns",           typeof(bool), isHostOnly: true,  value => { configManager.SellShotguns =           bool.Parse(value); }),
            new ConfigItem("sellAmmo",               typeof(bool), isHostOnly: true,  value => { configManager.SellAmmo =               bool.Parse(value); }),
            new ConfigItem("sellPickles",            typeof(bool), isHostOnly: true,  value => { configManager.SellPickles =            bool.Parse(value); }),
            new ConfigItem("sellScrapWorthZero",     typeof(bool), isHostOnly: true,  value => { configManager.SellScrapWorthZero =     bool.Parse(value); }),
            new ConfigItem("onlySellScrapOnFloor",   typeof(bool), isHostOnly: true,  value => { configManager.OnlySellScrapOnFloor =   bool.Parse(value); }),
            new ConfigItem("overrideWelcomeMessage", typeof(bool), isHostOnly: false, value => { configManager.OverrideWelcomeMessage = bool.Parse(value); }),
            new ConfigItem("overrideHelpMessage",    typeof(bool), isHostOnly: false, value => { configManager.OverrideHelpMessage =    bool.Parse(value); }),
            new ConfigItem("showFoundItems",         typeof(bool), isHostOnly: false, value => { configManager.ShowFoundItems =         bool.Parse(value); }),
            new ConfigItem("sortFoundItemsPrice",    typeof(bool), isHostOnly: false, value => { configManager.SortFoundItemsPrice =    bool.Parse(value); }),
            new ConfigItem("alignFoundItemsPrice",   typeof(bool), isHostOnly: false, value => { configManager.AlignFoundItemsPrice =   bool.Parse(value); }),
            new ConfigItem("speakInShip",            typeof(bool), isHostOnly: false, value => { configManager.SpeakInShip =            bool.Parse(value); }),
        ];

        scrapEaterConfigItems = [
            new ConfigItem("scrapEaterChance",       typeof(int),  isHostOnly: false, value => { configManager.ScrapEaterChance =       int.Parse(value);  }, () => { return configManager.ScrapEaterChance.ToString();   }),
            new ConfigItem("octolarSpawnWeight",     typeof(int),  isHostOnly: false, value => { configManager.OctolarSpawnWeight =     int.Parse(value);  }, () => { return configManager.OctolarSpawnWeight.ToString(); }),
            new ConfigItem("takeySpawnWeight",       typeof(int),  isHostOnly: false, value => { configManager.TakeySpawnWeight =       int.Parse(value);  }, () => { return configManager.TakeySpawnWeight.ToString();   }),
            new ConfigItem("maxwellSpawnWeight",     typeof(int),  isHostOnly: false, value => { configManager.MaxwellSpawnWeight =     int.Parse(value);  }, () => { return configManager.MaxwellSpawnWeight.ToString(); }),
            new ConfigItem("yippeeSpawnWeight",      typeof(int),  isHostOnly: false, value => { configManager.YippeeSpawnWeight =      int.Parse(value);  }, () => { return configManager.YippeeSpawnWeight.ToString();  }),
        ];
    }

    public static bool TrySetConfigValue(string key, string value, out ConfigItem configItem, out string parsedValue)
    {
        configItem = GetConfigItem(key);
        parsedValue = string.Empty;

        if (configItem == null) return false;

        if (configItem.isHostOnly && !SellMyScrapBase.IsHostOrServer) return false;

        if (configItem.type == typeof(bool))
        {
            if (bool.TryParse(value, out bool parsedBool))
            {
                configItem.SetValue(value);
                parsedValue = parsedBool.ToString();

                return true;
            }
            else
            {
                return false;
            }
        }

        if (configItem.type == typeof(int))
        {
            if (int.TryParse(value, out int parsedInteger))
            {
                configItem.SetValue(value);
                parsedValue = parsedInteger.ToString();

                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public static void AddScrapEaterConfigItem(string key, Type type, bool isHostOnly, Action<string> SetValue, Func<string> GetValue)
    {
        scrapEaterConfigItems.Add(new ConfigItem(key, type, isHostOnly, SetValue, GetValue));
    }

    private static ConfigItem GetConfigItem(string key)
    {
        List<ConfigItem> _configItems = configItems;
        configItems.AddRange(scrapEaterConfigItems);

        foreach (ConfigItem item in _configItems)
        {
            if (item.key.ToLower() == key.ToLower())
            {
                return item;
            }
        }

        return null;
    }

    public static string GetConfigSettingsMessage()
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        string syncedMessage = SellMyScrapBase.IsHostOrServer ? string.Empty : " (Synced with host)";

        string message = $"[Sell Settings]{syncedMessage}\n";
        message += $"sellGifts:    {configManager.SellGifts}\n";
        message += $"sellShotguns: {configManager.SellShotguns}\n";
        message += $"sellAmmo:     {configManager.SellAmmo}\n";
        message += $"sellPickles:  {configManager.SellPickles}\n\n";
        message += $"[Advanced Sell Settings]{syncedMessage}\n";
        message += $"sellScrapWorthZero:   {configManager.SellScrapWorthZero}\n";
        message += $"onlySellScrapOnFloor: {configManager.OnlySellScrapOnFloor}\n";
        message += $"dontSellListJson: {JsonConvert.SerializeObject(configManager.DontSellListJson)}\n\n";
        message += "[Terminal Settings]\n";
        message += $"overrideWelcomeMessage: {configManager.OverrideWelcomeMessage}\n";
        message += $"overrideHelpMessage:    {configManager.OverrideHelpMessage}\n";
        message += $"showFoundItems:         {configManager.ShowFoundItems}\n";
        message += $"sortFoundItemsPrice:    {configManager.SortFoundItemsPrice}\n";
        message += $"alignFoundItemsPrice:   {configManager.AlignFoundItemsPrice}\n\n";
        message += "[Misc Settings]\n";
        message += $"speakInShip:         {configManager.SpeakInShip}\n\n";
        message += GetScrapEaterConfigSettingsMessage();

        return message;
    }

    private static string GetScrapEaterConfigSettingsMessage()
    {
        string[] keys = scrapEaterConfigItems.Select(item => item.key).ToArray();
        int maxLength = Utils.GetLongestStringFromArray(keys).Length + 1;

        string message = "[Scrap Eater Settings]\n";
        
        scrapEaterConfigItems.ForEach(configItem =>
        {
            if (configItem.GetValue == null)
            {
                SellMyScrapBase.mls.LogError($"Error: GetValue() for \"{configItem.key}\" could not be found!");
                return;
            }

            message += $"{Utils.GetStringWithSpacingInBetween($"{configItem.key}:", configItem.GetValue(), maxLength)}\n";
        });

        return message.Trim();
    }
}

public class ConfigItem
{
    public string key;
    public Type type;
    public bool isHostOnly;
    public Action<string> SetValue;
    public Func<string> GetValue;

    public ConfigItem(string key, Type type, bool isHostOnly, Action<string> SetValue, Func<string> GetValue = null)
    {
        this.key = key;
        this.type = type;
        this.isHostOnly = isHostOnly;
        this.SetValue = SetValue;
        this.GetValue = GetValue;
    }
}
