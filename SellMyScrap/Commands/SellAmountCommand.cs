using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellAmountCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        if (args[0] == "sell" && args[1] != string.Empty) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        if (!int.TryParse(args[1], out int requestedValue) || requestedValue <= 0)
        {
            return TerminalPatch.CreateTerminalNode("Error: sell amount is invalid.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer.\nExample: sell 500\n\n");
        }

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllowedScrapToSell(requestedValue);

        if (scrapToSell.count == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        string message = $"Found {scrapToSell.count} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"Requested value: ${requestedValue}\n";
        message += $"The Company is buying at %{companyBuyingRate}\n\n";

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{scrapToSell.GetListAsString()}\n\n";
        }

        message += "Please CONFIRM or DENY.\n\n";

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellAmount, scrapToSell.value, requestedValue, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(message);
    }
}
