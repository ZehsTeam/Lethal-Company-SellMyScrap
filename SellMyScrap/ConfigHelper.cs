﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

internal class ConfigHelper
{
    private static List<ConfigItem> configItems = new List<ConfigItem>();

    public static void Initialize()
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        configItems = [
            new ConfigItem("sellGifts",              typeof(bool), isHostOnly: true,  value => { configManager.SellGifts =              bool.Parse(value); }),
            new ConfigItem("sellShotguns",           typeof(bool), isHostOnly: true,  value => { configManager.SellShotguns =           bool.Parse(value); }),
            new ConfigItem("sellAmmo",               typeof(bool), isHostOnly: true,  value => { configManager.SellAmmo =               bool.Parse(value); }),
            new ConfigItem("sellPickles",            typeof(bool), isHostOnly: true,  value => { configManager.SellPickles =            bool.Parse(value); }),
            new ConfigItem("sellScrapWorthZero",     typeof(bool), isHostOnly: true,  value => { configManager.SellScrapWorthZero =     bool.Parse(value); }),
            new ConfigItem("overrideWelcomeMessage", typeof(bool), isHostOnly: false, value => { configManager.OverrideWelcomeMessage = bool.Parse(value); }),
            new ConfigItem("overrideHelpMessage",    typeof(bool), isHostOnly: false, value => { configManager.OverrideHelpMessage =    bool.Parse(value); }),
            new ConfigItem("showFoundItems",         typeof(bool), isHostOnly: false, value => { configManager.ShowFoundItems =         bool.Parse(value); }),
            new ConfigItem("sortFoundItems",         typeof(bool), isHostOnly: false, value => { configManager.SortFoundItems =         bool.Parse(value); }),
            new ConfigItem("alignFoundItemsPrice",   typeof(bool), isHostOnly: false, value => { configManager.AlignFoundItemsPrice =   bool.Parse(value); }),
            new ConfigItem("speakInShip",            typeof(bool), isHostOnly: false, value => { configManager.SpeakInShip =            bool.Parse(value); }),
        ];
    }

    public static bool TrySetConfigValue(string key, string value, out ConfigItem configItem, out string parsedValue)
    {
        configItem = GetConfigItem(key);
        parsedValue = string.Empty;

        if (configItem == null) return false;

        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        if (!isHostOrServer && configItem.isHostOnly) return false;

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

        return false;
    }

    private static ConfigItem GetConfigItem(string key)
    {
        foreach (ConfigItem item in configItems)
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

        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        string syncedMessage = isHostOrServer ? string.Empty : " (Synced with host)";

        string message = $"[Sell Settings]{syncedMessage}\n";
        message += $"sellGifts:    {configManager.SellGifts}\n";
        message += $"sellShotguns: {configManager.SellShotguns}\n";
        message += $"sellAmmo:     {configManager.SellAmmo}\n";
        message += $"sellPickles:  {configManager.SellPickles}\n\n";
        message += $"[Advanced Sell Settings]{syncedMessage}\n";
        message += $"sellScrapWorthZero: {configManager.SellScrapWorthZero}\n";
        message += $"dontSellListJson: {JsonConvert.SerializeObject(configManager.DontSellListJson)}\n\n";
        message += "[Terminal Settings]\n";
        message += $"overrideWelcomeMessage: {configManager.OverrideWelcomeMessage}\n";
        message += $"overrideHelpMessage:    {configManager.OverrideHelpMessage}\n";
        message += $"showFoundItems:         {configManager.ShowFoundItems}\n";
        message += $"sortFoundItems:         {configManager.SortFoundItems}\n";
        message += $"alignFoundItemsPrice:   {configManager.AlignFoundItemsPrice}\n\n";
        message += "[Misc Settings]\n";
        message += $"speakInShip: {configManager.SpeakInShip}";

        return message;
    }
}

public class ConfigItem
{
    public string key;
    public Type type;
    public bool isHostOnly;
    public Action<string> SetValue;

    public ConfigItem(string key, Type type, bool isHostOnly, Action<string> SetValue)
    {
        this.key = key;
        this.type = type;
        this.isHostOnly = isHostOnly;
        this.SetValue = SetValue;
    }
}