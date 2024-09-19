using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewScrapCommand : Command
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "view", "scrap")) return true;
        if (MatchesPattern(ref args, "view-scrap")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        ScrapToSell scrapToSell = new ScrapToSell(ScrapHelper.GetAllScrap(onlyAllowedScrap: false, onlyUseShipInventory: OnlyUseShipInventory()));

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalPatch.CreateTerminalNode("No items found.\n\n");
        }

        string message = $"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.TotalScrapValue}\n\n";
        message += $"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }

    private bool OnlyUseShipInventory()
    {
        if (!ShipInventoryProxy.Enabled) return false;

        if (HasFlag("inv")) return true;
        if (HasFlag("inventory")) return true;
        if (HasFlag("shipinv")) return true;
        if (HasFlag("shipinventory")) return true;

        return false;
    }
}
