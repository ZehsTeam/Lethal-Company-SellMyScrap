using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Dependencies.LethalConfigProxy;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class SyncedConfigEntry<T> : SyncedConfigEntryBase
{
    public T Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public T DefaultValue => (T)_configEntry.DefaultValue;

    public override string Section => _configEntry.Definition.Section;
    public override string Key => _configEntry.Definition.Key;

    public event Action<T> SettingChanged;

    private ConfigEntry<T> _configEntry;
    private T _serverValue;

    public SyncedConfigEntry(string section, string key, T defaultValue, string description, AcceptableValueBase acceptableValues = null, ConfigFile configFile = null)
    {
        AddInstance(this);

        _configEntry = ConfigHelper.Bind(section, key, defaultValue, requiresRestart: false, description, acceptableValues: acceptableValues, configFile: configFile);
        _configEntry.SettingChanged += SettingChangedInternal;
    }

    public T GetValue()
    {
        if (NetworkUtils.IsConnected && !NetworkUtils.IsServer)
        {
            return _serverValue;
        }

        return _configEntry.Value;
    }

    public void SetValue(T value)
    {
        if (NetworkUtils.IsConnected && !NetworkUtils.IsServer)
        {
            return;
        }

        _configEntry.Value = value;
    }

    public void ResetToDefault()
    {
        if (NetworkUtils.IsConnected && !NetworkUtils.IsServer)
        {
            return;
        }

        _configEntry.Value = DefaultValue;
    }

    private void SettingChangedInternal(object sender, EventArgs e)
    {
        if (!NetworkUtils.IsConnected || !NetworkUtils.IsServer) return;

        SettingChanged?.Invoke(Value);
        SendConfigToClients();
    }

    private void SendConfigToClients()
    {
        if (!NetworkUtils.IsConnected || !NetworkUtils.IsServer) return;

        PluginNetworkBehaviour.Instance?.SetSyncedConfigValueClientRpc(Section, Key, Value.ToString());
    }

    public override void SendConfigToClient(ulong clientId)
    {
        if (!NetworkUtils.IsConnected || !NetworkUtils.IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        PluginNetworkBehaviour.Instance?.SetSyncedConfigValueClientRpc(Section, Key, Value.ToString(), clientRpcParams);
    }

    public override void SetValueFromServer(string value)
    {
        if (!NetworkUtils.IsConnected || NetworkUtils.IsServer) return;

        if (Utils.TryParseValue(value, out T parsedValue))
        {
            _serverValue = parsedValue;

            Plugin.Instance.LogInfoExtended($"Set synced config entry value from server. (Section: \"{Section}\", Key: \"{Key}\", Value: \"{value}\")");

            SettingChanged?.Invoke(parsedValue);

        }
        else
        {
            throw new InvalidOperationException($"Failed to parse value: \"{value}\" for type {typeof(T)}");
        }
    }
}

public abstract class SyncedConfigEntryBase
{
    public static List<SyncedConfigEntryBase> Instances { get; private set; } = [];

    public abstract string Section { get; }
    public abstract string Key { get; }

    private static readonly object _instancesLock = new object();

    public static void AddInstance(SyncedConfigEntryBase instance)
    {
        lock (_instancesLock)
        {
            Instances.Add(instance);
        }
    }

    public static void RemoveInstance(SyncedConfigEntryBase instance)
    {
        lock (_instancesLock)
        {
            Instances.Remove(instance);
        }
    }

    public abstract void SendConfigToClient(ulong clientId);

    public abstract void SetValueFromServer(string value);

    public static void SendConfigsToClient(ulong clientId)
    {
        if (!NetworkUtils.IsConnected || !NetworkUtils.IsServer) return;

        if (NetworkUtils.IsLocalClientId(clientId))
        {
            return;
        }

        foreach (var instance in Instances)
        {
            instance.SendConfigToClient(clientId);
        }
    }

    public static void SetValueFromServer(string section, string key, string value)
    {
        if (!NetworkUtils.IsConnected || NetworkUtils.IsServer) return;

        SyncedConfigEntryBase syncedConfigEntryBase = Instances.Find(x => x.Section == section && x.Key == key);

        if (syncedConfigEntryBase == null)
        {
            Plugin.Logger.LogWarning($"No matching synced config entry found for section: \"{section}\", key: \"{key}\"");
            return;
        }

        syncedConfigEntryBase.SetValueFromServer(value);
    }
}
