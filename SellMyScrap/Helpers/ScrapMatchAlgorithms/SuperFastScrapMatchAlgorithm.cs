using com.github.zehsteam.SellMyScrap.Objects;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;

/// <summary>
/// Super fast algorithm to find any match of scrap to sell that fulfills or exceeds the <see cref="BaseScrapMatchAlgorithm.targetValue"/>
/// </summary>
public class SuperFastScrapMatchAlgorithm : BaseScrapMatchAlgorithm
{
    /// <inheritdoc/>
    public override int FlagIndex => 3;

    /// <inheritdoc/>
    protected override List<ItemData> RunScrapMatchAlgorithm(int totalScrapValue)
    {
        var currentSellValue = 0;
        var itemsToSell = new List<ItemData>();
        var items = this.items.OrderBy(i => i.ItemName);

        // Step 1: Calculate all priority scraps first
        var priorityItems = items.Where(IsPriority).ToList();
        AddToSellWhileNotFulfilled(priorityItems, ref currentSellValue, itemsToSell);

        if (currentSellValue >= targetValue) return itemsToSell;
            
        // Step 2: Calculate other scraps if priority scrap wasn't enough
        var otherItems = items.Except(priorityItems).ToList();
        AddToSellWhileNotFulfilled(otherItems, ref currentSellValue, itemsToSell);

        return itemsToSell;
    }

    private void AddToSellWhileNotFulfilled(List<ItemData> items, ref int currentSellValue, List<ItemData> itemsToSell)
    {
        for (int i = 0; i < items.Count && currentSellValue < targetValue; i++)
        {
            currentSellValue += items[i].ScrapValue;
            itemsToSell.Add(items[i]);
        }
    }
}
