using com.github.zehsteam.SellMyScrap.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class EditConfigCommand : Command
{
    private JsonListEditor _activeJsonListEditor;
    private JsonListEditor _dontSellListJsonEditor;
    private JsonListEditor _sellListJsonEditor;

    private bool _inResetToDefaultMenu = false;

    public EditConfigCommand()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        _dontSellListJsonEditor = new JsonListEditor("dontSellListJson", isHostOnly: true, () =>
        {
            return configManager.DontSellListJson;
        },
        value =>
        {
            configManager.DontSellListJson = value;
        });

        _sellListJsonEditor = new JsonListEditor("sellListJson", isHostOnly: true, () =>
        {
            return configManager.SellListJson;
        },
        value =>
        {
            configManager.SellListJson = value;
        });
    }

    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "edit" && args[1] == "config") return true;
        if (args[0] == "edit-config") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        _activeJsonListEditor = null;
        _inResetToDefaultMenu = false;

        AwaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        if (_inResetToDefaultMenu)
        {
            return ExecuteResetToDefaultConfirmation(args);
        }

        string[] _args = Utils.GetArrayToLower(args);

        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];
        
        if (exitStrings.Contains(_args[0]))
        {
            if (_activeJsonListEditor != null)
            {
                _activeJsonListEditor = null;
                return TerminalPatch.CreateTerminalNode(GetMessage());
            }

            AwaitingConfirmation = false;
            return TerminalPatch.CreateTerminalNode("Closed config editor.\n\n");
        }

        SyncedConfigManager configManager = Plugin.ConfigManager;

        if (_activeJsonListEditor != null)
        {
            return _activeJsonListEditor.ExecuteConfirmation(args);
        }

        if (_args[0] == "dontselllistjson")
        {
            _activeJsonListEditor = _dontSellListJsonEditor;
            return _activeJsonListEditor.Execute();
        }

        if (_args[0] == "selllistjson")
        {
            _activeJsonListEditor = _sellListJsonEditor;
            return _activeJsonListEditor.Execute();
        }

        if (_args[0] == "reset")
        {
            _inResetToDefaultMenu = true;
            return ExecuteResetToDefault();
        }

        if (_args[0] != string.Empty && _args[1] != string.Empty)
        {
            return EditConfigSettings(args);
        }

        return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid command.\n\n"));
    }

    private string GetMessage(string additionMessage = "")
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += $"{ConfigHelper.GetConfigSettingsMessage()}\n\n";
        message += $"The following commands are available:\n\n";
        message += $"<key> <value>\n";
        message += $"reset\n";
        message += $"exit\n\n";
        message += additionMessage;

        return message;
    }

    private TerminalNode EditConfigSettings(string[] args)
    {
        string key = args[0];
        string value = args[1];

        if (ConfigHelper.TrySetConfigValue(key, value, out ConfigItem configItem, out string parsedValue))
        {
            return TerminalPatch.CreateTerminalNode(GetMessage($"Set {configItem.Key} to {parsedValue}\n\n"));
        }

        if (configItem == null)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid key.\n\n"));
        }

        if (configItem.IsHostOnly && !NetworkUtils.IsServer)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: only the host can edit this setting.\n\n"));
        }

        return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid value.\n\n"));
    }

    private TerminalNode ExecuteResetToDefault()
    {
        return TerminalPatch.CreateTerminalNode(GetResetToDefaultMessage());
    }

    private TerminalNode ExecuteResetToDefaultConfirmation(string[] args)
    {
        string arg = args[0].ToLower();

        if ("confirm".Contains(arg) && arg.Length > 0)
        {
            Plugin.ConfigManager.ResetToDefault();
            _inResetToDefaultMenu = false;
            return TerminalPatch.CreateTerminalNode(GetMessage("Reset all config settings to their default value.\n\n"));
        }

        if ("deny".Contains(arg) && arg.Length > 0)
        {
            _inResetToDefaultMenu = false;
            return TerminalPatch.CreateTerminalNode(GetMessage());
        }

        return TerminalPatch.CreateTerminalNode(GetResetToDefaultMessage());
    }

    private string GetResetToDefaultMessage(string additionMessage = "")
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += "Are you sure you want to reset all config settings to their default value?\n\n";
        message += "Please CONFIRM or DENY.\n\n";
        message += additionMessage;

        return message;
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class JsonListEditor
{
    public string Key;
    public bool IsHostOnly = false;
    private List<string> List;
    private Func<string[]> GetValue;
    private Action<string[]> SetValue;

    public JsonListEditor(string key, bool isHostOnly, Func<string[]> GetValue, Action<string[]> SetValue)
    {
        Key = key;
        IsHostOnly = isHostOnly;
        this.GetValue = GetValue;
        this.SetValue = SetValue;
    }

    public TerminalNode Execute()
    {
        List = GetValue().ToList();
        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    public TerminalNode ExecuteConfirmation(string[] args)
    {
        string[] _args = Utils.GetArrayToLower(args);
        List = GetValue().ToList();

        if (IsHostOnly && !NetworkUtils.IsServer)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage($"Error: only the host can edit this setting.\n\n"));
        }

        if (_args[1] == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid input.\n\n"));
        }

        if (_args[0] == "add")
        {
            return Add(args);
        }

        if (_args[0] == "remove")
        {
            return Remove(args);
        }

        if (_args[0] == "clear" && _args[1] == "all")
        {
            return ClearAll(args);
        }

        return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid command.\n\n"));
    }

    private TerminalNode Add(string[] args)
    {
        string item = GetItemName(args);

        if (item == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid input.\n\n"));
        }

        List.Add(item);
        SetValue(List.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Added \"{item}\"\n\n"));
    }

    private TerminalNode Remove(string[] args)
    {
        string item = GetItemName(args);

        if (item == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid input.\n\n"));
        }

        string _item = Utils.GetItemFromList(List, item);

        if (_item == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: item was not found.\n\n"));
        }

        List.Remove(_item);
        SetValue(List.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Removed \"{_item}\"\n\n"));
    }

    private TerminalNode ClearAll(string[] args)
    {
        List = [];
        SetValue(List.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Removed all items.\n\n"));
    }

    private string GetItemName(string[] args)
    {
        return string.Join(" ", args).Substring(args[0].Length).Replace("\"", "").Replace("\\", "").Trim();
    }

    private string GetMessage(string additionalMessage = "")
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += $"{Key} config editor\n\n";
        message += $"{JsonConvert.SerializeObject(List)}\n\n";
        message += $"The following commands are available:\n\n";
        message += $"add <value>\n";
        message += $"remove <value>\n";
        message += $"clear all\n";
        message += $"exit\n\n";
        message += additionalMessage;
        
        return message;
    }
}
