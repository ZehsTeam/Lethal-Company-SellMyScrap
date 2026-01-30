using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Managers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Data;
using System.Text;

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
        if (!CanUseCommand(out TerminalNode failReason))
        {
            return failReason;
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
            Logger.LogError($"Error: failed to evalute expression for sell <amount>");
        }

        if (!int.TryParse(evaluatedExpression, out int requestedValue) || requestedValue <= 0)
        {
            return TerminalHelper.CreateTerminalNode(GetSellAmountInvalidMessage());
        }

        ScrapToSell scrapToSell = SellManager.GetScrapToSell(new SellCommandRequest(requestedValue)
        {
            WithOvertimeBonus = WithOvertimeBonus(),
            OnlyUseShipInventory = OnlyUseShipInventory(),
            ScrapMatchAlgorithm = GetScrapMatchAlgorithm()
        });

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalHelper.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellManager.CreateSellRequest(SellType.Amount, scrapToSell.TotalScrapValue, requestedValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue, WithOvertimeBonus());
        return TerminalHelper.CreateTerminalNode(message);
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

        var builder = new StringBuilder();

        builder.AppendLine($"Found {scrapToSell.ItemCount} items with a total value of <color={foundColor}>${scrapToSell.RealTotalScrapValue}</color>");
        builder.AppendLine($"Requested value: ${requestedValue}");
        builder.AppendLine(GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue));
        builder.Append(overtimeBonusString);
        builder.AppendLine($"The Company is buying at %{CompanyBuyingRate}\n");

        if (ConfigManager.ShowFoundItems.Value)
        {
            builder.AppendLine($"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n");
        }

        builder.AppendLine("Please CONFIRM or DENY.\n\n");

        return builder.ToString();
    }

    private string GetSellAmountInvalidMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Error: sell amount is invalid.\n");
        builder.AppendLine("Usage:");
        builder.AppendLine("sell <amount>");
        builder.AppendLine("sell <amount> -o\n");
        builder.AppendLine("Where <amount> is a positive integer or math expression.\n");
        builder.AppendLine("Flags:");
        builder.AppendLine("    -o    Will sell for a less amount so (less amount + overtime bonus) = initial amount.\n");
        builder.AppendLine("Usage examples:");
        builder.AppendLine("    sell 500");
        builder.AppendLine("    sell 110 * 5 - 50");
        builder.AppendLine("    sell 550 -o\n\n");
        return builder.ToString();
    }
}
