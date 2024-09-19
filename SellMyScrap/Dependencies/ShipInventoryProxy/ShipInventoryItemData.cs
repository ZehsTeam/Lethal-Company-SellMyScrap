using ShipInventory.Objects;
using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Serializable]
public class ShipInventoryItemData : INetworkSerializable
{
    public Item Item => ShipInventoryProxy.GetItemById(Id);
    public string ItemName => Item != null ? Item.itemName : string.Empty;

    public int Id;
    public int ScrapValue;
    public int SaveData;

    public ShipInventoryItemData()
    {

    }

    public ShipInventoryItemData(int id, int scrapValue, int saveData)
    {
        Id = id;
        ScrapValue = scrapValue;
        SaveData = saveData;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ItemData GetItemData()
    {
        return new ItemData
        {
            ID = Id,
            SCRAP_VALUE = ScrapValue,
            SAVE_DATA = SaveData
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref ScrapValue);
        serializer.SerializeValue(ref SaveData);
    }
}
