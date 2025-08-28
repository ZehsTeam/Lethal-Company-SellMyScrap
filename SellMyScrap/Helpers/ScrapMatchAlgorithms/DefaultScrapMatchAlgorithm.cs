using com.github.zehsteam.SellMyScrap.Objects;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;

/// <summary>
/// Default OG algorithm to find the best match of scrap to sell
/// </summary>
public class DefaultScrapMatchAlgorithm : BaseScrapMatchAlgorithm
{
    /// <inheritdoc/>
    public override int FlagIndex => 1;

    /// <inheritdoc/>
    protected override List<ItemData> RunScrapMatchAlgorithm(int totalScrapValue)
    {
        // Step 1: Initialize DP structures with additional priority count tracking
        int maxPossibleValue = totalScrapValue;
        int[] dp = new int[maxPossibleValue + 1];
        List<ItemData>[] dpItems = new List<ItemData>[maxPossibleValue + 1];
        int[] dpPriorityCount = new int[maxPossibleValue + 1]; // Track priority item counts in the DP states

        for (int i = 0; i <= maxPossibleValue; i++)
        {
            dp[i] = int.MaxValue;
            dpItems[i] = new List<ItemData>();
            dpPriorityCount[i] = 0;
        }

        dp[0] = 0; // Base case

        foreach (var item in items)
        {
            bool isPriority = IsPriority(item);
            int itemPriorityValue = isPriority ? 1 : 0;

            for (int j = maxPossibleValue; j >= item.ScrapValue; j--)
            {
                int remainingValue = j - item.ScrapValue;
                if (dp[remainingValue] != int.MaxValue)
                {
                    int newScrapValue = dp[remainingValue] + item.ScrapValue;
                    int newPriorityCount = dpPriorityCount[remainingValue] + itemPriorityValue;

                    // Compare based on scrap value first, then prioritize count if values match
                    if (newScrapValue < dp[j] || newScrapValue == dp[j] && newPriorityCount > dpPriorityCount[j])
                    {
                        dp[j] = newScrapValue;
                        dpItems[j] = new List<ItemData>(dpItems[remainingValue]) { item };
                        dpPriorityCount[j] = newPriorityCount;
                    }
                }
            }
        }

        // Step 2: Return the exact match if possible
        if (dp[targetValue] != int.MaxValue)
        {
            return dpItems[targetValue];
        }

        // Step 3: If exact match is not possible, find the smallest valid over-target solution
        for (int i = targetValue + 1; i <= maxPossibleValue; i++)
        {
            if (dp[i] != int.MaxValue)
            {
                return dpItems[i];
            }
        }

        // Fallback in case no valid set is found (should not happen in usual cases)
        return items;
    }
}
