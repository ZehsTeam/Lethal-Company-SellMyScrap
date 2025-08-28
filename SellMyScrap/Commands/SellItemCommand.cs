using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Text;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellItemCommand : SellCommand
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell", "item")) return true;
        if (MatchesPattern(ref args, "sell-item")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode failReason))
        {
            return failReason;
        }

        string itemName = string.Join(" ", args).Trim();

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return TerminalHelper.CreateTerminalNode(GetSellItemInvalidMessage());
        }

        ScrapToSell scrapToSell = SellManager.SetScrapToSell(ScrapHelper.GetAllScrapByItemName(itemName, onlyUseShipInventory: OnlyUseShipInventory()));

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalHelper.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellManager.CreateSellRequest(SellType.Item, scrapToSell.TotalScrapValue, scrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalHelper.CreateTerminalNode(message);
    }

    private static string GetMessage(ScrapToSell scrapToSell)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.RealTotalScrapValue}");
        builder.AppendLine(GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue));
        builder.Append(GetOvertimeBonusString(scrapToSell.RealTotalScrapValue));
        builder.AppendLine($"The Company is buying at %{CompanyBuyingRate}\n");
        builder.AppendLine($"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n");
        builder.AppendLine("Please CONFIRM or DENY.\n\n");

        return builder.ToString();
    }

    private string GetSellItemInvalidMessage()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Error: sell item name is invalid.\n");
        builder.AppendLine("Usage:");
        builder.AppendLine("sell item <name>\n");
        builder.AppendLine("Where <name> is an item name.\n");
        builder.AppendLine("Usage examples:");
        builder.AppendLine("    sell item Whoopie cushion");
        builder.AppendLine("    sell item Whoopie");
        builder.AppendLine("    sell item Whoo\n\n");

        return builder.ToString();
    }
}
