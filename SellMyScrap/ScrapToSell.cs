using System.Collections.Generic;

namespace SellMyScrap
{
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
            ScrapItemStackManager itemStackManager = new ScrapItemStackManager();
            scrap.ForEach(item => itemStackManager.AddItem(item.itemProperties.itemName, item.scrapValue));
            List<ScrapItemStack> sortedItemStacks = itemStackManager.GetSortedItemStackList();

            if (SellMyScrapBase.Instance.ConfigManager.AlignFoundItemsPrice)
            {
                return GetAlignedSortedListAsString(sortedItemStacks);
            }

            return GetNormalSortedListAsString(sortedItemStacks);
        }

        private string GetNormalSortedListAsString(List<ScrapItemStack> sortedItemStacks)
        {
            string result = string.Empty;

            sortedItemStacks.ForEach(item =>
            {
                result += $"{item.name} (x{item.amount}) : ${item.value}\n";
            });

            return result.Trim();
        }

        private string GetAlignedSortedListAsString(List<ScrapItemStack> sortedItemStacks)
        {
            PricedLogMessageManager logMessageManager = new PricedLogMessageManager();
            sortedItemStacks.ForEach(item =>
            {
                logMessageManager.Add($"{item.name} (x{item.amount}) :", item.value);
            });
            List<PricedLogMessage> logMessages = logMessageManager.GetSortedLogMessages();

            string result = string.Empty;

            logMessages.ForEach(item =>
            {
                result += $"{item.GetMessage()}\n";
            });

            return result.Trim();
        }
    }

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
    
        public List<ScrapItemStack> GetSortedItemStackList()
        {
            List<ScrapItemStack> _itemStacks = new List<ScrapItemStack>(itemStacks);
            List<ScrapItemStack> _sortedItemStacks = new List<ScrapItemStack>();

            while (_itemStacks.Count != 0)
            {
                ScrapItemStack highestItemStack = GetHighestItemStack(_itemStacks);

                _sortedItemStacks.Add(highestItemStack);
                _itemStacks.Remove(highestItemStack);
            }

            return _sortedItemStacks;
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

    public class PricedLogMessageManager
    {
        List<PricedLogMessage> logMessages = new List<PricedLogMessage>();

        public void Add(string message, int price)
        {
            logMessages.Add(new PricedLogMessage(message, price));
        }

        public List<PricedLogMessage> GetSortedLogMessages()
        {
            PricedLogMessage longestLogMessage = GetLongestLogMessage(logMessages);
            List<PricedLogMessage> sortedLogMessages = new List<PricedLogMessage>(logMessages);

            sortedLogMessages.ForEach(logMessage =>
            {
                if (logMessage == longestLogMessage) return;

                logMessage.SetOffset(longestLogMessage.message.Length);
            });

            return sortedLogMessages;
        }

        public PricedLogMessage GetLongestLogMessage(List<PricedLogMessage> _logMessages)
        {
            PricedLogMessage longestLogMessage = null;

            _logMessages.ForEach(logMessage =>
            {
                // Set first item
                if (longestLogMessage == null)
                {
                    longestLogMessage = logMessage;
                    return;
                }

                // Get highest itemStack
                if (logMessage.message.Length > longestLogMessage.message.Length)
                {
                    longestLogMessage = logMessage;
                }
            });

            return longestLogMessage;
        }
    }

    public class PricedLogMessage
    {
        public string message;
        public int price;
        public int offset;

        public PricedLogMessage(string message, int price)
        {
            this.message = message;
            this.price = price;
            offset = 0;
        }

        public void SetOffset(int maxLength)
        {
            offset = maxLength - message.Length;
        }

        public string GetMessage()
        {
            string result = message;

            for (int i = 0; i < offset; i++)
            {
                result += " ";
            }

            result += $" ${price}";

            return result;
        }
    }
}
