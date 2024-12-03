using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Text;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAllCommand : SellCommand
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell", "all")) return true;
        if (MatchesPattern(ref args, "sell-all")) return true;
        if (MatchesPattern(ref args, "sell", "everything")) return true;
        if (MatchesPattern(ref args, "sell-everything", "everything")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        ScrapToSell scrapToSell = Plugin.Instance.GetScrapToSell(int.MaxValue, onlyUseShipInventory: OnlyUseShipInventory());

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        Plugin.Instance.CreateSellRequest(SellType.SellAll, scrapToSell.TotalScrapValue, scrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.RealTotalScrapValue}");
        builder.AppendLine(GetQuotaFulfilledString(scrapToSell.RealTotalScrapValue));
        builder.Append(GetOvertimeBonusString(scrapToSell.RealTotalScrapValue));
        builder.AppendLine($"The Company is buying at %{CompanyBuyingRate}\n");

        if (Plugin.ConfigManager.ShowFoundItems.Value)
        {
            builder.AppendLine($"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n");
        }

        builder.AppendLine("Please CONFIRM or DENY.\n\n");

        return builder.ToString();
    }
}
