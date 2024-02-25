using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapHelper
{
    public static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int totalValue)
    {
        return new ScrapToSell(FindBestMatch(scrap, GetSellValue(totalValue)));
    }

    private static List<GrabbableObject> FindBestMatch(List<GrabbableObject> scrap, int target)
    {
        int totalValue = scrap.Sum(item => item.scrapValue);

        if (totalValue <= target)
        {
            // If total value is under or equal to the quota, return all items
            return scrap;
        }

        int n = scrap.Count;
        int[,] dp = new int[n + 1, target + 1];

        // Fill the dp array
        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= target; j++)
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
        int total = target;
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
                    int overAmount = dp[i, target] - target;
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
        return (int)Mathf.Ceil(value / StartOfRound.Instance.companyBuyingRate);
    }

    public static int GetRealValue(int value)
    {
        return (int)(value * StartOfRound.Instance.companyBuyingRate);
    }

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
}
