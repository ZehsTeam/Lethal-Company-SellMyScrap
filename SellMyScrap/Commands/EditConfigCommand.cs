﻿using com.github.zehsteam.SellMyScrap.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class EditConfigCommand : Command
{
    private bool editingDontSellListJson = false;
    private JsonListEditor dontSellListJsonEditor;

    public EditConfigCommand()
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        dontSellListJsonEditor = new JsonListEditor("dontSellListJson", isHostOnly: true, configManager.DontSellListJson.ToList(), value =>
        {
            configManager.DontSellListJson = value;
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
        editingDontSellListJson = false;
        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        string[] _args = Utils.GetArrayToLower(args);

        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];
        
        if (exitStrings.Contains(_args[0]))
        {
            if (editingDontSellListJson)
            {
                editingDontSellListJson = false;
                return TerminalPatch.CreateTerminalNode(GetMessage());
            }

            awaitingConfirmation = false;
            return TerminalPatch.CreateTerminalNode("Closed config editor.\n\n");
        }

        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        if (editingDontSellListJson)
        {
            return dontSellListJsonEditor.ExecuteConfirmation(args, configManager.DontSellListJson.ToList());
        }

        if (_args[0] == "json" || _args[0] == "dontselllistjson")
        {
            editingDontSellListJson = true;
            return dontSellListJsonEditor.Execute(configManager.DontSellListJson.ToList());
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
        message += $"json\n";
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
            return TerminalPatch.CreateTerminalNode(GetMessage($"Set {configItem.key} to {parsedValue}\n\n"));
        }

        if (configItem == null)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid key.\n\n"));
        }

        if (configItem.isHostOnly && !SellMyScrapBase.IsHostOrServer)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: only the host can change this setting.\n\n"));
        }

        return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid value.\n\n"));
    }
}

class JsonListEditor
{
    public string key;
    public bool isHostOnly = false;
    private List<string> list;
    private Action<string[]> SetValue;

    public JsonListEditor(string key, bool isHostOnly, List<string> list, Action<string[]> SetValue)
    {
        this.key = key;
        this.isHostOnly = isHostOnly;
        this.list = list;
        this.SetValue = SetValue;
    }

    public TerminalNode Execute(List<string> list)
    {
        this.list = list;
        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    public TerminalNode ExecuteConfirmation(string[] args, List<string> list)
    {
        string[] _args = Utils.GetArrayToLower(args);
        this.list = list;

        if (isHostOnly && !SellMyScrapBase.IsHostOrServer)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage($"Error: only the host can change this setting.\n\n"));
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

        list.Add(item);
        SetValue(list.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Added \"{item}\"\n\n"));
    }

    private TerminalNode Remove(string[] args)
    {
        string item = GetItemName(args);

        if (item == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid input.\n\n"));
        }

        string _item = Utils.GetItemFromList(list, item);

        if (_item == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: item was not found.\n\n"));
        }

        list.Remove(_item);
        SetValue(list.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Removed \"{_item}\"\n\n"));
    }

    private TerminalNode ClearAll(string[] args)
    {
        list = [];
        SetValue(list.ToArray());

        return TerminalPatch.CreateTerminalNode(GetMessage($"Removed all items.\n\n"));
    }

    private string GetItemName(string[] args)
    {
        return string.Join(" ", args).Substring(args[0].Length).Replace("\"", "").Replace("\\", "").Trim();
    }

    private string GetMessage(string additionalMessage = "")
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += $"{key} config editor\n\n";
        message += $"{JsonConvert.SerializeObject(list)}\n\n";
        message += $"The following commands are available:\n\n";
        message += $"add <value>\n";
        message += $"remove <value>\n";
        message += $"clear all\n";
        message += $"exit\n\n";
        message += additionalMessage;

        return message;
    }
}
