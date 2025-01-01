using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using System;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.SellMyScrap.Dependencies;

internal static class LethalConfigProxy
{
    public const string PLUGIN_GUID = "ainavt.lc.lethalconfig";
    public static bool Enabled
    {
        get
        {
            _enabled ??= Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);
            return _enabled.Value;
        }
    }

    private static bool? _enabled;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SkipAutoGen()
    {
        LethalConfigManager.SkipAutoGen();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfig<T>(ConfigEntry<T> configEntry, bool requiresRestart = false)
    {
        // Check if the ConfigEntry has an AcceptableValueBase
        if (configEntry.Description.AcceptableValues is AcceptableValueBase acceptableValue)
        {
            // Check if it is an AcceptableValueRange for either float or int
            if (acceptableValue is AcceptableValueRange<float> || acceptableValue is AcceptableValueRange<int>)
            {
                AddConfigSlider(configEntry, requiresRestart);
                return;
            }
            // Check if it is an AcceptableValueList for string
            else if (acceptableValue is AcceptableValueList<string>)
            {
                AddConfigDropdown(configEntry, requiresRestart);
                return;
            }
        }

        // Use pattern matching or type checks to determine which type-specific ConfigItem to create
        switch (configEntry)
        {
            case ConfigEntry<string> strEntry:
                LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(strEntry, requiresRestart));
                break;
            case ConfigEntry<bool> boolEntry:
                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(boolEntry, requiresRestart));
                break;
            case ConfigEntry<float> floatEntry:
                LethalConfigManager.AddConfigItem(new FloatInputFieldConfigItem(floatEntry, requiresRestart));
                break;
            case ConfigEntry<int> intEntry:
                LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(intEntry, requiresRestart));
                break;
            default:
                throw new NotSupportedException($"Unsupported type: {typeof(T)}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigSlider<T>(ConfigEntry<T> configEntry, bool requiresRestart = false)
    {
        // Handle sliders for float and int specifically
        switch (configEntry)
        {
            case ConfigEntry<float> floatEntry:
                LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(floatEntry, requiresRestart));
                break;
            case ConfigEntry<int> intEntry:
                LethalConfigManager.AddConfigItem(new IntSliderConfigItem(intEntry, requiresRestart));
                break;
            default:
                throw new NotSupportedException($"Slider not supported for type: {typeof(T)}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigDropdown<T>(ConfigEntry<T> configEntry, bool requiresRestart = false)
    {
        // Handle dropdown for string or enum-like entries
        switch (configEntry)
        {
            case ConfigEntry<string> stringEntry:
                LethalConfigManager.AddConfigItem(new TextDropDownConfigItem(stringEntry, requiresRestart));
                break;
            default:
                throw new NotSupportedException($"Dropdown not supported for type: {typeof(T)}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddButton(string section, string name, string description, string buttonText, Action callback)
    {
        LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(section, name, description, buttonText, () => callback?.Invoke()));
    }
}
