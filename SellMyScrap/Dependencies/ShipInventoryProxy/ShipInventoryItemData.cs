using ShipInventory.Helpers;
using ShipInventory.Objects;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[System.Serializable]
public class ShipInventoryItemData : INetworkSerializable
{
    public string Id;
    public int ScrapValue;
    public int SaveData;

    public ShipInventoryItemData() { }

    public ShipInventoryItemData(string id, int scrapValue, int saveData)
    {
        Id = id;
        ScrapValue = scrapValue;
        SaveData = saveData;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ShipInventoryItemData(ItemData itemData)
    {
        try
        {
            Id = itemData.ID;
            ScrapValue = itemData.SCRAP_VALUE;
            SaveData = itemData.SAVE_DATA;
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to create ShipInventoryItemData. {ex}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public bool MatchesItemData(ItemData itemData)
    {
        try
        {
            if (itemData.Equals(default))
            {
                return false;
            }

            if (itemData.ID != Id) return false;
            if (itemData.SCRAP_VALUE != ScrapValue) return false;
            if (itemData.SAVE_DATA != SaveData) return false;

            return true;
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to compare ShipInventory ItemData. {ex}");
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ItemData GetItemData()
    {
        try
        {
            return ItemManager.GetItems().FirstOrDefault(MatchesItemData);
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to get ShipInventory ItemData. {ex}");
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public Item GetItem()
    {
        try
        {
            ItemData itemData = GetItemData();

            if (itemData.Equals(default))
            {
                Plugin.Logger.LogError($"Failed to get ShipInventory item. Item does not exist in the ShipInventory storage.");
                return null;
            }

            return itemData.GetItem();
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to get ShipInventory Item. {ex}");
        }

        return null;
    }

    public string GetItemName()
    {
        return GetItem()?.itemName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref ScrapValue);
        serializer.SerializeValue(ref SaveData);
    }
}
