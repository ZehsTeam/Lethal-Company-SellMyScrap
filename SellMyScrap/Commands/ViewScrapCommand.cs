using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Objects;
using System.Text;

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
        ScrapToSell scrapToSell = new ScrapToSell(ScrapHelper.GetAllScrap(onlyAllowedScrap: false, onlyUseShipInventory: OnlyUseShipInventory(), includeScrapWorthZero: true));

        if (scrapToSell.ItemCount == 0)
        {
            return TerminalHelper.CreateTerminalNode("No items found.\n\n");
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Found {scrapToSell.ItemCount} items with a total value of ${scrapToSell.TotalScrapValue}\n");
        builder.AppendLine($"{ScrapHelper.GetScrapMessage(scrapToSell.ItemDataList)}\n\n");

        return TerminalHelper.CreateTerminalNode(builder.ToString());
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
