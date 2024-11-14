using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Data;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAmountCommand : SellCommand
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell") && args[0] != string.Empty) return true;
        if (MatchesPattern(ref args, "sell", "amount")) return true;
        if (MatchesPattern(ref args, "sell-amount")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        string expression = string.Join(" ", args).Trim();
        string evaluatedExpression = string.Empty;

        try
        {
            DataTable dataTable = new DataTable();
            evaluatedExpression = dataTable.Compute(expression, "").ToString();
        }
        catch
        {
            Plugin.Logger.LogError($"Error: failed to evalute expression for sell <amount>");
        }

        if (!int.TryParse(evaluatedExpression, out int requestedValue) || requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode(GetSellAmountInvalidMessage());
        }

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(requestedValue, withOvertimeBonus: WithOvertimeBonus(), onlyUseShipInventory: OnlyUseShipInventory());

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellAmount, scrapToSell.TotalScrapValue, requestedValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue, WithOvertimeBonus());
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell, int requestedValue, bool withOvertimeBonus)
    {
        string overtimeBonusString;
        string foundColor;

        if (withOvertimeBonus)
        {
            overtimeBonusString = GetOvertimeBonusWithValueString(scrapToSell.RealTotalScrapValue, requestedValue, out bool hasEnoughWithOvertimeBonus);
            foundColor = scrapToSell.RealTotalScrapValue >= requestedValue || hasEnoughWithOvertimeBonus ? "green" : "red"; 
        }
        else
        {
            overtimeBonusString = GetOvertimeBonusString(scrapToSell.RealTotalScrapValue);
            foundColor = scrapToSell.RealTotalScrapValue >= requestedValue ? "green" : "red";
        }

        string message = $"Found {scrapToSell.ItemCount} items with a total value of <color={foundColor}>${scrapToSell.RealTotalScrapValue}</color>\n";
        message += $"Requested value: ${requestedValue}\n";
        message += GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue);
        message += overtimeBonusString;
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }

    private string GetSellAmountInvalidMessage()
    {
        string message = "Error: sell amount is invalid.\n\n";
        message += "Usage:\n";
        message += "    sell <amount>\n";
        message += "    sell <amount> -o\n\n";
        message += "Where <amount> is a positive integer or math expression.\n\n";
        message += "Flags:\n";
        message += "    -o    Will sell for a less amount so (less amount + overtime bonus) = initial amount.\n\n";
        message += "Usage examples:\n";
        message += "    sell 500\n";
        message += "    sell 110 * 5 - 50\n";
        message += "    sell 550 -o\n\n";

        return message;
    }
}
