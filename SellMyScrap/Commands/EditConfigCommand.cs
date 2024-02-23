using com.github.zehsteam.SellMyScrap.Patches;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class EditConfigCommand : Command
{
    private bool editingDontSellListJson = false;

    public override bool IsCommand(string[] args)
    {
        if (args[0] == "edit" && args[1] == "config") return true;
        if (args[0] == "edit-config") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        editingDontSellListJson = false;
        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(GetMainMessage());
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        if ((args[0] == "exit" || args[0] == "quit") && editingDontSellListJson)
        {
            editingDontSellListJson = false;
            return TerminalPatch.CreateTerminalNode(GetMainMessage());
        }

        if (args[0] == "exit" || args[0] == "quit")
        {
            awaitingConfirmation = false;
            return TerminalPatch.CreateTerminalNode("Closed config editor.\n\n");
        }

        if (editingDontSellListJson)
        {
            return EditDontSellListJson(args);
        }

        if (args[0] == "json" || args[0] == "dontselllistjson")
        {
            editingDontSellListJson = true;
            return TerminalPatch.CreateTerminalNode(GetJsonMessage());
        }

        if (args[0] != string.Empty && args[1] != string.Empty)
        {
            return EditConfigSettings(args);
        }

        return TerminalPatch.CreateTerminalNode(GetMainMessage());
    }

    private string GetMainMessage()
    {
        return GetMainMessage(string.Empty);
    }

    private string GetMainMessage(string additionMessage)
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        string syncedMessage = isHostOrServer ? string.Empty : " (Synced with host)";

        string message = $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += $"[Sell Settings]{syncedMessage}\n";
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
        message += $"speakInShip: {configManager.SpeakInShip}\n\n";
        message += $"The following commands are available:\n\n";
        message += $"<config> <value>\n";
        message += $"json\n";
        message += $"exit\n\n";
        message += additionMessage;

        return message;
    }

    private TerminalNode EditConfigSettings(string[] args)
    {
        string[] hostOnlySettings = ["sellgifts", "sellshotguns", "sellammo", "sellpickles", "sellscrapworthzero"];
        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        SyncedConfig syncedConfig = SellMyScrapBase.Instance.ConfigManager;

        string key = args[0];
        string value = args[1];

        if (hostOnlySettings.Contains(key))
        {
            return TerminalPatch.CreateTerminalNode(GetMainMessage("Only the host can edit this setting.\n\n"));
        }

        bool parsedBool = bool.TryParse(value, out bool booleanValue);
        string editedConfigSetting = string.Empty;

        if (key == "sellgifts" && parsedBool)
        {
            syncedConfig.SellGifts = booleanValue;
            editedConfigSetting = "sellGifts";
        }
        if (key == "sellshotguns" && parsedBool)
        {
            syncedConfig.SellShotguns = booleanValue;
            editedConfigSetting = "sellShotguns";
        }
        if (key == "sellammo" && parsedBool)
        {
            syncedConfig.SellAmmo = booleanValue;
            editedConfigSetting = "sellAmmo";
        }
        if (key == "sellpickles" && parsedBool)
        {
            syncedConfig.SellPickles = booleanValue;
            editedConfigSetting = "sellPickles";
        }
        if (key == "sellscrapworthzero" && parsedBool)
        {
            syncedConfig.SellScrapWorthZero = booleanValue;
            editedConfigSetting = "sellScrapWorthZero";
        }
        if (key == "overridewelcomemessage" && parsedBool)
        {
            syncedConfig.OverrideWelcomeMessage = booleanValue;
            editedConfigSetting = "overrideWelcomeMessage";
        }
        if (key == "overridehelpmessage" && parsedBool)
        {
            syncedConfig.OverrideHelpMessage = booleanValue;
            editedConfigSetting = "overrideHelpMessage";
        }
        if (key == "showfounditems" && parsedBool)
        {
            syncedConfig.ShowFoundItems = booleanValue;
            editedConfigSetting = "showFoundItems";
        }
        if (key == "sortfounditems" && parsedBool)
        {
            syncedConfig.SortFoundItems = booleanValue;
            editedConfigSetting = "sortFoundItems";
        }
        if (key == "alignfounditemsprice" && parsedBool)
        {
            syncedConfig.AlignFoundItemsPrice = booleanValue;
            editedConfigSetting = "alignFoundItemsPrice";
        }
        if (key == "speakinship" && parsedBool)
        {
            syncedConfig.SpeakInShip = booleanValue;
            editedConfigSetting = "speakInShip";
        }

        string additionalMessage = editedConfigSetting != string.Empty ? $"Set {editedConfigSetting} to {booleanValue}\n\n" : string.Empty;
        return TerminalPatch.CreateTerminalNode(GetMainMessage(additionalMessage));
    }

    private TerminalNode EditDontSellListJson(string[] args)
    {
        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

        if (!isHostOrServer)
        {
            return TerminalPatch.CreateTerminalNode(GetJsonMessage("Only the host can edit this setting.\n\n"));
        }

        SyncedConfig syncedConfig = SellMyScrapBase.Instance.ConfigManager;

        List<string> list = syncedConfig.DontSellListJson.ToList();
        string value = string.Join(" ", args).Replace(args[0], "").Replace("\"", "").Trim();

        if (args[0] == "add")
        {
            list.Add(value);
            syncedConfig.DontSellListJson = list.ToArray();
            return TerminalPatch.CreateTerminalNode(GetJsonMessage($"Added \"{value}\"\n\n"));
        }

        if (args[0] == "remove")
        {
            string key = GetListItem(list, value);

            if (list.Remove(key))
            {
                syncedConfig.DontSellListJson = list.ToArray();
                return TerminalPatch.CreateTerminalNode(GetJsonMessage($"Removed \"{key}\"\n\n"));
            }
        }

        return TerminalPatch.CreateTerminalNode(GetJsonMessage());
    }

    private string GetListItem(List<string> list, string data)
    {
        string result = string.Empty;

        list.ForEach(item =>
        {
            if (result != string.Empty) return;

            if (item.ToLower() == data)
            {
                result = item;
            }
        });

        return result;
    }

    private string GetJsonMessage()
    {
        return GetJsonMessage(string.Empty);
    }

    private string GetJsonMessage(string additionMessage)
    {
        string message = $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION} config editor\n\n";
        message += $"dontSellListJson editor\n\n";
        message += $"{JsonConvert.SerializeObject(SellMyScrapBase.Instance.ConfigManager.DontSellListJson)}\n\n";
        message += "The following commands are available:\n\n";
        message += "add <item>\n";
        message += "remove <item>\n";
        message += "exit\n\n";
        message += additionMessage;

        return message;
    }
}
