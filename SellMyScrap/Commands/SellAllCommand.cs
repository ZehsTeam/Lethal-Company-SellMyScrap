using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAllCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
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

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllAllowedScrapToSell();

        if (scrapToSell.count == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        string message = $"Found {scrapToSell.count} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"The Company is buying at %{companyBuyingRate}\n\n";

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{scrapToSell.GetListAsString()}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAll, scrapToSell.value, scrapToSell.value, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(message);
    }
}
