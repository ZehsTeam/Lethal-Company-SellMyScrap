using ShipInventory.Objects;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[System.Serializable]
public class ShipInventoryItemData : INetworkSerializable
{
    public Item Item => ScrapHelper.GetItemByName(Id);
    public string ItemName => Item != null ? Item.itemName : string.Empty;

    public string Id;
    public int ScrapValue;
    public int SaveData;
    public bool PersistedThroughRounds;

    public ShipInventoryItemData()
    {

    }

    public ShipInventoryItemData(string id, int scrapValue, int saveData, bool persistedThroughRounds)
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref ScrapValue);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref PersistedThroughRounds);
    }
}
