﻿using BepInEx.Logging;

namespace com.github.zehsteam.SellMyScrap;

internal static class Logger
{
    public static ManualLogSource ManualLogSource { get; private set; }

    public static void Initialize(ManualLogSource manualLogSource)
    {
        ManualLogSource = manualLogSource;
    }

    public static void LogDebug(object data)
    {
        Log(LogLevel.Debug, data);
    }

    public static void LogInfo(object data, bool extended = false)
    {
        Log(LogLevel.Info, data, extended);
    }

    public static void LogWarning(object data, bool extended = false)
    {
        Log(LogLevel.Warning, data, extended);
    }

    public static void LogError(object data, bool extended = false)
    {
        Log(LogLevel.Error, data, extended);
    }

    public static void LogFatal(object data, bool extended = false)
    {
        Log(LogLevel.Fatal, data, extended);
    }

    public static void Log(LogLevel logLevel, object data, bool extended = false)
    {
        if (extended && !IsExtendedLoggingEnabled())
        {
            return;
        }

        ManualLogSource?.Log(logLevel, data);
    }

    public static bool IsExtendedLoggingEnabled()
    {
        if (ConfigManager.ExtendedLogging == null)
        {
            return false;
        }

        return ConfigManager.ExtendedLogging.Value;
    }
}
