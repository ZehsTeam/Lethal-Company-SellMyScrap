using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapHelper
{
    public static GameObject HangarShip
    {
        get
        {
            if (_hangarShip == null)
            {
                _hangarShip = GameObject.Find("/Environment/HangarShip");
            }

            return _hangarShip;
        }
    }

    private static GameObject _hangarShip;

    #region Get Scrap
    public static List<GrabbableObject> GetScrapFromShip(bool onlyAllowedScrap = true)
    {
        List<GrabbableObject> items = HangarShip.GetComponentsInChildren<GrabbableObject>().ToList();
        items.AddRange(GetAllItemsFromVehicle());

        List<GrabbableObject> scrap = [];

        foreach (var item in items)
        {
            if (!IsScrapItem(item)) continue;
            if (!Plugin.ConfigManager.SellScrapWorthZero && item.scrapValue <= 0) continue;
            if (Plugin.ConfigManager.OnlySellScrapOnFloor && !IsScrapOnFloor(item)) continue;
            if (onlyAllowedScrap && !IsAllowedScrapItem(item, Plugin.ConfigManager.DontSellListJson)) continue;

            scrap.Add(item);
        }

        return scrap;
    }

    public static List<GrabbableObject> GetAllItemsFromVehicle()
    {
        VehicleController vehicleController = Object.FindFirstObjectByType<VehicleController>();
        if (vehicleController == null) return [];

        return vehicleController.GetComponentsInChildren<GrabbableObject>().ToList();
    }

    public static List<GrabbableObject> GetScrapByItemName(string itemName, bool onlyAllowedScrap = true)
    {
        List<GrabbableObject> scrap = GetScrapFromShip(onlyAllowedScrap);
        List<GrabbableObject> foundScrap = [];

        scrap.ForEach(item =>
        {
            string _itemName = item.itemProperties.itemName;

            if (_itemName.Contains(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                foundScrap.Add(item);
            }
        });

        return foundScrap;
    }

    public static List<GrabbableObject> GetScrapBySellListJson(string[] sellListJson, bool onlyAllowedScrap = false)
    {
        List<GrabbableObject> scrap = [];

        foreach (var itemName in sellListJson)
        {
            scrap.AddRange(GetScrapByItemName(itemName, onlyAllowedScrap));
        }

        return scrap;
    }

    public static List<Item> GetAllScrapItems()
    {
        List<Item> allScrapItems = new List<Item>();

        StartOfRound.Instance.allItemsList.itemsList.ForEach(item =>
        {
            if (IsScrapItem(item))
            {
                allScrapItems.Add(item);
            }
        });

        return allScrapItems;
    }

    private static bool IsScrapItem(GrabbableObject grabbableObject)
    {
        if (grabbableObject == null) return false;
        if (!grabbableObject.itemProperties.isScrap) return false;
        if (grabbableObject.isHeld || grabbableObject.isPocketed || !grabbableObject.grabbable) return false;

        return true;
    }

    private static bool IsScrapItem(Item item)
    {
        if (item == null) return false;
        if (!item.isScrap) return false;

        return true;
    }

    private static bool IsAllowedScrapItem(GrabbableObject grabbableObject, string[] dontSellList)
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;

        string itemName = grabbableObject.itemProperties.itemName;

        if (itemName == "Gift" && !configManager.SellGifts) return false;
        if (itemName == "Shotgun" && !configManager.SellShotguns) return false;
        if (itemName == "Ammo" && !configManager.SellAmmo) return false;
        if (itemName == "Kitchen knife" && !configManager.SellKnives) return false;
        if (itemName == "Jar of pickles" && !configManager.SellPickles) return false;

        // Dont sell list
        foreach (var dontSellItem in dontSellList)
        {
            if (dontSellItem.ToLower() == itemName.ToLower()) return false;
        }

        return true;
    }
    
    private static bool IsScrapOnFloor(GrabbableObject grabbableObject)
    {
        BoxCollider boxCollider = grabbableObject.GetComponent<BoxCollider>();
        if (boxCollider == null) return true;

        Bounds bounds = boxCollider.bounds;
        float shipY = HangarShip.transform.position.y;
        float bottomY = bounds.center.y - bounds.extents.y;
        float yOffset = bottomY - shipY;

        return yOffset <= 0.1f;
    }
    #endregion

    #region Get Scrap to Sell
    public static ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true, bool withOvertimeBonus = false)
    {
        return GetScrapToSell(GetScrapFromShip(onlyAllowedScrap), value, withOvertimeBonus);
    }

    private static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int value, bool withOvertimeBonus = false)
    {
        if (value == int.MaxValue)
        {
            return new ScrapToSell(scrap);
        }

        int targetValue = withOvertimeBonus ? GetSellValueWithOvertime(value) : GetSellValue(value);

        return new ScrapToSell(FindBestMatch(scrap, targetValue));
    }

    public static List<GrabbableObject> FindBestMatch(List<GrabbableObject> scrap, int targetValue)
    {
        // Step 1: Handle empty list or max target value
        if (scrap.Count == 0 || targetValue == int.MaxValue)
            return scrap;

        // Step 2: Find the minimum scrapValue item
        var minScrapItem = scrap.OrderBy(go => go.scrapValue).First();
        if (targetValue <= minScrapItem.scrapValue)
            return [minScrapItem];

        // Step 3: Check if total scrapValue is less than targetValue
        int totalScrapValue = scrap.Sum(go => go.scrapValue);
        if (totalScrapValue < targetValue)
            return scrap;

        // Step 4 and 5: Dynamic Programming approach to find the best match
        int maxPossibleValue = totalScrapValue;
        int[] dp = new int[maxPossibleValue + 1];
        List<GrabbableObject>[] dpItems = new List<GrabbableObject>[maxPossibleValue + 1];

        for (int i = 0; i <= maxPossibleValue; i++)
        {
            dp[i] = int.MaxValue;
            dpItems[i] = new List<GrabbableObject>();
        }

        dp[0] = 0; // Base case

        foreach (var item in scrap)
        {
            for (int j = maxPossibleValue; j >= item.scrapValue; j--)
            {
                int remainingValue = j - item.scrapValue;
                if (dp[remainingValue] != int.MaxValue && dp[remainingValue] + item.scrapValue < dp[j])
                {
                    dp[j] = dp[remainingValue] + item.scrapValue;
                    dpItems[j] = new List<GrabbableObject>(dpItems[remainingValue]) { item };
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
        return scrap;
    }

    public static ScrapToSell GetScrapToSell(string[] sellListJson, bool onlyAllowedScrap = false)
    {
        return new ScrapToSell(GetScrapBySellListJson(sellListJson, onlyAllowedScrap));
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
    public static string GetScrapMessage(List<GrabbableObject> scrap, string color2 = "")
    {
        SyncedConfigManager configManager = Plugin.ConfigManager;
        return GetScrapMessage(scrap, configManager.SortFoundItemsPrice, configManager.AlignFoundItemsPrice, color2);
    }

    public static string GetScrapMessage(List<GrabbableObject> scrap, bool sortFoundItemsPrice, bool alignFoundItemsPrice, string color2 = "")
    {
        List<string> distinctScrap = scrap.Select(item => item.itemProperties.itemName).Distinct().ToList();
        Dictionary<string, int> combinedScrap = [];

        distinctScrap.ForEach(distinctItem =>
        {
            int amount = 0;
            int value = 0;

            scrap.ForEach(item =>
            {
                if (distinctItem != item.itemProperties.itemName) return;

                amount++;
                value += item.scrapValue;
            });

            if (string.IsNullOrWhiteSpace(color2))
            {
                combinedScrap.Add($"{distinctItem} (x{amount}) :", value);
            }
            else
            {
                combinedScrap.Add($"{distinctItem} <color={color2}>(x{amount})</color>  ", value);
            }
        });

        if (sortFoundItemsPrice)
        {
            var sortedCombinedScrap = from entry in combinedScrap orderby entry.Value descending select entry;
            combinedScrap = sortedCombinedScrap.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        string[] combinedScrapKeys = combinedScrap.Keys.ToArray();
        string[] combinedscrapValues = combinedScrap.Values.Select(value => $"${value}").ToArray();

        string message = string.Empty;

        if (alignFoundItemsPrice)
        {
            int maxLength = Utils.GetLongestStringFromArray(combinedScrapKeys).Length;

            for (int i = 0; i < combinedScrap.Count; i++)
            {
                message += Utils.GetStringWithSpacingInBetween(combinedScrapKeys[i], combinedscrapValues[i], maxLength) + "\n";
            }

            return message.Trim();
        }

        for (int i = 0; i < combinedScrap.Count; i++)
        {
            message += $"{combinedScrapKeys[i]} {combinedscrapValues[i]}\n";
        }

        return message.Trim();
    }
    
    public static string GetScrapItemMessage(List<Item> scrapItems, int columns = 1, int padding = 25)
    {
        int itemsPerColumn = Mathf.CeilToInt((float)scrapItems.Count / (float)columns);
        string[] rows = new string[itemsPerColumn];

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < itemsPerColumn; j++)
            {
                int index = itemsPerColumn * i + j;
                if (index > scrapItems.Count - 1) continue;

                string itemName = scrapItems[index].itemName;
                if (itemName == string.Empty) continue;

                rows[j] += itemName.PadRight(padding);

                if (i == columns - 1)
                {
                    rows[j] = rows[j].Trim();
                }
            }
        }

        return string.Join('\n', rows).Trim();
    }
    #endregion
}
