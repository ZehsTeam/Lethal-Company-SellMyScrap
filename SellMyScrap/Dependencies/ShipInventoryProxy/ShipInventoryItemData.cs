using ShipInventory.Helpers;
using ShipInventory.Objects;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[System.Serializable]
public class ShipInventoryItemData : INetworkSerializable
{
    public Item Item => GetItem();
    public string ItemName => Item != null ? Item.itemName : string.Empty;

    public int Id;
    public int ScrapValue;
    public int SaveData;
    public bool PersistedThroughRounds;

    public ShipInventoryItemData()
    {

    }

    public ShipInventoryItemData(int id, int scrapValue, int saveData, bool persistedThroughRounds)
    {
        Id = id;
        ScrapValue = scrapValue;
        SaveData = saveData;
        PersistedThroughRounds = persistedThroughRounds;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ItemData GetItemData()
    {
        return new ItemData
        {
            ID = Id,
            SCRAP_VALUE = ScrapValue,
            SAVE_DATA = SaveData,
            PERSISTED_THROUGH_ROUNDS = PersistedThroughRounds
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public Item GetItem()
    {
        return ItemManager.ALLOWED_ITEMS.GetValueOrDefault(Id);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref ScrapValue);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref PersistedThroughRounds);
    }
}
