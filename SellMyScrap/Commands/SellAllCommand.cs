using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAllCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == "all") return true;
        if (args[0] == "sell-all") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(int.MaxValue);

        if (scrapToSell.amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAll, scrapToSell.value, scrapToSell.value, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private string GetMessage(ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.amount} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"The Company is buying at %{companyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.realValue);

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.scrap)}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        return message;
    }
}
