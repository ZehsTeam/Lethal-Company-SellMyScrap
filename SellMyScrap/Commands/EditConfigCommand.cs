using com.github.zehsteam.SellMyScrap.Patches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class EditConfigCommand : Command
{
    private StringArrayEditor _activeStringArrayEditor;
    private StringArrayEditor _dontSellListEditor;
    private StringArrayEditor _sellListEditor;
    private bool _inResetToDefaultMenu = false;

    public EditConfigCommand()
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        _dontSellListEditor = new StringArrayEditor("dontSellList", isHostOnly: true, () =>
        {
            return configManager.DontSellList;
        },
        value =>
        {
            configManager.DontSellList = value;
        });

        _sellListEditor = new StringArrayEditor("sellList", isHostOnly: true, () =>
        {
            return configManager.SellList;
        },
        value =>
        {
            configManager.SellList = value;
        });
    }

    public override bool IsCommand(ref string[] args)
    {
        return MatchesPattern(ref args, "edit", "config") || MatchesPattern(ref args, "edit-config");
    }

    public override TerminalNode Execute(string[] args)
    {
        _activeStringArrayEditor = null;
        _inResetToDefaultMenu = false;

        AwaitingConfirmation = true;
        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        // Handle the reset-to-default confirmation
        if (_inResetToDefaultMenu)
        {
            return ExecuteResetToDefaultConfirmation(args);
        }

        string[] _args = Utils.GetArrayToLower(args);
        string firstArg = _args.Length > 0 ? _args[0] : string.Empty;

        // Handle common exit commands
        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];

        if (exitStrings.Contains(firstArg))
        {
            return HandleExit();
        }

        // Handle active editor commands
        if (_activeStringArrayEditor != null)
        {
            return _activeStringArrayEditor.ExecuteConfirmation(args);
        }

        if (firstArg.Equals(_dontSellListEditor.Key, StringComparison.OrdinalIgnoreCase))
        {
            return SetActiveEditor(_dontSellListEditor);
        }

        if (firstArg.Equals(_sellListEditor.Key, StringComparison.OrdinalIgnoreCase))
        {
            return SetActiveEditor(_sellListEditor);
        }

        if (firstArg.Equals("reset", StringComparison.OrdinalIgnoreCase))
        {
            return EnterResetToDefaultMenu();
        }

        if (args.Length >= 2)
        {
            return EditConfigSettings(args);
        }

        return TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: invalid command."));
    }

    private TerminalNode HandleExit()
    {
        if (_activeStringArrayEditor != null)
        {
            _activeStringArrayEditor = null;
            return TerminalPatch.CreateTerminalNode(GetMessage());
        }

        AwaitingConfirmation = false;
        return TerminalPatch.CreateTerminalNode("Closed config editor.\n\n");
    }

    private TerminalNode SetActiveEditor(StringArrayEditor editor)
    {
        _activeStringArrayEditor = editor;
        return _activeStringArrayEditor.Execute();
    }

    private TerminalNode EnterResetToDefaultMenu()
    {
        _inResetToDefaultMenu = true;
        return ExecuteResetToDefault();
    }

    private string GetErrorMessage(string additionalMessage)
    {
        return GetMessage(Utils.GetStringWithColor(additionalMessage, TerminalPatch.RedColor));
    }

    private string GetMessage(string additionalMessage = "")
    {
        return $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n" +
               $"{ConfigHelper.GetConfigSettingsMessage()}\n\n" +
               $"The following commands are available:\n\n" +
               $"<key> <value>\nreset\nexit\n\n" +
               additionalMessage;
    }

    private TerminalNode EditConfigSettings(string[] args)
    {
        string key = args[0];
        string value = args[1];

        if (ConfigHelper.TrySetConfigValue(key, value, out ConfigItem configItem, out string parsedValue))
        {
            return TerminalPatch.CreateTerminalNode(GetMessage($"Set {configItem.Key} to {parsedValue}\n\n"));
        }

        string errorMessage = configItem == null ? "Error: invalid key.\n\n" :
                             configItem.IsHostOnly && !NetworkUtils.IsServer ? "Error: only the host can edit this setting.\n\n" :
                             "Error: invalid value.\n\n";

        return TerminalPatch.CreateTerminalNode(GetErrorMessage(errorMessage));
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

    private string GetResetToDefaultMessage(string additionalMessage = "")
    {
        return $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n" +
               "Are you sure you want to reset all config settings to their default value?\n\n" +
               "Please CONFIRM or DENY.\n\n" +
               additionalMessage;
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class StringArrayEditor
{
    public string Key { get; }
    public bool IsHostOnly { get; }
    private List<string> List;
    private Func<string[]> GetValue;
    private Action<string[]> SetValue;

    public StringArrayEditor(string key, bool isHostOnly, Func<string[]> getValue, Action<string[]> setValue)
    {
        Key = key;
        IsHostOnly = isHostOnly;
        GetValue = getValue;
        SetValue = setValue;
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
            return TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: only the host can edit this setting.\n\n"));
        }

        return _args[0] switch
        {
            "add" => Add(args),
            "remove" => Remove(args),
            "clear" when _args.Length > 1 && _args[1] == "all" => ClearAll(),
            _ => TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: invalid command.\n\n"))
        };
    }

    private TerminalNode Add(string[] args)
    {
        string item = GetItemName(args);

        if (string.IsNullOrEmpty(item))
        {
            return TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: invalid input.\n\n"));
        }

        List.Add(item);
        SetValue(List.ToArray());
        return TerminalPatch.CreateTerminalNode(GetMessage($"Added \"{item}\"\n\n"));
    }

    private TerminalNode Remove(string[] args)
    {
        string item = GetItemName(args);
        if (string.IsNullOrEmpty(item))
        {
            return TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: invalid input.\n\n"));
        }

        string _item = Utils.GetItemFromList(List, item);
        if (string.IsNullOrEmpty(_item))
        {
            return TerminalPatch.CreateTerminalNode(GetErrorMessage("Error: item was not found.\n\n"));
        }

        List.Remove(_item);
        SetValue(List.ToArray());
        return TerminalPatch.CreateTerminalNode(GetMessage($"Removed \"{_item}\"\n\n"));
    }

    private TerminalNode ClearAll()
    {
        List.Clear();
        SetValue(List.ToArray());
        return TerminalPatch.CreateTerminalNode(GetMessage("Removed all items.\n\n"));
    }

    private string GetItemName(string[] args)
    {
        return string.Join(" ", args.Skip(1)).Trim();
    }

    private string GetErrorMessage(string additionalMessage)
    {
        return GetMessage(Utils.GetStringWithColor(additionalMessage, TerminalPatch.RedColor));
    }

    private string GetMessage(string additionalMessage = "")
    {
        return $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n" +
               $"{Key} config editor\n\n{Utils.GetStringWithColor(string.Join(", ", List), TerminalPatch.GreenColor2)}\n\n" +
               "The following commands are available:\n\n" +
               "add <value>\nremove <value>\nclear all\nexit\n\n" +
               additionalMessage;
    }
}
