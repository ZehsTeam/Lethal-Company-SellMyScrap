using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Managers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Text;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellListCommand : SellCommand
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell", "list")) return true;
        if (MatchesPattern(ref args, "sell-list")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode failReason))
        {
            return failReason;
        }

        ScrapToSell scrapToSell = SellManager.GetScrapToSell(ConfigManager.SellListArray, onlyUseShipInventory: OnlyUseShipInventory());

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalHelper.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellManager.CreateSellRequest(SellType.List, scrapToSell.TotalScrapValue, scrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;
        
        string message = GetMessage(scrapToSell);
        return TerminalHelper.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.RealTotalScrapValue}");
        builder.AppendLine(GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue));
        builder.Append(GetOvertimeBonusString(scrapToSell.RealTotalScrapValue));
        builder.AppendLine($"The Company is buying at %{CompanyBuyingRate}\n");

        if (ConfigManager.ShowFoundItems.Value)
        {
            builder.AppendLine($"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n");
        }

        builder.AppendLine("Please CONFIRM or DENY.\n\n");

        return builder.ToString();
    }
}
