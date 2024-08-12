using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class ConfigHelper
{
    // Config Items
    private static List<ConfigItem> _sellConfigItems = [];
    private static List<ConfigItem> _advancedSellConfigItems = [];
    private static List<ConfigItem> _terminalConfigItems = [];
    private static List<ConfigItem> _miscConfigItems = [];
    private static List<ConfigItem> _scrapEaterConfigItems = [];

    private static List<ConfigItem> _allConfigItems
    {
        get
        {
            return [
                .. _sellConfigItems,
                .. _advancedSellConfigItems,
                .. _terminalConfigItems,
                .. _miscConfigItems,
                .. _scrapEaterConfigItems,
            ];
        }
    }

    internal static void Initialize()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        _sellConfigItems = [
            new ConfigItem("SellGifts",    typeof(bool), isHostOnly: true, value => { configManager.SellGifts =    bool.Parse(value); }, () => { return configManager.SellGifts.ToString();    }),
            new ConfigItem("SellShotguns", typeof(bool), isHostOnly: true, value => { configManager.SellShotguns = bool.Parse(value); }, () => { return configManager.SellShotguns.ToString(); }),
            new ConfigItem("SellAmmo",     typeof(bool), isHostOnly: true, value => { configManager.SellAmmo =     bool.Parse(value); }, () => { return configManager.SellAmmo.ToString();     }),
            new ConfigItem("SellKnives",   typeof(bool), isHostOnly: true, value => { configManager.SellKnives =   bool.Parse(value); }, () => { return configManager.SellKnives.ToString();   }),
            new ConfigItem("SellPickles",  typeof(bool), isHostOnly: true, value => { configManager.SellPickles =  bool.Parse(value); }, () => { return configManager.SellPickles.ToString();  }),
        ];

        _advancedSellConfigItems = [
            new ConfigItem("SellScrapWorthZero",   typeof(bool),     isHostOnly: true, value => { configManager.SellScrapWorthZero =   bool.Parse(value); }, () => { return configManager.SellScrapWorthZero.ToString();   }),
            new ConfigItem("OnlySellScrapOnFloor", typeof(bool),     isHostOnly: true, value => { configManager.OnlySellScrapOnFloor = bool.Parse(value); }, () => { return configManager.OnlySellScrapOnFloor.ToString(); }),
            new ConfigItem("DontSellListJson",     typeof(string[]), isHostOnly: true, value => { configManager.DontSellListJson = JsonConvert.DeserializeObject<string[]>(value); }, () => { return JsonConvert.SerializeObject(configManager.DontSellListJson); }),
            new ConfigItem("SellListJson",         typeof(string[]), isHostOnly: true, value => { configManager.SellListJson = JsonConvert.DeserializeObject<string[]>(value); }, () => { return JsonConvert.SerializeObject(configManager.SellListJson); }),
        ];

        _terminalConfigItems = [
            new ConfigItem("OverrideWelcomeMessage", typeof(bool), isHostOnly: false, value => { configManager.OverrideWelcomeMessage = bool.Parse(value); }, () => { return configManager.OverrideWelcomeMessage.ToString(); }),
            new ConfigItem("OverrideHelpMessage",    typeof(bool), isHostOnly: false, value => { configManager.OverrideHelpMessage =    bool.Parse(value); }, () => { return configManager.OverrideHelpMessage.ToString();    }),
            new ConfigItem("ShowFoundItems",         typeof(bool), isHostOnly: false, value => { configManager.ShowFoundItems =         bool.Parse(value); }, () => { return configManager.ShowFoundItems.ToString();         }),
            new ConfigItem("SortFoundItemsPrice",    typeof(bool), isHostOnly: false, value => { configManager.SortFoundItemsPrice =    bool.Parse(value); }, () => { return configManager.SortFoundItemsPrice.ToString();    }),
            new ConfigItem("AlignFoundItemsPrice",   typeof(bool), isHostOnly: false, value => { configManager.AlignFoundItemsPrice =   bool.Parse(value); }, () => { return configManager.AlignFoundItemsPrice.ToString();   }),
        ];

        _miscConfigItems = [
            new ConfigItem("SpeakInShip",         typeof(bool),  isHostOnly: false, value => { configManager.SpeakInShip =         bool.Parse(value);  }, () => { return configManager.SpeakInShip.ToString();         }),
            new ConfigItem("RareVoiceLineChance", typeof(float), isHostOnly: true,  value => { configManager.RareVoiceLineChance = float.Parse(value); }, () => { return configManager.RareVoiceLineChance.ToString(); }),
            new ConfigItem("ShowQuotaWarning",    typeof(bool),  isHostOnly: false, value => { configManager.ShowQuotaWarning =    bool.Parse(value);  }, () => { return configManager.ShowQuotaWarning.ToString();    }),
        ];

        _scrapEaterConfigItems = [
            new ConfigItem("ScrapEaterChance",      typeof(int), isHostOnly: true, value => { configManager.ScrapEaterChance =      int.Parse(value); }, () => { return configManager.ScrapEaterChance.ToString();      }),
            new ConfigItem("OctolarSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.OctolarSpawnWeight =    int.Parse(value); }, () => { return configManager.OctolarSpawnWeight.ToString();    }),
            new ConfigItem("TakeySpawnWeight",      typeof(int), isHostOnly: true, value => { configManager.TakeySpawnWeight =      int.Parse(value); }, () => { return configManager.TakeySpawnWeight.ToString();      }),
            new ConfigItem("MaxwellSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.MaxwellSpawnWeight =    int.Parse(value); }, () => { return configManager.MaxwellSpawnWeight.ToString();    }),
            new ConfigItem("YippeeSpawnWeight",     typeof(int), isHostOnly: true, value => { configManager.YippeeSpawnWeight =     int.Parse(value); }, () => { return configManager.YippeeSpawnWeight.ToString();     }),
            new ConfigItem("CookieFumoSpawnWeight", typeof(int), isHostOnly: true, value => { configManager.CookieFumoSpawnWeight = int.Parse(value); }, () => { return configManager.CookieFumoSpawnWeight.ToString(); }),
            new ConfigItem("PsychoSpawnWeight",     typeof(int), isHostOnly: true, value => { configManager.PsychoSpawnWeight =     int.Parse(value); }, () => { return configManager.PsychoSpawnWeight.ToString();     }),
            new ConfigItem("ZombiesSpawnWeight",    typeof(int), isHostOnly: true, value => { configManager.ZombiesSpawnWeight =    int.Parse(value); }, () => { return configManager.ZombiesSpawnWeight.ToString();    }),
        ];
    }

    internal static bool TrySetConfigValue(string key, string value, out ConfigItem configItem, out string parsedValue)
    {
        parsedValue = string.Empty;

        configItem = GetConfigItem(key);
        if (configItem == null) return false;

        if (configItem.IsHostOnly && !Plugin.IsHostOrServer) return false;

        if (configItem.Type == typeof(bool))
        {
            return TrySetBoolConfigValue(configItem, value, out parsedValue);
        }

        if (configItem.Type == typeof(int))
        {
            return TrySetIntConfigValue(configItem, value, out parsedValue);
        }

        if (configItem.Type == typeof(float))
        {
            return TrySetFloatConfigValue(configItem, value, out parsedValue);
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

        if (int.TryParse(value, out int parsedInt))
        {
            configItem.SetValue(value);
            parsedValue = parsedInt.ToString();

            return true;
        }

        return false;
    }

    private static bool TrySetFloatConfigValue(ConfigItem configItem, string value, out string parsedValue)
    {
        parsedValue = string.Empty;

        if (float.TryParse(value, out float parsedFloat))
        {
            configItem.SetValue(value);
            parsedValue = parsedFloat.ToString();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Link your scrap eater SpawnWeight config setting to the SellMyScrap terminal config editor.
    /// </summary>
    /// <param name="key">SpawnWeight config setting key.</param>
    /// <param name="SetValue">Action for setting your spawnWeight config setting value.</param>
    /// <param name="GetValue">Func for getting your spawnWeight config setting value.</param>
    public static void AddScrapEaterConfigItem(string key, Action<string> SetValue, Func<string> GetValue)
    {
        _scrapEaterConfigItems.Add(new ConfigItem(key, typeof(int), isHostOnly: true, SetValue, GetValue));
    }

    private static ConfigItem GetConfigItem(string key)
    {
        foreach (ConfigItem configItem in _allConfigItems)
        {
            if (configItem.Key.ToLower() == key.ToLower())
            {
                return configItem;
            }
        }

        return null;
    }

    internal static string GetConfigSettingsMessage()
    {
        string message = string.Empty;
        message += GetConfigItemListMessage("[Sell Settings]", _sellConfigItems, syncedWithHost: true);
        message += GetConfigItemListMessage("[Advanced Sell Settings]", _advancedSellConfigItems, syncedWithHost: true);
        message += GetConfigItemListMessage("[Terminal Settings]", _terminalConfigItems);
        message += GetConfigItemListMessage("[Misc Settings]", _miscConfigItems);
        message += GetConfigItemListMessage("[Scrap Eater Settings]", _scrapEaterConfigItems, hostOnly: true);

        return message.Trim();
    }

    private static string GetConfigItemListMessage(string header, List<ConfigItem> list, bool syncedWithHost = false, bool hostOnly = false)
    {
        string[] keys = list.Select(item => item.Key).ToArray();
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
                Plugin.logger.LogError($"Func<string> GetValue() for ConfigItem key: \"{configItem.Key}\" could not be found!");
                return;
            }

            message += $"{Utils.GetStringWithSpacingInBetween($"{configItem.Key}:", configItem.GetValue(), maxLength)}\n";
        });

        return $"{message.Trim()}\n\n";
    }
}

public class ConfigItem
{
    public string Key;
    public Type Type;
    public bool IsHostOnly;
    public Action<string> SetValue;
    public Func<string> GetValue;

    public ConfigItem(string key, Type type, bool isHostOnly, Action<string> SetValue, Func<string> GetValue)
    {
        Key = key;
        Type = type;
        IsHostOnly = isHostOnly;
        this.SetValue = SetValue;
        this.GetValue = GetValue;
    }
}
