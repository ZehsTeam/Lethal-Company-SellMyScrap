using com.github.zehsteam.SellMyScrap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;

/// <summary>
/// Base abstract class for scrap match algorithms.
/// </summary>
public abstract class BaseScrapMatchAlgorithm
{
    /// <summary>All items available to sell</summary>
    protected List<ItemData> items;
    /// <summary>Requested target value that must be fulfilled</summary>
    protected int targetValue;
    /// <summary>Priority scrap names <see cref="HashSet{T}"/> for quick case-insensitive lookups</summary>
    protected HashSet<string> prioritySet;

    /// <summary>
    /// Checks for edge cases and uses an algorithm to find a suitable combination of scrap to fulfill <see cref="targetValue"/>.
    /// </summary>
    /// <returns>Scrap list to sell from given <see cref="items"/> to fulfill <see cref="targetValue"/>.</returns>
    public List<ItemData> FindMatch(List<ItemData> items, int targetValue, string[] priorityList = null)
    {
        this.items = items;
        this.targetValue = targetValue;
        this.prioritySet = new HashSet<string>(priorityList ?? [], System.StringComparer.OrdinalIgnoreCase);

        // Step 1: Handle edge cases
        if (items.Count == 0 || targetValue == int.MaxValue)
        {
            return items;
        }

        // Step 2: Find the minimum scrapValue item
        var minScrapItem = items.OrderBy(item => item.ScrapValue).First();

        if (targetValue <= minScrapItem.ScrapValue)
        {
            return [minScrapItem];
        }

        // Step 3: Check if total scrapValue is less than targetValue
        int totalScrapValue = items.Sum(item => item.ScrapValue);

        if (totalScrapValue < targetValue)
        {
            return items;
        }

        // Step 4: Use algorithm logic to find the best match to return
        return RunScrapMatchAlgorithm(totalScrapValue);
    }

    /// <summary>
    /// Checks if given <paramref name="item"/> is contained inside of a <see cref="prioritySet"/> HashSet
    /// </summary>
    /// <param name="item">Scrap Item</param>
    /// <returns><see langword="true"/> if <see cref="prioritySet"/> has <paramref name="item"/>; otherwise, <see langword="false"/></returns>
    protected bool IsPriority(ItemData item) => prioritySet.Contains(item.ItemName);

    /// <summary>
    /// Flag in the terminal to activate this algorithm (ex: 'sell quota -a1', then this property should return '1'). 
    /// </summary>
    public abstract int FlagIndex { get; }

    /// <summary>
    /// Runs scrap match searching algorithm to find a suitable combination assuming that edge cases were already checked.
    /// </summary>
    /// <param name="totalScrapValue">Total scrap value on the ship</param>
    /// <returns>Scrap list to sell from given <see cref="items"/> to fulfill <see cref="targetValue"/>.</returns>
    protected abstract List<ItemData> RunScrapMatchAlgorithm(int totalScrapValue);

    /// <param name="flagIndex">Flag (ex: 'sell quota -a1' => 1)</param>
    /// <returns><see cref="BaseScrapMatchAlgorithm"/> for given <paramref name="flagIndex"/></returns>
    public static BaseScrapMatchAlgorithm GetAlgorithmByFlag(int flagIndex)
    {
        // Get all types from current assembly
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyTypes = assembly.GetTypes();

        // Iterate every child of BaseScrapMatchAlgorithm
        foreach (var scrapMatchAlgorithmType in assemblyTypes
            .Where(typeof(BaseScrapMatchAlgorithm).IsAssignableFrom))
        {
            // Check for abstraction, should not attempt to instantiate abstract classes as it'll lead to an exception
            if (scrapMatchAlgorithmType.IsAbstract) continue;

            // Instantiate currently observed algorithm
            var scrapMatchAlgorithmObject = (BaseScrapMatchAlgorithm)Activator.CreateInstance(scrapMatchAlgorithmType);

            // If flag equals, return the algorithm
            if (flagIndex == scrapMatchAlgorithmObject.FlagIndex)
            {
                return scrapMatchAlgorithmObject;
            }
        }

        // In case no algorithm was found by given flag the code returns the default one
        return Default;
    }

    /// <summary>
    /// Get default scrap match algorithm
    /// </summary>
    public static BaseScrapMatchAlgorithm Default => new DefaultScrapMatchAlgorithm();
}
