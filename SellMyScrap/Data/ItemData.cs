using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

namespace com.github.zehsteam.SellMyScrap.Data;

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
    public ShipInventoryItemData ShipInventoryItemData { get; set; }

    public ItemLocation ItemLocation { get; set; }
    public string ItemName => GetItemName();
    public int ScrapValue => GetScrapValue();

    public ItemData(GrabbableObject grabbableObject, ItemLocation itemLocation)
    {
        GrabbableObject = grabbableObject;
        ItemLocation = itemLocation;
    }

    public ItemData(ShipInventoryItemData shipInventoryItemData, ItemLocation itemLocation)
    {
        ShipInventoryItemData = shipInventoryItemData;
        ItemLocation = itemLocation;
    }

    private string GetItemName()
    {
        if (GrabbableObject != null)
        {
            return GrabbableObject.itemProperties.itemName;
        }

        if (ShipInventoryProxy.Enabled && ShipInventoryItemData != null)
        {
            return ShipInventoryItemData.GetItemName();
        }

        return string.Empty;
    }

    private int GetScrapValue()
    {
        if (GrabbableObject != null)
        {
            return GrabbableObject.scrapValue;
        }

        if (ShipInventoryProxy.Enabled && ShipInventoryItemData != null)
        {
            return ShipInventoryItemData.ScrapValue;
        }

        return 0;
    }
}
