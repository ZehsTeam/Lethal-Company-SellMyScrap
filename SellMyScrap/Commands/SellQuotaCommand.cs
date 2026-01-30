using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Managers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Text;

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
        if (!CanUseCommand(out TerminalNode failReason))
        {
            return failReason;
        }

        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int requestedValue = profitQuota - quotaFulfilled;

        if (requestedValue <= 0)
        {
            return TerminalHelper.CreateTerminalNode("Quota has already been fulfilled.\n\n");
        }

        ScrapToSell scrapToSell = SellManager.GetScrapToSell(new SellCommandRequest(requestedValue)
        {
            OnlyUseShipInventory = OnlyUseShipInventory(),
            ScrapMatchAlgorithm = GetScrapMatchAlgorithm()
        });

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalHelper.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellManager.CreateSellRequest(SellType.Quota, scrapToSell.TotalScrapValue, requestedValue, ConfirmationStatus.AwaitingConfirmation, GetScrapEaterIndex(), GetScrapEaterVariantIndex());
        AwaitingConfirmation = true;

        string message = GetMessage(scrapToSell, requestedValue);
        return TerminalHelper.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell, int requestedValue)
    {
        string foundColor = scrapToSell.RealTotalScrapValue >= requestedValue ? "green" : "red";
        string message = $"Found {scrapToSell.ItemCount} items with a total value of <color={foundColor}>${scrapToSell.RealTotalScrapValue}</color>";
        
        StringBuilder builder = new StringBuilder();

        builder.AppendLine(message);
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
