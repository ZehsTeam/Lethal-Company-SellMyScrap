using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Patches;

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
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        string itemName = string.Join(" ", args).Trim();

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return TerminalPatch.CreateTerminalNode(GetSellItemInvalidMessage());
        }

        ScrapToSell scrapToSell = Plugin.Instance.SetScrapToSell(ScrapHelper.GetAllScrapByItemName(itemName, onlyUseShipInventory: OnlyUseShipInventory()));

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellItem, scrapToSell.TotalScrapValue, scrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private static string GetMessage(ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.RealTotalScrapValue}\n";
        message += GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue);
        message += GetOvertimeBonusString(scrapToSell.RealTotalScrapValue);
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += "\n";
        message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n\n";
        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }

    private string GetSellItemInvalidMessage()
    {
        string message = "Error: sell item name is invalid.\n\n";
        message += "Usage:\n";
        message += "    sell item <name>\n\n";
        message += "Where <name> is an item name.\n\n";
        message += "Usage examples:\n";
        message += "    sell item Whoopie cushion\n";
        message += "    sell item Whoopie\n";
        message += "    sell item Whoo\n\n";

        return message;
    }
}
