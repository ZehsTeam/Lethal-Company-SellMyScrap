using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Dependencies.LethalConfigMod;
using com.github.zehsteam.SellMyScrap.Objects;
using System;

namespace com.github.zehsteam.SellMyScrap.Helpers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static class ConfigHelper
{
    #region LethalConfig
    public static void SkipAutoGen()
    {
        if (LethalConfigProxy.IsEnabled)
        {
            LethalConfigProxy.SkipAutoGen();
        }
    }

    public static void AddButton(string section, string name, string description, string buttonText, Action callback)
    {
        if (LethalConfigProxy.IsEnabled)
        {
            LethalConfigProxy.AddButton(section, name, description, buttonText, callback);
        }
    }
    #endregion

    public static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, bool requiresRestart, string description, AcceptableValueBase acceptableValues = null, Action<T> settingChanged = null, ConfigFile configFile = null)
    {
        configFile ??= Plugin.Instance.Config;

        var configEntry = acceptableValues == null
            ? configFile.Bind(section, key, defaultValue, description)
            : configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues));

        if (settingChanged != null)
        {
            configEntry.SettingChanged += (sender, e) => settingChanged?.Invoke(configEntry.Value);
        }

        if (LethalConfigProxy.IsEnabled)
        {
            LethalConfigProxy.AddConfig(configEntry, requiresRestart);
        }

        return configEntry;
    }

    public static SyncedConfigEntry<T> BindSynced<T>(string section, string key, T defaultValue, string description, AcceptableValueBase acceptableValues = null, Action<T> settingChanged = null, ConfigFile configFile = null)
    {
        SyncedConfigEntry<T> syncedConfigEntry = new SyncedConfigEntry<T>(section, key, defaultValue, description, acceptableValues, configFile);

        if (settingChanged != null)
        {
            syncedConfigEntry.SettingChanged += settingChanged;
        }

        return syncedConfigEntry;
    }
}
