using com.github.zehsteam.SellMyScrap.Data;
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
        ScrapToSell scrapToSell = new ScrapToSell(ScrapHelper.GetAllScrap(onlyAllowedScrap: false));

        // No items found
        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found.\n\n");
        }

        string message = $"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.TotalScrapValue}\n\n";
        message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList, TerminalPatch.GreenColor2)}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
