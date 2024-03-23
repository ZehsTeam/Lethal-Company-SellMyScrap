using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellItemCommand : SellCommand
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == "item") return true;
        if (args[0] == "sell-item") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        string[] _args = Utils.GetArrayToLower(args);

        if (!CanUseCommand(out TerminalNode terminalNode))
        {
            return terminalNode;
        }

        if (_args[1] == "item" && _args[2] == string.Empty || _args[1] == string.Empty)
        {
            return TerminalPatch.CreateTerminalNode(GetSellItemInvalidMessage());
        }

        int length = args[0].Length;
        if (_args[1] == "item") length += args[1].Length + 1;
        string itemName = string.Join(' ', _args).Substring(length).Trim();

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.SetScrapToSell(ScrapHelper.GetScrapByItemName(itemName, false));

        if (scrapToSell.amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found to sell.\n\n");
        }

        SellMyScrapBase.Instance.CreateSellRequest(SellType.SellItem, scrapToSell.value, scrapToSell.value, ConfirmationType.AwaitingConfirmation);
        awaitingConfirmation = true;

        string message = GetMessage(scrapToSell);
        return TerminalPatch.CreateTerminalNode(message);
    }

    private static string GetMessage(ScrapToSell scrapToSell)
    {
        string message = $"Found {scrapToSell.amount} items with a total value of {GetValueString(scrapToSell)}\n";
        message += $"The Company is buying at %{CompanyBuyingRate}\n";
        message += GetOvertimeBonusString(scrapToSell.realValue);

        if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
        {
            message += $"{ScrapHelper.GetScrapMessage(scrapToSell.scrap)}\n\n";
        }

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
