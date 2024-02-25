using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapHelper
{
    public static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int amount)
    {
        int target = GetSellValue(amount);
        int remaining = target;
        List<GrabbableObject> foundScrap = new List<GrabbableObject>();

        // Get highest value items
        while (true)
        {
            GrabbableObject item = GetHighestItem(scrap, remaining);
            if (item == null) break;

            foundScrap.Add(item);
            scrap.Remove(item);
            remaining -= item.scrapValue;
        }

        // Needs one more scrap, get lowest value item to match
        if (remaining > 0)
        {
            GrabbableObject item = GetLowestItem(scrap, remaining);

            if (item != null)
            {
                foundScrap.Add(item);
                scrap.Remove(item);
                remaining -= item.scrapValue;
            }
        }

        if (remaining == 0 || scrap.Count == 0) return new ScrapToSell(foundScrap); // Found exact value or no scrap left

        int difference = Mathf.Abs(remaining);
        GrabbableObject replacement = null;
        GrabbableObject previous = null;

        foundScrap.ForEach(item =>
        {
            if (replacement != null) return;

            GrabbableObject found = GetExactItem(scrap, item.scrapValue - difference);

            if (found != null)
            {
                previous = item;
                replacement = found;
            }
        });

        if (replacement != null)
        {
            foundScrap.Add(replacement);
            foundScrap.Remove(previous);
            scrap.Remove(replacement);

            return new ScrapToSell(foundScrap);
        }

        return new ScrapToSell(foundScrap);
    }

    public static GrabbableObject GetExactItem(List<GrabbableObject> scrap, int target)
    {
        GrabbableObject selected = null;

        scrap.ForEach(item =>
        {
            if (selected != null) return;

            if (item.scrapValue == target)
            {
                selected = item;
                return;
            }
        });

        return selected;
    }

    public static GrabbableObject GetHighestItem(List<GrabbableObject> scrap, int target)
    {
        GrabbableObject selected = null;

        scrap.ForEach(item =>
        {
            // First item
            if (selected == null)
            {
                if (item.scrapValue > target) return;

                selected = item;
                return;
            }

            // Found exact match
            if (item.scrapValue == target)
            {
                selected = item;
                return;
            }

            // Find better item
            if (item.scrapValue < target && item.scrapValue > selected.scrapValue)
            {
                selected = item;
                return;
            }
        });

        return selected;
    }

    public static GrabbableObject GetLowestItem(List<GrabbableObject> scrap, int target)
    {
        GrabbableObject selected = null;

        scrap.ForEach(item =>
        {
            // First item
            if (selected == null)
            {
                selected = item;
                return;
            }

            // Found exact match
            if (item.scrapValue == target)
            {
                selected = item;
                return;
            }

            // Find better item.
            if (item.scrapValue > target && item.scrapValue < selected.scrapValue)
            {
                selected = item;
                return;
            }
        });

        return selected;
    }

    private static int GetSellValue(int value)
    {
        return (int)Mathf.Floor(value / StartOfRound.Instance.companyBuyingRate);
    }

    public static int GetRealValue(int value)
    {
        return (int)Mathf.Floor(value * StartOfRound.Instance.companyBuyingRate);
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
