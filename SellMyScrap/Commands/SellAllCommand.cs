using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAllCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == "all") return true;
        if (args[0] == "sell-all") return true;
        if (args[0] == "sell" && args[1] == "everything") return true;
        if (args[0] == "sell-everything") return true;

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

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(int.MaxValue);

        if (scrapToSell.Amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellAll, scrapToSell.Value, scrapToSell.Value, ConfirmationType.AwaitingConfirmation, scrapEaterIndex);
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.Amount} items with a total value of ${scrapToSell.RealValue}\n";
        message += GetQuotaFulfilledString();
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.RealValue);
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.Scrap)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
