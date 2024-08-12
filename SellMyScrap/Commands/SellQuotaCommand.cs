using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellQuotaCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == "quota") return true;
        if (args[0] == "sell-quota") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        string extra = string.Join(' ', args).Trim();
        List<CommandFlag> foundFlags = GetFlagsFromString(extra);
        int scrapEaterIndex = GetScrapEaterIndex(foundFlags);

        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int requestedValue = profitQuota - quotaFulfilled;

        if (requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode("Quota has already been fulfilled.\n\n");
        }

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(requestedValue);

        if (scrapToSell.Amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellQuota, scrapToSell.Value, requestedValue, ConfirmationType.AwaitingConfirmation, scrapEaterIndex);
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell, int requestedValue)
    {
        string foundColor = scrapToSell.RealValue >= requestedValue ? "green" : "red";
        string message = $"Found {scrapToSell.Amount} items with a total value of <color={foundColor}>${scrapToSell.RealValue}</color>\n";
        message += GetQuotaFulfilledString(scrapToSell.RealValue);
        message += GetOvertimeBonusString(scrapToSell.RealValue);
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.Scrap, TerminalPatch.GreenColor2)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
