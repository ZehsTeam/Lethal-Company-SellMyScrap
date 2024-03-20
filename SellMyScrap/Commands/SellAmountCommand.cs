using com.github.zehsteam.SellMyScrap.Patches;
using System.Data;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAmountCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] != string.Empty) return true;
        if (args[0] == "sell-amount" && args[1] != string.Empty) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        string extra = string.Join(' ', args).Substring(args[0].Length).Trim();
        string expression = GetExpression(extra);
        string[] flags = GetFlags(extra);

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
            return TerminalPatch.CreateTerminalNode(GetSellAmountInvalidMessage());
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(requestedValue, withOvertimeBonus: flags.Contains("-o"));

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
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.realValue);

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.scrap)}\n\n";
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
        message += "Where: <amount> is a positive integer or math expression.\n\n";
        message += "Flags:\n";
        message += "    -o    Will sell for an amount where (value + overtimeBonus) = requestedValue.\n\n";
        message += "Usage examples:\n";
        message += "    sell 500\n";
        message += "    sell 110 * 5 - 50\n";
        message += "    sell 550 -o\n\n";

        return message;
    }

    private string GetExpression(string extra)
    {
        string expression = extra;
        int flagsStartIndex = GetFlagsStartIndex(extra);

        if (flagsStartIndex != -1)
        {
            expression = expression.Substring(0, flagsStartIndex);
        }

        return expression.Trim();
    }

    private string[] GetFlags(string extra)
    {
        int startIndex = GetFlagsStartIndex(extra);
        if (startIndex == -1) return [];

        return extra.Substring(startIndex).ToLower().Split(' ');
    }

    private static int GetFlagsStartIndex(string extra)
    {
        string[] flags = ["-o"];
        int startIndex = -1;

        foreach (string flag in flags)
        {
            if (extra.Contains($"{flag}", System.StringComparison.OrdinalIgnoreCase))
            {
                int index = extra.IndexOf($"{flag}", System.StringComparison.OrdinalIgnoreCase);
                if (index > startIndex) startIndex = index;
            }
        }

        return startIndex;
    }
}
