using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

public class ScrapToSell
{
    public List<GrabbableObject> scrap;
    public int value;

    public ScrapToSell(List<GrabbableObject> scrap)
    {
        this.scrap = scrap;

        value = 0;
        scrap.ForEach(item => value += item.scrapValue);
    }

    public string GetListAsString()
    {
        bool sortFoundItems = SellMyScrapBase.Instance.ConfigManager.SortFoundItems;
        bool alignFoundItemsPrice = SellMyScrapBase.Instance.ConfigManager.AlignFoundItemsPrice;

        return GetListAsString(sortFoundItems, alignFoundItemsPrice);
    }

    public string GetListAsString(bool sortFoundItems, bool alignFoundItemsPrice)
    {
        ScrapItemStackManager itemStackManager = new ScrapItemStackManager();
        scrap.ForEach(item => itemStackManager.AddItem(item.itemProperties.itemName, item.scrapValue));

        // Sort found items
        if (sortFoundItems) itemStackManager.SortItemStackList();

        // Align found items price
        if (alignFoundItemsPrice)
        {
            return GetAlignedListAsString(itemStackManager.itemStacks);
        }

        return GetDefaultListAsString(itemStackManager.itemStacks);
    }

    private string GetDefaultListAsString(List<ScrapItemStack> itemStacks)
    {
        string result = string.Empty;

        itemStacks.ForEach(itemStack =>
        {
            result += $"{itemStack.name} (x{itemStack.amount}) : ${itemStack.value}\n";
        });

        return result.Trim();
    }

    private string GetAlignedListAsString(List<ScrapItemStack> itemStacks)
    {
        List<string> array = new List<string>();
        itemStacks.ForEach(itemStack => array.Add($"{itemStack.name} (x{itemStack.amount}) :"));
        int maxLength = StringUtils.GetLongestStringFromArray(array.ToArray()).Length;

        string result = string.Empty;

        itemStacks.ForEach(itemStack =>
        {
            string a = $"{itemStack.name} (x{itemStack.amount}) :";
            string b = $"${itemStack.value}";
            result += StringUtils.GetStringWithSpacingInBetween(a, b, maxLength) + "\n";
        });

        return result.Trim();
    }
}

// For ScrapToSell.GetListAsString() ONLY
public class ScrapItemStackManager
{
    public List<ScrapItemStack> itemStacks = new List<ScrapItemStack>();

    public void AddItem(string name, int value)
    {
        int itemStackIndex = -1;

        for (int i = 0; i < itemStacks.Count; i++)
        {
            if (name == itemStacks[i].name) itemStackIndex = i;
        }

        if (itemStackIndex == -1)
        {
            itemStacks.Add(new ScrapItemStack(name, 1, value));
            return;
        }

        itemStacks[itemStackIndex].AddItem(value);
    }
    
    public void SortItemStackList()
    {
        List<ScrapItemStack> _itemStacks = new List<ScrapItemStack>(itemStacks);
        List<ScrapItemStack> _sortedItemStacks = new List<ScrapItemStack>();

        while (_itemStacks.Count != 0)
        {
            ScrapItemStack highestItemStack = GetHighestItemStack(_itemStacks);

            _sortedItemStacks.Add(highestItemStack);
            _itemStacks.Remove(highestItemStack);
        }

        itemStacks = _sortedItemStacks;
    }

    public ScrapItemStack GetHighestItemStack(List<ScrapItemStack> _itemStacks)
    {
        ScrapItemStack highestItemStack = null;

        _itemStacks.ForEach(itemStack =>
        {
            // Set first item
            if (highestItemStack == null)
            {
                highestItemStack = itemStack;
                return;
            }

            // Get highest itemStack
            if (itemStack.value > highestItemStack.value)
            {
                highestItemStack = itemStack;
            }
        });

        return highestItemStack;
    }
}

public class ScrapItemStack
{
    public string name;
    public int amount;
    public int value;

    public ScrapItemStack(string name, int amount, int value)
    {
        this.name = name;
        this.amount = amount;
        this.value = value;
    }

    public void AddItem(int value)
    {
        amount++;
        this.value += value;
    }
}
