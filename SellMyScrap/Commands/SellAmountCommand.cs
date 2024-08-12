using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAmountCommand : SellCommand
{
    public SellAmountCommand()
    {
        Flags.Add(new CommandFlag("-o"));
    }

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

        List<CommandFlag> foundFlags = GetFlagsFromString(extra.Substring(expression.Length));
        bool withOvertimeBonus = GetWithOvertimeBonus(foundFlags);
        int scrapEaterIndex = GetScrapEaterIndex(foundFlags);

        string evaluatedExpression = expression;

        try
        {
            DataTable dataTable = new DataTable();
            evaluatedExpression = dataTable.Compute(expression, "").ToString();
        }
        catch
        {
            Plugin.logger.LogError($"Error: failed to evalute expression for sell <amount>");
        }

        if (!int.TryParse(evaluatedExpression, out int requestedValue) || requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode(GetSellAmountInvalidMessage());
        }

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(requestedValue, withOvertimeBonus: withOvertimeBonus);

        if (scrapToSell.Amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellAmount, scrapToSell.Value, requestedValue, ConfirmationType.AwaitingConfirmation, scrapEaterIndex);
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue, withOvertimeBonus);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell, int requestedValue, bool withOvertimeBonus)
    {
        string overtimeBonusString;
        string foundColor;

        if (withOvertimeBonus)
        {
            overtimeBonusString = GetOvertimeBonusWithValueString(scrapToSell.RealValue, requestedValue, out bool hasEnoughWithOvertimeBonus);
            foundColor = scrapToSell.RealValue >= requestedValue || hasEnoughWithOvertimeBonus ? "green" : "red"; 
        }
        else
        {
            overtimeBonusString = GetOvertimeBonusString(scrapToSell.RealValue);
            foundColor = scrapToSell.RealValue >= requestedValue ? "green" : "red";
        }

        string message = $"Found {scrapToSell.Amount} items with a total value of <color={foundColor}>${scrapToSell.RealValue}</color>\n";
        message += $"Requested value: ${requestedValue}\n";
        message += GetQuotaFulfilledString(scrapToSell.RealValue);
        message += overtimeBonusString;
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.Scrap, TerminalPatch.GreenColor2)}\n\n";
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

    private string GetExpression(string extra)
    {
        string expression = extra;
        int flagsStartIndex = GetFlagsStartIndexInString(extra);

        if (flagsStartIndex != -1)
        {
            expression = expression.Substring(0, flagsStartIndex);
        }

        return expression.Trim();
    }

    private bool GetWithOvertimeBonus(List<CommandFlag> foundFlags)
    {
        CommandFlag flag = foundFlags.Find(_ => _.Key.ToLower() == "-o");
        if (flag == null) return false;

        return flag.CanUse;
    }
}
