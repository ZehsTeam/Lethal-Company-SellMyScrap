using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapCalculator
{
    public static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int amount, float rate)
    {
        int target = (int)Mathf.Ceil(amount / rate);
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
}
