using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellListCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == "list") return true;
        if (args[0] == "sell-list") return true;

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

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(Plugin.ConfigManager.SellList);

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellList, scrapToSell.TotalScrapValue, scrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, scrapEaterIndex);
        AwaitingConfirmation = true;
        
        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.RealTotalScrapValue}\n";
        message += GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue);
        message += GetOvertimeBonusString(scrapToSell.RealTotalScrapValue);
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList, TerminalPatch.GreenColor2)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
