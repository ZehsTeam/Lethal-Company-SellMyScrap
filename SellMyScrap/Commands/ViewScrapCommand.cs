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
        ScrapToSell scrapToSell = new ScrapToSell(ScrapHelper.GetScrapFromShip(false));

        // No items found
        if (scrapToSell.Amount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found.\n\n");
        }

        string message = $"Found {scrapToSell.Amount} items with a total value of ${scrapToSell.Value}\n\n";
        message += $"{ScrapHelper.GetScrapMessage(scrapToSell.Scrap)}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
