﻿using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellQuotaCommand : SellCommand
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell", "quota")) return true;
        if (MatchesPattern(ref args, "sell-quota")) return true;

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

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(requestedValue, onlyUseShipInventory: OnlyUseShipInventory());

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellQuota, scrapToSell.TotalScrapValue, requestedValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell, int requestedValue)
    {
        string foundColor = scrapToSell.RealTotalScrapValue >= requestedValue ? "green" : "red";
        string message = $"Found {scrapToSell.ItemCount} items with a total value of <color={foundColor}>${scrapToSell.RealTotalScrapValue}</color>\n";
        message += GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue);
        message += GetOvertimeBonusString(scrapToSell.RealTotalScrapValue);
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";

        if (Plugin.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
