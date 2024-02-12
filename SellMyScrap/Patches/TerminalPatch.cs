using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    private static bool hasOverrideTerminalNodes = false;

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    [HarmonyPriority(int.MaxValue)]
    static void StartPatch(ref TerminalNodesList ___terminalNodes)
    {
        OverrideTerminalNodes(___terminalNodes);
    }

    #region TerminalNode Overrides
    private static void OverrideTerminalNodes(TerminalNodesList terminalNodes)
    {
        if (hasOverrideTerminalNodes) return;
        hasOverrideTerminalNodes = true;

        if (SellMyScrapBase.Instance.ConfigManager.OverrideWelcomeMessage) OverrideWelcomeTerminalNode(terminalNodes);
        if (SellMyScrapBase.Instance.ConfigManager.OverrideHelpMessage) OverrideHelpTerminalNode(terminalNodes);
    }

    private static void OverrideWelcomeTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 1;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;
        
        string message = defaultMessage.Trim();
        message += $"\n\n[SellMyScrap v{MyPluginInfo.PLUGIN_VERSION}]\nType \"Sell\" for a list of commands.\n\n\n\n";

        terminalNodes.specialNodes[index].displayText = message;
    }

    private static void OverrideHelpTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 13;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;
        
        string message = defaultMessage.Replace("[numberOfItemsOnRoute]", "").Trim();
        message += "\n\n>SELL\nTo see the list of SellMyScrap commands.\n\n";

        terminalNodes.specialNodes[index].displayText = message;
    }
    #endregion

    [HarmonyPatch("ParsePlayerSentence")]
    [HarmonyPrefix]
    [HarmonyPriority(int.MaxValue)]
    static bool ParsePlayerSentencePatch(ref Terminal __instance, ref TerminalNode __result)
    {
        string[] array = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Split(' ');

        // Parse command confirmation
        CommandResponse response = ParseCommandConfirmation(array);

        // Found valid command confirmation
        if (response.success)
        {
            __result = CreateTerminalNode(response.result);
            return false;
        }

        // Parse command
        response = ParseCommand(array);

        // Found valid command
        if (response.success)
        {
            __result = CreateTerminalNode(response.result);
            return false;
        }

        // Nothing to do here. Continue.
        return true;
    }

    [HarmonyPatch("QuitTerminal")]
    [HarmonyPostfix]
    static void QuitTerminalPatch()
    {
        SellMyScrapBase.Instance.OnTerminalQuit();
    }

    #region Main Parse
    private static CommandResponse ParseCommand(string[] array)
    {
        string first = array[0].ToLower();
        string second = array.Length > 1 ? array[1].ToLower() : string.Empty;
        string third = array.Length > 2 ? array[2].ToLower() : string.Empty;

        string[] args = [first, second, third];

        if (IsHelpCommand(args)) return new CommandResponse(true, ParseHelpCommand(array));
        if (IsSellQuotaCommand(args)) return new CommandResponse(true, ParseSellQuotaCommand(array));
        if (IsSellAllCommand(args)) return new CommandResponse(true, ParseSellAllCommand(array));
        if (IsSellAmountCommand(args)) return new CommandResponse(true, ParseSellAmountCommand(array));
        if (IsViewScrapCommand(args)) return new CommandResponse(true, ParseViewScrapCommand(array));
        if (IsViewConfigCommand(args)) return new CommandResponse(true, ParseViewConfigCommand(array));

        if (array[0] == "cosmic") return new CommandResponse(true, "Cosmic is gay.\n\n");

        return new CommandResponse(false, string.Empty);
    }

    private static CommandResponse ParseCommandConfirmation(string[] array)
    {
        CommandResponse response = ParseSellCommandConfirmation(array);
        if (response.success) return response;

        return new CommandResponse(false, string.Empty);
    }
    #endregion

    #region Is Command
    private static bool IsHelpCommand(string[] array)
    {
        if (array[0] == "sell" && array[1] == string.Empty) return true;

        return false;
    }

    private static bool IsSellQuotaCommand(string[] array)
    {
        if (array[0] == "sell" && array[1] == "quota") return true;
        if (array[0] == "sell-quota") return true;

        return false;
    }

    private static bool IsSellAllCommand(string[] array)
    {
        if (array[0] == "sell" && array[1] == "all") return true;
        if (array[0] == "sell-all") return true;

        return false;
    }

    private static bool IsSellAmountCommand(string[] array)
    {
        if (array[0] == "sell" && array[1] != string.Empty) return true;

        return false;
    }

    private static bool IsViewScrapCommand(string[] array)
    {
        if (array[0] == "view" && array[1] == "scrap") return true;
        if (array[0] == "view-scrap") return true;

        return false;
    }
    
    private static bool IsViewConfigCommand(string[] array)
    {
        if (array[0] == "view" && array[1] == "config") return true;
        if (array[0] == "view-config") return true;

        return false;
    }
    #endregion

    #region Parse Commands
    private static string ParseHelpCommand(string[] array)
    {
        string message = string.Empty;
        message += $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION}\n\n";
        message += "The following commands are available:\n\n";
        message += "sell <amount>\n";
        message += "sell quota        sell-quota\n";
        message += "sell all          sell-all\n";
        message += "view scrap        view-scrap\n";
        message += "view config       view-config\n\n";

        return message;
    }

    #region Parse Sell Commands
    private static CommandResponse CanUseSellCommands()
    {
        StartOfRound startOfRound = StartOfRound.Instance;

        bool isAtCompany = startOfRound.currentLevelID == 3;
        bool isLanded = !startOfRound.inShipPhase && !startOfRound.travellingToNewLevel;

        if (!isAtCompany || !isLanded)
        {
            return new CommandResponse(false, $"You must be landed at The Company building to sell your scrap!\n\n");
        }

        return new CommandResponse(true, string.Empty);
    }

    private static string ParseSellAmountCommand(string[] array)
    {
        CommandResponse response = CanUseSellCommands();
        if (!response.success) return response.result;

        // Amount not specified
        if (array.Length < 2)
        {
            return "Please specify an amount to sell.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer\nExample: sell 500\n\n";
        }

        int amount;
        int.TryParse(array[1], out amount);

        // Invalid sell amount
        if (amount <= 0)
        {
            return "ERROR! Sell amount is invalid.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer\nExample: sell 500\n\n";
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllowedScrapToSell(amount);
        int rate = (int)(100f * StartOfRound.Instance.companyBuyingRate);

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nRequested amount: ${amount}\nThe Company is buying at %{rate}\n\n";

        // Display found scrap items
        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
            {
                message += $"{scrapToSell.GetListAsString()}\n\n";
            }
            else
            {
                message += $"[Too many items to show]\n\n";
            }
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAmount, scrapToSell.value, amount, ConfirmationType.AwaitingConfirmation);

        // Return confirmation message
        return message;
    }

    private static string ParseSellQuotaCommand(string[] array)
    {
        CommandResponse response = CanUseSellCommands();
        if (!response.success) return response.result;

        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int amount = profitQuota - quotaFulfilled;

        // Quota has already been fulfilled
        if (amount <= 0)
        {
            return "Quota has already been fulfilled.\n\n";
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllowedScrapToSell(amount);
        int rate = (int)(100f * StartOfRound.Instance.companyBuyingRate);

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nProfit quota: ${quotaFulfilled} / ${profitQuota} (${amount})\nThe Company is buying at %{rate}\n\n";

        // Display found scrap items
        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
            {
                message += $"{scrapToSell.GetListAsString()}\n\n";
            }
            else
            {
                message += $"[Too many items to show]\n\n";
            }
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellQuota, scrapToSell.value, amount, ConfirmationType.AwaitingConfirmation);

        // Return confirmation message
        return message;
    }

    private static string ParseSellAllCommand(string[] array)
    {
        CommandResponse response = CanUseSellCommands();
        if (!response.success) return response.result;

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllAllowedScrapToSell();
        int rate = (int)(100f * StartOfRound.Instance.companyBuyingRate);

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nThe Company is buying at %{rate}\n\n";

        // Display found scrap items
        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
            {
                message += $"{scrapToSell.GetListAsString()}\n\n";
            }
            else
            {
                message += $"[Too many items to show]\n\n";
            }
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAll, scrapToSell.value, scrapToSell.value, ConfirmationType.AwaitingConfirmation);

        // Return confirmation message
        return message;
    }

    private static CommandResponse ParseSellCommandConfirmation(string[] array)
    {
        SellRequest sellRequest = SellMyScrapBase.Instance.sellRequest;

        // No sellReqest found
        if (sellRequest == null) return new CommandResponse(false, string.Empty);

        // Not awaiting a sell confirmation
        if (sellRequest.confirmationType != ConfirmationType.AwaitingConfirmation) return new CommandResponse(false, string.Empty);

        string command = array[0].ToLower();

        if ("confirm".Contains(command))
        {
            SellMyScrapBase.Instance.ConfirmSellRequest();
            return new CommandResponse(true, $"Sell confirmed. Processing ${sellRequest.amountFound}...\n\n");
        }

        if ("deny".Contains(command))
        {
            SellMyScrapBase.Instance.CancelSellRequest();
            return new CommandResponse(true, "Sell aborted.\n\n");
        }

        SellMyScrapBase.Instance.CancelSellRequest();
        return new CommandResponse(true, "Invalid input. Sell aborted.\n\n");
    }
    #endregion

    private static string ParseViewScrapCommand(string[] array)
    {
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllScrapToSell();

        // No items found
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\n\n";

        message += $"{scrapToSell.GetListAsString()}\n\n";

        return message;
    }

    private static string ParseViewConfigCommand(string[] array)
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        string syncedMessage = NetworkManager.Singleton.IsHost ? string.Empty : " (Synced with host)";

        string message = string.Empty;
        message += $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION} config\n\n";
        message += $"[Sell Settings]{syncedMessage}\n";
        message += $"sellGifts:    {configManager.SellGifts}\n";
        message += $"sellShotguns: {configManager.SellShotguns}\n";
        message += $"sellAmmo:     {configManager.SellAmmo}\n";
        message += $"sellPickles:  {configManager.SellPickles}\n\n";
        message += $"[Advanced Sell Settings]{syncedMessage}\n";
        message += $"dontSellListJson: {JsonConvert.SerializeObject(configManager.DontSellListJson)}\n\n";
        message += "[Terminal Settings]\n";
        message += $"overrideWelcomeMessage: {configManager.OverrideWelcomeMessage}\n";
        message += $"overrideHelpMessage:    {configManager.OverrideHelpMessage}\n";
        message += $"showFoundItems:         {configManager.ShowFoundItems}\n";
        message += $"showFoundItemsLimit:    {configManager.ShowFoundItemsLimit}\n";
        message += $"sortFoundItems:         {configManager.SortFoundItems}\n";
        message += $"alignFoundItemsPrice:   {configManager.AlignFoundItemsPrice}\n\n";
        message += "[Misc Settings]\n";
        message += $"speakInShip: {configManager.SpeakInShip}\n\n";

        return message;
    }
    #endregion

    #region Create TerminalNode
    private static TerminalNode CreateTerminalNode(string message)
    {
        return CreateTerminalNode(message, true);
    }

    private static TerminalNode CreateTerminalNode(string message, bool clearPreviousText)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = message;
        terminalNode.clearPreviousText = clearPreviousText;
        terminalNode.maxCharactersToType = 50;

        return terminalNode;
    }
    #endregion
}

public class CommandResponse
{
    public bool success;
    public string result;

    public CommandResponse(bool success, string result)
    {
        this.success = success;
        this.result = result;
    }
}
