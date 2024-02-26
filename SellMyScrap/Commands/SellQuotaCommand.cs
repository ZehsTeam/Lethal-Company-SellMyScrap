using com.github.zehsteam.SellMyScrap.Patches;

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

        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int requestedValue = profitQuota - quotaFulfilled;

        if (requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode("Quota has already been fulfilled.\n\n");
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(requestedValue);

        if (scrapToSell.amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        string message = $"Found {scrapToSell.amount} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"Profit quota: ${quotaFulfilled} / ${profitQuota} (${requestedValue})\n";
        message += $"The Company is buying at %{companyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.realValue);

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.scrap)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellQuota, scrapToSell.value, requestedValue, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(message);
    }
}
