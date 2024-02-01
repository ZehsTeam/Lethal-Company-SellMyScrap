using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
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

        if (command == "sell") return new CommandResponse(true, ParseSellAmount(array));
        if (command == "sell-quota") return new CommandResponse(true, ParseSellQuota(array));
        if (command == "sell-all") return new CommandResponse(true, ParseSellAll(array));
        if (command == "view-scrap") return new CommandResponse(true, ParseViewScrap(array));
        if (command == "cosmic") return new CommandResponse(true, "Cosmic is gay.\n\n");

        return new CommandResponse(false, string.Empty);
    }

    private static CommandResponse ParseCommandConfirmation(string[] array)
    {
        CommandResponse response = ParseSellCommandConfirmation(array);
        if (response.success) return response;

        return new CommandResponse(false, string.Empty);
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

    #region Parse Sell Commands
    private static CommandResponse CanUseSellCommands()
    {
        //if (!NetworkManager.Singleton.IsHost)
        //{
        //    return new CommandResponse(false, "Only the host can use sell commands!\n\n");
        //}

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
