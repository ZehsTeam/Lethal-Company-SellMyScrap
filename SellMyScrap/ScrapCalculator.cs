using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

internal class ScrapCalculator
{
    public static ScrapToSell GetScrapToSell(List<GrabbableObject> scrap, int quota, float rate)
    {
        int target = (int)((float)quota / rate);
        int progress = 0;
        List<GrabbableObject> foundScrap = new List<GrabbableObject>();

        // Get highest items
        while (true)
        {
            GrabbableObject item = GetHighestItem(scrap, target - progress);
            if (item == null) break;

            foundScrap.Add(item);
            scrap.Remove(item);
            progress += item.scrapValue;
        }

        // Needs one more scrap, get lowest item
        if (target - progress > 0)
        {
            GrabbableObject item = GetLowestItem(scrap);

            if (item != null)
            {
                foundScrap.Add(item);
                scrap.Remove(item);
                progress += item.scrapValue;
            }
        }

        if (progress == 0 || scrap.Count == 0) return new ScrapToSell(foundScrap); // Found exact quota or no scrap left

        int difference = progress - target;
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

            if (item.scrapValue == target) selected = item;
        });

        return selected;
    }

    public static GrabbableObject GetHighestItem(List<GrabbableObject> scrap, int target)
    {
        bool search = true;
        GrabbableObject selected = null;

        scrap.ForEach(item =>
        {
            if (!search) return;

            // First item
            if (selected == null)
            {
                if (item.scrapValue > target) return;

                selected = item;
                return;
            }

            // Find better item
            if (item.scrapValue > selected.scrapValue && item.scrapValue <= target)
            {
                selected = item;
                return;
            }

            if (selected.scrapValue == target) search = false; // Found perfect match
        });

        return selected;
    }

    public static GrabbableObject GetLowestItem(List<GrabbableObject> scrap)
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

            // Find better item
            if (item.scrapValue < selected.scrapValue)
            {
                selected = item;
                return;
            }
        });

        return selected;
    }
}
