using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Objects;

namespace com.github.zehsteam.SellMyScrap.Objects;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public enum ItemLocation
{
    Ship,
    Vehicle,
    ShipInventory
}

public class ItemData
{
    public GrabbableObject GrabbableObject { get; set; }
    public SI_ItemDataProxy SI_ItemDataProxy { get; set; }

    public ItemLocation ItemLocation { get; set; }
    public string ItemName => GetItemName();
    public int ScrapValue => GetScrapValue();

    public ItemData(GrabbableObject grabbableObject, ItemLocation itemLocation)
    {
        GrabbableObject = grabbableObject;
        ItemLocation = itemLocation;
    }

    public ItemData(SI_ItemDataProxy si_ItemDataProxy, ItemLocation itemLocation)
    {
        SI_ItemDataProxy = si_ItemDataProxy;
        ItemLocation = itemLocation;
    }

    private string GetItemName()
    {
        if (GrabbableObject != null)
        {
            return GrabbableObject.itemProperties.itemName;
        }

        if (ShipInventoryProxy.Enabled && SI_ItemDataProxy.IsValid())
        {
            return SI_ItemDataProxy.ItemName;
        }

        return "Unknown Item";
    }

    private int GetScrapValue()
    {
        if (GrabbableObject != null)
        {
            return GrabbableObject.scrapValue;
        }

        if (ShipInventoryProxy.Enabled && SI_ItemDataProxy.IsValid())
        {
            return SI_ItemDataProxy.ScrapValue;
        }

        return 0;
    }
}
