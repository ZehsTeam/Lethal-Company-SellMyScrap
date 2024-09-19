using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal static class ScrapHelper
{
    public static Transform HangarShipTransform => StartOfRound.Instance.elevatorTransform;

    #region Get Scrap
    private static List<GrabbableObject> GetValidScrap(IEnumerable<GrabbableObject> grabbableObjects, bool onlyAllowedScrap)
    {
        if (grabbableObjects == null) return [];

        return grabbableObjects.Where(x => IsValidScrap(x, onlyAllowedScrap)).ToList();
    }

    public static List<GrabbableObject> GetScrapFromShip(bool onlyAllowedScrap = true)
    {
        if (HangarShipTransform == null) return [];

        return GetValidScrap(HangarShipTransform.GetComponentsInChildren<GrabbableObject>(), onlyAllowedScrap);
    }

    public static List<GrabbableObject> GetScrapFromVehicle(bool onlyAllowedScrap = true)
    {
        VehicleController vehicleController = Object.FindFirstObjectByType<VehicleController>();
        if (vehicleController == null) return [];

        return GetValidScrap(vehicleController.GetComponentsInChildren<GrabbableObject>(), onlyAllowedScrap);
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

    public static List<ItemData> GetAllScrap(bool onlyAllowedScrap = true, bool onlyUseShipInventory = false)
    {
        ShipInventoryItemData[] shipInventoryItems = [];

        if (ShipInventoryProxy.Enabled)
        {
            shipInventoryItems = ShipInventoryProxy.GetItems().Where(x => IsValidScrap(x, onlyAllowedScrap)).ToArray();

            if (onlyUseShipInventory)
            {
                return GetItemDataList([], [], shipInventoryItems);
            }
        }

        return GetItemDataList(GetScrapFromShip(onlyAllowedScrap), GetScrapFromVehicle(onlyAllowedScrap), shipInventoryItems);
    }

    public static List<ItemData> GetAllScrapByItemName(string itemName, bool matchCase = false, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

        return GetAllScrap(onlyAllowedScrap, onlyUseShipInventory).Where(item =>
        {
            return item.ItemName.Contains(itemName, comparisonType);
        }).ToList();
    }

    public static List<ItemData> GetAllScrapByItemNames(string[] itemNames, bool matchCase = false, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

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

        return IsAllowedScrap(shipInventoryItemData.ItemName, dontSellItemNames, matchCase);
    }

    public static bool IsAllowedScrap(string itemName, string[] dontSellItemNames, bool matchCase = false)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

        if (itemName.Equals("Gift", comparisonType) && !Plugin.ConfigManager.SellGifts) return false;
        if (itemName.Equals("Shotgun", comparisonType) && !Plugin.ConfigManager.SellShotguns) return false;
        if (itemName.Equals("Ammo", comparisonType) && !Plugin.ConfigManager.SellAmmo) return false;
        if (itemName.Equals("Kitchen knife", comparisonType) && !Plugin.ConfigManager.SellKnives) return false;
        if (itemName.Equals("Jar of pickles", comparisonType) && !Plugin.ConfigManager.SellPickles) return false;

        foreach (var dontSellItemName in dontSellItemNames)
        {
            if (itemName.Contains(dontSellItemName, comparisonType))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidScrap(GrabbableObject grabbableObject, bool onlyAllowedScrap)
    {
        if (grabbableObject == null) return false;

        if (!IsScrap(grabbableObject)) return false;

        if (!Plugin.ConfigManager.SellScrapWorthZero && grabbableObject.scrapValue <= 0)
        {
            return false;
        }

        if (Plugin.ConfigManager.OnlySellScrapOnFloor && !IsScrapOnFloor(grabbableObject))
        {
            return false;
        }

        if (onlyAllowedScrap && !IsAllowedScrap(grabbableObject, Plugin.ConfigManager.DontSellList))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidScrap(ShipInventoryItemData shipInventoryItemData, bool onlyAllowedScrap)
    {
        if (shipInventoryItemData == null) return false;

        if (!Plugin.ConfigManager.SellScrapWorthZero && shipInventoryItemData.ScrapValue <= 0)
        {
            return false;
        }

        if (onlyAllowedScrap && !IsAllowedScrap(shipInventoryItemData, Plugin.ConfigManager.DontSellList))
        {
            return false;
        }

        return true;
    }

    public static bool IsScrapOnFloor(GrabbableObject grabbableObject)
    {
        if (grabbableObject == null) return false;

        BoxCollider boxCollider = grabbableObject.GetComponent<BoxCollider>();
        if (boxCollider == null) return true;

        Bounds bounds = boxCollider.bounds;
        float shipY = HangarShipTransform.position.y;
        float bottomY = bounds.center.y - bounds.extents.y;
        float yOffset = bottomY - shipY;

        return yOffset <= 0.1f;
    }
    #endregion

    #region Get Scrap to Sell
    public static ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true, bool withOvertimeBonus = false, bool onlyUseShipInventory = false)
    {
        return GetScrapToSell(GetAllScrap(onlyAllowedScrap, onlyUseShipInventory), value, withOvertimeBonus);
    }

    private static ScrapToSell GetScrapToSell(List<ItemData> items, int value, bool withOvertimeBonus = false)
    {
        if (value == int.MaxValue)
        {
            return new ScrapToSell(items);
        }

        int targetValue = withOvertimeBonus ? GetSellValueWithOvertime(value) : GetSellValue(value);

        return new ScrapToSell(FindBestMatch(items, targetValue));
    }

    public static List<ItemData> FindBestMatch(List<ItemData> items, int targetValue)
    {
        // Step 1: Handle empty list or max target value
        if (items.Count == 0 || targetValue == int.MaxValue)
            return items;

        // Step 2: Find the minimum scrapValue item
        var minScrapItem = items.OrderBy(go => go.ScrapValue).First();
        if (targetValue <= minScrapItem.ScrapValue)
            return [minScrapItem];

        // Step 3: Check if total scrapValue is less than targetValue
        int totalScrapValue = items.Sum(go => go.ScrapValue);
        if (totalScrapValue < targetValue)
            return items;

        // Step 4 and 5: Dynamic Programming approach to find the best match
        int maxPossibleValue = totalScrapValue;
        int[] dp = new int[maxPossibleValue + 1];
        List<ItemData>[] dpItems = new List<ItemData>[maxPossibleValue + 1];

        for (int i = 0; i <= maxPossibleValue; i++)
        {
            dp[i] = int.MaxValue;
            dpItems[i] = new List<ItemData>();
        }

        dp[0] = 0; // Base case

        foreach (var item in items)
        {
            for (int j = maxPossibleValue; j >= item.ScrapValue; j--)
            {
                int remainingValue = j - item.ScrapValue;
                if (dp[remainingValue] != int.MaxValue && dp[remainingValue] + item.ScrapValue < dp[j])
                {
                    dp[j] = dp[remainingValue] + item.ScrapValue;
                    dpItems[j] = new List<ItemData>(dpItems[remainingValue]) { item };
                }
            }
        }

        // Exact match check
        if (dp[targetValue] != int.MaxValue)
        {
            return dpItems[targetValue];
        }

        // Smallest over target check
        for (int i = targetValue + 1; i <= maxPossibleValue; i++)
        {
            if (dp[i] != int.MaxValue)
            {
                return dpItems[i];
            }
        }

        // Fallback in case no valid set is found (which shouldn't happen)
        return items;
    }

    public static ScrapToSell GetScrapToSell(string[] sellList, bool onlyAllowedScrap = false, bool onlyUseShipInventory = false)
    {
        return new ScrapToSell(GetAllScrapByItemNames(sellList, onlyAllowedScrap, onlyUseShipInventory));
    }

    private static int GetSellValue(int value)
    {
        if (value == int.MaxValue) return value;
        return Mathf.CeilToInt(value / StartOfRound.Instance.companyBuyingRate);
    }

    private static int GetSellValueWithOvertime(int value)
    {
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int valueOver = (quotaFulfilled + value) - profitQuota;
        if (valueOver <= 0) return GetSellValue(value);

        int profitQuotaLeft = Mathf.Max(profitQuota - quotaFulfilled, 0);
        value -= (TimeOfDayPatch.GetDaysUntilDeadline() + 1) * 15;
        int newValue = Mathf.CeilToInt((5 * value + profitQuotaLeft + 75) / 6f);

        return GetSellValue(newValue);
    }

    public static int GetRealValue(int value)
    {
        return (int)(value * StartOfRound.Instance.companyBuyingRate);
    }
    #endregion

    #region Get Scrap Message
    public static string GetScrapMessage(List<ItemData> items)
    {
        return GetScrapMessage(items, Plugin.ConfigManager.SortFoundItemsPrice, Plugin.ConfigManager.AlignFoundItemsPrice);
    }

    public static string GetScrapMessage(List<ItemData> items, bool sortFoundItemsPrice, bool alignFoundItemsPrice)
    {
        string[] itemNames = items.Select(x => x.ItemName).ToArray();
        int[] scrapValues = items.Select(x => x.ScrapValue).ToArray();
        ItemLocation[] itemLocations = items.Select(x => x.ItemLocation).ToArray();

        return GetScrapMessage(itemNames, scrapValues, itemLocations, sortFoundItemsPrice, alignFoundItemsPrice, color2: TerminalPatch.GreenColor2);
    }
    
    public static string GetScrapMessage(List<GrabbableObject> grabbableObjects)
    {
        return GetScrapMessage(grabbableObjects, Plugin.ConfigManager.SortFoundItemsPrice, Plugin.ConfigManager.AlignFoundItemsPrice);
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
            ItemLocation.Vehicle => $" <color={TerminalPatch.GrayColor}>(Vehicle)</color>",
            ItemLocation.ShipInventory => $" <color={TerminalPatch.GrayColor}>(ShipInventory)</color>",
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
