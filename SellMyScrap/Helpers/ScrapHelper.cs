using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Dependencies.Vanilla;
using com.github.zehsteam.SellMyScrap.Extensions;
using com.github.zehsteam.SellMyScrap.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class ScrapHelper
{
    public static Transform ShipTransform => StartOfRound.Instance.elevatorTransform;

    #region Get Scrap
    private static List<GrabbableObject> GetValidScrap(IEnumerable<GrabbableObject> grabbableObjects, bool onlyAllowedScrap, bool includeScrapWorthZero = false)
    {
        if (grabbableObjects == null) return [];

        return grabbableObjects.Where(x => IsValidScrap(x, onlyAllowedScrap, includeScrapWorthZero)).ToList();
    }

    public static List<GrabbableObject> GetScrapFromShip(bool onlyAllowedScrap = true, bool includeScrapWorthZero = false)
    {
        if (ShipTransform == null) return [];

        List<GrabbableObject> grabbableObjects = ShipTransform.GetComponentsInChildren<GrabbableObject>().ToList();
        grabbableObjects.AddRange(GetGrabbableObjectsFromShipPlaceableObjects());

        return GetValidScrap(grabbableObjects, onlyAllowedScrap, includeScrapWorthZero);
    }

    private static List<GrabbableObject> GetGrabbableObjectsFromShipPlaceableObjects()
    {
        List<GrabbableObject> grabbableObjects = [];

        foreach (var autoParentToShip in Object.FindObjectsByType<AutoParentToShip>(FindObjectsSortMode.None))
        {
            if (autoParentToShip.transform.parent != null)
            {
                continue;
            }

            grabbableObjects.AddRange(autoParentToShip.GetComponentsInChildren<GrabbableObject>());
        }

        return grabbableObjects;
    }

    public static List<GrabbableObject> GetScrapFromVehicle(bool onlyAllowedScrap = true, bool includeScrapWorthZero = false)
    {
        if (VehicleControllerProxy.Enabled)
        {
            return GetValidScrap(VehicleControllerProxy.GetGrabbableObjects(), onlyAllowedScrap, includeScrapWorthZero);
        }

        return [];
    }

    public static List<ItemData> GetItemDataList(List<GrabbableObject> shipGrabbableObjects, List<GrabbableObject> vehicleGrabbableObjects, ShipInventoryItemData[] shipInventoryItems)
    {
        List<ItemData> items = [];

        foreach (var grabbableObject in shipGrabbableObjects)
        {
            items.Add(new ItemData(grabbableObject, ItemLocation.Ship));
        }

        foreach (var grabbableObject in vehicleGrabbableObjects)
        {
            items.Add(new ItemData(grabbableObject, ItemLocation.Vehicle));
        }

        foreach (var shipInventoryItemData in shipInventoryItems)
        {
            items.Add(new ItemData(shipInventoryItemData, ItemLocation.ShipInventory));
        }

        return items;
    }

    public static List<ItemData> GetAllScrap(bool onlyAllowedScrap = true, bool onlyUseShipInventory = false, bool includeScrapWorthZero = false)
    {
        ShipInventoryItemData[] shipInventoryItems = [];

        if (ShipInventoryProxy.Enabled)
        {
            shipInventoryItems = ShipInventoryProxy.GetItems().Where(x => IsValidScrap(x, onlyAllowedScrap, includeScrapWorthZero)).ToArray();

            if (onlyUseShipInventory)
            {
                return GetItemDataList([], [], shipInventoryItems);
            }
        }

        return GetItemDataList(GetScrapFromShip(onlyAllowedScrap, includeScrapWorthZero), GetScrapFromVehicle(onlyAllowedScrap, includeScrapWorthZero), shipInventoryItems);
    }

    public static List<ItemData> GetAllScrapByItemName(string itemName, bool matchCase = false, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;

        return GetAllScrap(onlyAllowedScrap, onlyUseShipInventory).Where(item =>
        {
            return item.ItemName.Contains(itemName, comparisonType);
        }).ToList();
    }

    public static List<ItemData> GetAllScrapByItemNames(string[] itemNames, bool matchCase = false, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;

        return GetAllScrap(onlyAllowedScrap, onlyUseShipInventory).Where(item =>
        {
            foreach (var itemName in itemNames)
            {
                if (item.ItemName.Contains(itemName, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }).ToList();
    }

    public static List<Item> GetAllScrapItems()
    {
        if (StartOfRound.Instance == null) return [];

        return StartOfRound.Instance.allItemsList.itemsList.Where(IsScrap).ToList();
    }

    public static bool IsScrap(GrabbableObject grabbableObject)
    {
        if (grabbableObject == null) return false;
        if (!IsScrap(grabbableObject.itemProperties)) return false;

        if (grabbableObject.isHeld || !grabbableObject.grabbable)
        {
            return false;
        }

        return true;
    }

    public static bool IsScrap(Item item)
    {
        if (item == null) return false;

        return item.isScrap;
    }

    public static bool IsAllowedScrap(GrabbableObject grabbableObject, string[] dontSellList, bool matchCase = false)
    {
        if (grabbableObject == null) return false;

        return IsAllowedScrap(grabbableObject.itemProperties, dontSellList, matchCase);
    }

    public static bool IsAllowedScrap(Item item, string[] dontSellItemNames, bool matchCase = false)
    {
        if (item == null) return false;

        return IsAllowedScrap(item.itemName, dontSellItemNames, matchCase);
    }

    public static bool IsAllowedScrap(ShipInventoryItemData shipInventoryItemData, string[] dontSellItemNames, bool matchCase = false)
    {
        if (shipInventoryItemData == null) return false;

        return IsAllowedScrap(shipInventoryItemData.GetItemName(), dontSellItemNames, matchCase);
    }

    public static bool IsAllowedScrap(string itemName, string[] dontSellItemNames, bool matchCase = false)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;

        if (itemName.Equals("Gift", comparisonType) && !ConfigManager.SellGifts.Value) return false;
        if (itemName.Equals("Shotgun", comparisonType) && !ConfigManager.SellShotguns.Value) return false;
        if (itemName.Equals("Ammo", comparisonType) && !ConfigManager.SellAmmo.Value) return false;
        if (itemName.Equals("Kitchen knife", comparisonType) && !ConfigManager.SellKnives.Value) return false;
        if (itemName.Equals("Jar of pickles", comparisonType) && !ConfigManager.SellPickles.Value) return false;

        foreach (var dontSellItemName in dontSellItemNames)
        {
            if (itemName.Contains(dontSellItemName, comparisonType))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidScrap(GrabbableObject grabbableObject, bool onlyAllowedScrap, bool includeScrapWorthZero = false)
    {
        if (grabbableObject == null) return false;

        if (!IsScrap(grabbableObject)) return false;

        if (!includeScrapWorthZero && !ConfigManager.SellScrapWorthZero.Value && grabbableObject.scrapValue <= 0)
        {
            return false;
        }

        if (ConfigManager.OnlySellScrapOnFloor.Value && !grabbableObject.IsOnShipFloor())
        {
            return false;
        }

        if (onlyAllowedScrap && !IsAllowedScrap(grabbableObject, ConfigManager.DontSellListArray))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidScrap(ShipInventoryItemData shipInventoryItemData, bool onlyAllowedScrap, bool includeScrapWorthZero = false)
    {
        if (shipInventoryItemData == null) return false;

        if (!includeScrapWorthZero && !ConfigManager.SellScrapWorthZero.Value && shipInventoryItemData.ScrapValue <= 0)
        {
            return false;
        }

        if (onlyAllowedScrap && !IsAllowedScrap(shipInventoryItemData, ConfigManager.DontSellListArray))
        {
            return false;
        }

        return true;
    }
    #endregion

    #region Get Scrap to Sell
    public static ScrapToSell GetScrapToSell(SellCommandRequest sellRequest)
    {
        var items = GetAllScrap(sellRequest.OnlyAllowedScrap, sellRequest.OnlyUseShipInventory);
        return sellRequest.GetScrapToSell(items);
    }

    public static ScrapToSell GetScrapToSell(string[] sellList, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        return new ScrapToSell(GetAllScrapByItemNames(sellList, onlyAllowedScrap, onlyUseShipInventory));
    }

    public static int GetRealValue(int value)
    {
        return (int)(value * StartOfRound.Instance.companyBuyingRate);
    }
    #endregion

    #region Get Scrap Message
    public static string GetScrapMessage(List<ItemData> items)
    {
        return GetScrapMessage(items, ConfigManager.SortFoundItemsPrice.Value, ConfigManager.AlignFoundItemsPrice.Value);
    }

    public static string GetScrapMessage(List<ItemData> items, bool sortFoundItemsPrice, bool alignFoundItemsPrice)
    {
        string[] itemNames = items.Select(x => x.ItemName).ToArray();
        int[] scrapValues = items.Select(x => x.ScrapValue).ToArray();
        ItemLocation[] itemLocations = items.Select(x => x.ItemLocation).ToArray();

        return GetScrapMessage(itemNames, scrapValues, itemLocations, sortFoundItemsPrice, alignFoundItemsPrice, color2: TerminalHelper.GreenColor2);
    }

    public static string GetScrapMessage(List<GrabbableObject> grabbableObjects)
    {
        return GetScrapMessage(grabbableObjects, ConfigManager.SortFoundItemsPrice.Value, ConfigManager.AlignFoundItemsPrice.Value);
    }

    public static string GetScrapMessage(List<GrabbableObject> grabbableObjects, bool sortFoundItemsPrice, bool alignFoundItemsPrice)
    {
        string[] itemNames = grabbableObjects.Select(x => x.itemProperties.itemName).ToArray();
        int[] scrapValues = grabbableObjects.Select(x => x.scrapValue).ToArray();
        ItemLocation[] itemLocations = Enumerable.Repeat(ItemLocation.Ship, itemNames.Length).ToArray();

        return GetScrapMessage(itemNames, scrapValues, itemLocations, sortFoundItemsPrice, alignFoundItemsPrice, color2: string.Empty);
    }

    public static string GetScrapMessage(string[] itemNames, int[] scrapValues, ItemLocation[] itemLocations, bool sortFoundItemsPrice, bool alignFoundItemsPrice, string color2)
    {
        // Combine the items with their scrap values and locations, grouping by item name and location
        var combinedScrap = itemNames
            .Select((item, index) => new
            {
                Item = item,
                Value = scrapValues[index],
                Location = itemLocations[index]
            })
            .GroupBy(i => new { i.Item, i.Location }) // Group by both item name and location
            .Select(g => new
            {
                ItemName = g.Key.Item,
                Count = g.Count(),
                TotalValue = g.Sum(i => i.Value),
                g.Key.Location
            })
            .ToList();

        // Sort if needed
        if (sortFoundItemsPrice)
        {
            combinedScrap = combinedScrap
                .OrderByDescending(item => item.TotalValue)
                .ToList();
        }

        // Determine max length of itemName + count strings for alignment
        int maxItemLength = combinedScrap
            .Select(item => GetFormattedItemName(item.ItemName, item.Count, color2).Length)
            .Max();

        // Determine the max length of price strings for alignment
        int maxPriceLength = combinedScrap
            .Select(item => $"${item.TotalValue}")
            .Max(p => p.Length);

        // Build the message
        var messageBuilder = new StringBuilder();

        foreach (var item in combinedScrap)
        {
            string formattedItem = GetFormattedItemName(item.ItemName, item.Count, color2);
            string formattedPrice = GetFormattedPriceWithLocation(item.TotalValue, item.Location, maxPriceLength);

            // Align the price and location based on maxItemLength if alignFoundItemsPrice is true
            if (alignFoundItemsPrice)
            {
                string paddedItem = formattedItem.PadRight(maxItemLength); // Pad item name + count to max length
                messageBuilder.AppendLine($"{paddedItem} {formattedPrice}");
            }
            else
            {
                messageBuilder.AppendLine($"{formattedItem} {formattedPrice}");
            }
        }

        return messageBuilder.ToString().Trim();
    }

    // Helper function to format the item name with count
    private static string GetFormattedItemName(string itemName, int count, string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return $"{itemName} (x{count}) :";
        }
        else
        {
            return $"{itemName} <color={color}>(x{count})</color>";
        }
    }

    // Helper function to get the location text
    private static string GetLocationText(ItemLocation location)
    {
        return location switch
        {
            ItemLocation.Vehicle => $" <color={TerminalHelper.GrayColor}>(Vehicle)</color>",
            ItemLocation.ShipInventory => $" <color={TerminalHelper.GrayColor}>(ShipInventory)</color>",
            _ => string.Empty
        };
    }

    // Helper function to format the price and align location text
    private static string GetFormattedPriceWithLocation(int totalValue, ItemLocation location, int maxPriceLength)
    {
        string price = $"${totalValue}";
        string locationText = GetLocationText(location);

        // Pad the price string to align location text
        return price.PadRight(maxPriceLength) + (string.IsNullOrWhiteSpace(locationText) ? "" : locationText);
    }

    public static string GetScrapItemMessage(List<Item> scrapItems, int columns = 1, int padding = 25)
    {
        if (scrapItems == null || scrapItems.Count == 0) return string.Empty;

        // Calculate number of items per column
        int itemsPerColumn = Mathf.CeilToInt((float)scrapItems.Count / columns);

        // Use StringBuilder for efficient string concatenation
        StringBuilder[] rowBuilders = new StringBuilder[itemsPerColumn];
        for (int j = 0; j < itemsPerColumn; j++)
        {
            rowBuilders[j] = new StringBuilder();
        }

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < itemsPerColumn; j++)
            {
                int index = itemsPerColumn * i + j;
                if (index >= scrapItems.Count) break; // No need to continue if index is out of bounds

                string itemName = scrapItems[index]?.itemName;
                if (string.IsNullOrEmpty(itemName)) continue; // Skip empty or null item names

                // Append the item name to the current row
                rowBuilders[j].Append(itemName.PadRight(padding));
            }
        }

        // Join rows and trim unnecessary spaces
        StringBuilder finalMessage = new StringBuilder();
        foreach (var row in rowBuilders)
        {
            finalMessage.AppendLine(row.ToString().Trim());
        }

        return finalMessage.ToString().Trim(); // Remove the last trailing newline
    }
    #endregion
}
