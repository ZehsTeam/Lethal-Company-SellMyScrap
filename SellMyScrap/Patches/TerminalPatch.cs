using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    static bool hasOverrideTerminalNodes = false;

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    static void StartPatch(ref TerminalNodesList ___terminalNodes)
    {
        if (!hasOverrideTerminalNodes)
        {
            hasOverrideTerminalNodes = true;

            if (SellMyScrapBase.Instance.ConfigManager.OverrideWelcomeMessage) OverrideWelcomeTerminalNode(___terminalNodes);
            if (SellMyScrapBase.Instance.ConfigManager.OverrideHelpMessage) OverrideHelpTerminalNode(___terminalNodes);
        }
    }

    #region TerminalNode Overrides
    private static void OverrideWelcomeTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 1;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;
        
        string message = defaultMessage.Trim();
        message += $"\n\n\nSellMyScrap v{MyPluginInfo.PLUGIN_VERSION}\n\nType \"Sell\" for a list of commands.\n\n\n\n";

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
        string command = array[0].ToLower();
        string second = array.Length > 1 ? array[1].ToLower() : string.Empty;

        if (command == "sell" && (second == "help" || second == string.Empty)) return new CommandResponse(true, ParseHelpCommand(array));
        if (command == "sell-quota" || (command == "sell" && second == "quota")) return new CommandResponse(true, ParseSellQuota(array));
        if (command == "sell-all" || (command == "sell" && second == "all")) return new CommandResponse(true, ParseSellAll(array));
        if (command == "sell") return new CommandResponse(true, ParseSellAmount(array));
        if (command == "view-scrap" || (command == "view" && second == "scrap")) return new CommandResponse(true, ParseViewScrap(array));
        if (command == "cosmic") return new CommandResponse(true, "Cosmic is gay.\n\n");

        return new CommandResponse(false, string.Empty);
    }

    private static CommandResponse ParseCommandConfirmation(string[] array)
    {
        CommandResponse response = ParseSellCommandConfirmation(array);
        if (response.success) return response;

        return new CommandResponse(false, string.Empty);
    }
    #endregion

    private static string ParseHelpCommand(string[] array)
    {
        string message = string.Empty;
        message += $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION}\n\n";
        message += "The following commands are available:\n\n";
        message += "sell <amount>\n";
        message += "sell-quota        sell quota\n";
        message += "sell-all          sell all\n";
        message += "view-scrap        view scrap\n\n";

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

    private static string ParseSellAmount(string[] array)
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

    private static string ParseSellQuota(string[] array)
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

    private static string ParseSellAll(string[] array)
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

    private static string ParseViewScrap(string[] array)
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
