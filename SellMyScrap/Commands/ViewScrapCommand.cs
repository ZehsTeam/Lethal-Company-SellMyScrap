using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewScrapCommand : Command
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "view" && args[1] == "scrap") return true;
        if (args[0] == "view-scrap") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllScrapToSell();

        // No items found
        if (scrapToSell.amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found.\n\n");
        }

        string message = $"Found {scrapToSell.amount} items with a total value of ${scrapToSell.value}\n\n";
        message += $"{scrapToSell.GetListAsString()}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
