using com.github.zehsteam.SellMyScrap.Data;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;

/// <summary>
/// Brute force algorithm to find first match of scrap that equals to <see cref="BaseScrapMatchAlgorithm.targetValue"/>.
/// Very unstable and this algorithm's speed can vary, but usually it's fast
/// </summary>
public class BruteForceMatchAlgorithm : BaseScrapMatchAlgorithm
{
    /// <inheritdoc/>
    public override int FlagIndex => 2;

    /// <inheritdoc/>
    protected override List<ItemData> RunScrapMatchAlgorithm(int totalScrapValue)
    {
        var itemsToSell = new List<ItemData>();
        orderedItems = this.items
            .OrderBy(IsPriority)
            .ThenByDescending(i => i.ScrapValue)
            .ToArray();
        bestCombination = new ScrapCombination();

        return RecursiveScrapSearch(bestCombination);
    }

    // This algorithm is an iterative function instead of simple recursion,
    // since otherwise the game will run out of memory
    private ScrapCombination RecursiveScrapSearch(ScrapCombination itemsToSell)
    {
        var lastIndex = 0;

        while (itemsToSell.Count != 0 || lastIndex != orderedItems.Length - 1)
        {
            for (int i = lastIndex; i < orderedItems.Length; i++)
            {
                var item = orderedItems[i];
                if (itemsToSell.TotalScrap + item.ScrapValue == targetValue)
                {
                    itemsToSell.Add(item, i);
                    bestCombination = itemsToSell;
                    return itemsToSell;
                }
                else if (itemsToSell.TotalScrap + item.ScrapValue < targetValue)
                {
                    itemsToSell.Add(item, i);
                }
                else itemsToSell.UpdateIfBetterWith(item);
            }

            // Compare current best combination with current recursive iteration's combination
            bestCombination = bestCombination.SaveBest(itemsToSell);

            // Since no equal combination (the one with total value == target value) was found,
            // remove last item from current combination
            lastIndex = itemsToSell.LastIndex + 1;
            itemsToSell.RemoveLast();
        }

        // If after iterating every possible combination was not found the equal one,
        // return combination with the closest value
        return bestCombination;
    }

    private ItemData[] orderedItems;
    private ScrapCombination bestCombination;

    private class ScrapCombination
    {
        public ScrapCombination()
        {
        }

        protected ScrapCombination(ScrapCombination other) : this()
        {
            TotalScrap = other.TotalScrap;
            TheoreticalLastItem = other.TheoreticalLastItem;
            Items = new List<ItemData>(other.Items);
            Indeces = new List<int>(other.Indeces);
        }

        public int TotalScrap { get; private set; } = 0;
        public ItemData TheoreticalLastItem { get; private set; }
        public int TotalScrapWithLastItem => TotalScrap + (TheoreticalLastItem?.ScrapValue ?? 0);
        public int LastIndex => Indeces[^1];
        private List<ItemData> Items { get; } = [];
        private List<int> Indeces { get; } = [];

        public int Count => Items.Count;

        public void Add(ItemData item, int itemIndex)
        {
            Items.Add(item);
            Indeces.Add(itemIndex);
            TotalScrap += item.ScrapValue;
            TheoreticalLastItem = null;
        }

        public void RemoveLast()
        {
            var item = Items[^1];
            Items.RemoveAt(Count - 1);
            Indeces.RemoveAt(Indeces.Count - 1);
            TotalScrap -= item.ScrapValue;
            TheoreticalLastItem = null;
        }

        public void UpdateIfBetterWith(ItemData item)
        {
            if (TheoreticalLastItem is null || TheoreticalLastItem.ScrapValue > item.ScrapValue)
            {
                TheoreticalLastItem = item;
            }
        }

        public ScrapCombination SaveBest(ScrapCombination other)
        {
            return other.TotalScrapWithLastItem >= TotalScrapWithLastItem ?
                this :
                new ScrapCombination(other);
        }

        public static implicit operator List<ItemData>(ScrapCombination sc)
        {
            var list = new List<ItemData>(sc.Items);
            if (sc.TheoreticalLastItem != null)
                list.Add(sc.TheoreticalLastItem);
            return list;
        }
    }
}
