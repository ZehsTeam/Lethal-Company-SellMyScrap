using ShipInventoryUpdated.Helpers.API;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using SI_ItemData = ShipInventoryUpdated.Objects.ItemData;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Objects;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Serializable]
public struct SI_ItemDataProxy : INetworkSerializable, IEquatable<SI_ItemDataProxy>
{
    public FixedString32Bytes Id;
    public int ScrapValue;
    public int SaveData;
    public bool PersistedThroughRounds;

    public string ItemName => GetItem()?.itemName ?? "Unknown Item";
    public bool IsScrap => GetItem()?.isScrap ?? false;

    public SI_ItemDataProxy(FixedString32Bytes id, int scrapValue, int saveData, bool persistedThroughRounds)
    {
        Id = id;
        ScrapValue = scrapValue;
        SaveData = saveData;
        PersistedThroughRounds = persistedThroughRounds;
    }

    public Item? GetItem()
    {
        return ItemIdentifier.GetItem(Id.Value);
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Id.Value);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref ScrapValue);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref PersistedThroughRounds);
    }

    public bool Equals(SI_ItemDataProxy other)
    {
        if (Id != other.Id) return false;
        if (ScrapValue != other.ScrapValue) return false;
        if (SaveData != other.SaveData) return false;
        if (PersistedThroughRounds != other.PersistedThroughRounds) return false;

        return true;
    }



    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public SI_ItemDataProxy(SI_ItemData itemData)
    {
        try
        {
            Id = itemData.ID;
            ScrapValue = itemData.SCRAP_VALUE;
            SaveData = itemData.SAVE_DATA;
            PersistedThroughRounds = itemData.PERSISTED_THROUGH_ROUNDS;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to properly construct {GetType().Name}. {ex}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public bool Equals(SI_ItemData other)
    {
        try
        {
            if (Id != other.ID) return false;
            if (ScrapValue != other.SCRAP_VALUE) return false;
            if (SaveData != other.SAVE_DATA) return false;
            if (PersistedThroughRounds != other.PERSISTED_THROUGH_ROUNDS) return false;

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to compare {GetType().Name} to {nameof(SI_ItemData)}. {ex}");
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public SI_ItemData CreateItemData()
    {
        try
        {
            return new SI_ItemData
            {
                ID = Id,
                SCRAP_VALUE = ScrapValue,
                SAVE_DATA = SaveData,
                PERSISTED_THROUGH_ROUNDS = PersistedThroughRounds,
            };
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to convert {GetType().Name} to {nameof(SI_ItemData)}. {ex}");
            return default;
        }
    }
}
