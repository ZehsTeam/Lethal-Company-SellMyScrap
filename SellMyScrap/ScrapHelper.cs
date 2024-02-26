using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapHelper
{
    private static GameObject hangarShip;
    public static GameObject HangarShip
    {
        get
        {
            if (hangarShip == null)
            {
                hangarShip = GameObject.Find("/Environment/HangarShip");
            }

            return hangarShip;
        }
    }

    #region Get Scrap
    public static List<GrabbableObject> GetScrapFromShip(bool onlyAllowedScrap = true)
    {
        GrabbableObject[] itemsInShip = HangarShip.GetComponentsInChildren<GrabbableObject>();
        List<GrabbableObject> scrap = new List<GrabbableObject>();

        string[] dontSellList = SellMyScrapBase.Instance.ConfigManager.DontSellListJson;

        foreach (var item in itemsInShip)
        {
            if (!IsScrapItem(item)) continue;
            if (onlyAllowedScrap && !IsAllowedScrapItem(item, dontSellList)) continue;

            scrap.Add(item);
        }

        return scrap;
    }

    private static bool IsScrapItem(GrabbableObject item)
    {
        if (!item.itemProperties.isScrap) return false;
        if (item.isPocketed) return false;
        if (item.isHeld) return false;

        return true;
    }

    private static bool IsAllowedScrapItem(GrabbableObject item, string[] dontSellList)
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        if (item.scrapValue <= 0 && !configManager.SellScrapWorthZero) return false;
        if (configManager.OnlySellScrapOnFloor && !IsScrapOnFloor(item)) return false;

        string itemName = item.itemProperties.itemName;

        if (itemName == "Gift" && !configManager.SellGifts) return false;
        if (itemName == "Shotgun" && !configManager.SellShotguns) return false;
        if (itemName == "Ammo" && !configManager.SellAmmo) return false;
        if (itemName == "Jar of pickles" && !configManager.SellPickles) return false;

        // Dont sell list
        foreach (var dontSellItem in dontSellList)
        {
            if (dontSellItem.ToLower() == itemName.ToLower()) return false;
        }

        return true;
    }
    
    private static bool IsScrapOnFloor(GrabbableObject item)
    {
        BoxCollider boxCollider = item.GetComponent<BoxCollider>();
        if (boxCollider == null) return true;

        Bounds bounds = boxCollider.bounds;
        float shipY = HangarShip.transform.position.y;
        float bottomY = bounds.center.y - bounds.extents.y;
        float yOffset = bottomY - shipY;

        return yOffset <= 0.1f;
    }
    #endregion

    #region Get Scrap to Sell
    public static ScrapToSell GetScrapToSell(int value, bool onlyAllowedScrap = true)
    {
        return GetScrapToSell(GetScrapFromShip(onlyAllowedScrap), value);
    }

    private static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int value)
    {
        return new ScrapToSell(FindBestMatch(scrap, GetSellValue(value)));
    }

    private static List<GrabbableObject> FindBestMatch(List<GrabbableObject> scrap, int targetValue)
    {
        if (scrap.Count == 0) return scrap;

        if (targetValue < scrap.Min(item => item.scrapValue))
        {
            // If quota is less than the value of the lowest value item,
            // return a list containing only the lowest value item
            var lowestValueItem = scrap.OrderBy(item => item.scrapValue).First();
            return [lowestValueItem];
        }

        int totalValue = scrap.Sum(item => item.scrapValue);

        // If total value is under or equal to the quota, return all items
        if (totalValue <= targetValue) return scrap;

        int n = scrap.Count;
        int[,] dp = new int[n + 1, targetValue + 1];

        // Fill the dp array
        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= targetValue; j++)
            {
                if (i == 0 || j == 0)
                    dp[i, j] = 0;
                else if (scrap[i - 1].scrapValue <= j)
                    dp[i, j] = Math.Max(dp[i - 1, j], scrap[i - 1].scrapValue + dp[i - 1, j - scrap[i - 1].scrapValue]);
                else
                    dp[i, j] = dp[i - 1, j];
            }
        }

        // Reconstruct the solution
        List<GrabbableObject> bestMatch = new List<GrabbableObject>();
        int total = targetValue;
        for (int i = n; i > 0; i--)
        {
            if (dp[i, total] != dp[i - 1, total])
            {
                bestMatch.Add(scrap[i - 1]);
                total -= scrap[i - 1].scrapValue;
            }
        }

        // If an exact match is not found, find the best match that is the smallest amount over the quota
        if (total != 0)
        {
            int smallestOverAmount = int.MaxValue;
            List<GrabbableObject> smallestOverMatch = null;

            for (int i = n; i >= 0; i--)
            {
                if (dp[i, total] == total)
                {
                    int overAmount = dp[i, targetValue] - targetValue;
                    if (overAmount < smallestOverAmount)
                    {
                        smallestOverAmount = overAmount;
                        smallestOverMatch = new List<GrabbableObject>();
                        for (int j = i; j > 0; j--)
                        {
                            if (dp[j, total] != dp[j - 1, total])
                            {
                                smallestOverMatch.Add(scrap[j - 1]);
                                total -= scrap[j - 1].scrapValue;
                            }
                        }
                    }
                }
            }

            if (smallestOverMatch != null)
                bestMatch = smallestOverMatch;
        }

        return bestMatch;
    }

    private static int GetSellValue(int value)
    {
        if (value == int.MaxValue) return value;
        return (int)Mathf.Ceil(value / StartOfRound.Instance.companyBuyingRate);
    }

    public static int GetRealValue(int value)
    {
        return (int)(value * StartOfRound.Instance.companyBuyingRate);
    }
    #endregion

    #region Get Scrap Message
    public static string GetScrapMessage(List<GrabbableObject> scrap)
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;
        return GetScrapMessage(scrap, configManager.SortFoundItems, configManager.AlignFoundItemsPrice);
    }

    public static string GetScrapMessage(List<GrabbableObject> scrap, bool sortFoundItems, bool alignFoundItemsPrice)
    {
        List<string> distinctScrap = scrap.Select(item => item.itemProperties.itemName).Distinct().ToList();
        Dictionary<string, int> combinedScrap = new Dictionary<string, int>();

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

            combinedScrap.Add($"{distinctItem} (x{amount}) :", value);
        });

        if (sortFoundItems)
        {
            var sortedCombinedScrap = from entry in combinedScrap orderby entry.Value descending select entry;
            combinedScrap = sortedCombinedScrap.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        string[] combinedScrapKeys = combinedScrap.Keys.ToArray();
        string[] combinedScrapValues = combinedScrap.Values.Select(value => $"${value}").ToArray();

        string message = string.Empty;

        if (alignFoundItemsPrice)
        {
            int maxLength = Utils.GetLongestStringFromArray(combinedScrapKeys).Length;

            for (int i = 0; i < combinedScrap.Count; i++)
            {
                message += Utils.GetStringWithSpacingInBetween(combinedScrapKeys[i], combinedScrapValues[i], maxLength) + "\n";
            }

            return message.Trim();
        }

        for (int i = 0; i < combinedScrap.Count; i++)
        {
            message += $"{combinedScrapKeys[i]} {combinedScrapValues[i]}\n";
        }

        return message.Trim();
    }
    #endregion
}
