using com.github.zehsteam.SellMyScrap.Patches;
using System.Data;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAmountCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] != string.Empty) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        string expression = string.Join(' ', args).Substring(args[0].Length).Trim();
        string evaluatedExpression = expression;

        try
        {
            DataTable dataTable = new DataTable();
            evaluatedExpression = dataTable.Compute(expression, "").ToString();
        }
        catch
        {
            SellMyScrapBase.mls.LogError($"Error: failed to evalute expression for sell <amount>");
        }

        if (!int.TryParse(evaluatedExpression, out int requestedValue) || requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode("Error: sell amount is invalid.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer.\nExample: sell 500\n\n");
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(requestedValue);

        if (scrapToSell.amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAmount, scrapToSell.value, requestedValue, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        string message = GetMessage(requestedValue, scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(int requestedValue, ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.amount} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"Requested value: ${requestedValue}\n";
        message += $"The Company is buying at %{companyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.realValue);

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.scrap)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
