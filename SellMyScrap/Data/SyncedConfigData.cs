﻿using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    // Sell Settings
    public bool SellGifts;
    public bool SellShotguns;
    public bool SellAmmo;
    public bool SellKnives;
    public bool SellPickles;

    // Advanced Sell Settings
    public bool SellScrapWorthZero;
    public bool OnlySellScrapOnFloor;
    public string PrioritySellList;
    public string DontSellList;
    public string SellList;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfigManager configManager)
    {
        // Sell Settings
        SellGifts = configManager.SellGifts;
        SellShotguns = configManager.SellShotguns;
        SellAmmo = configManager.SellAmmo;
        SellKnives = configManager.SellKnives;
        SellPickles = configManager.SellPickles;

        // Advanced Sell Settings
        SellScrapWorthZero = configManager.SellScrapWorthZero;
        OnlySellScrapOnFloor = configManager.OnlySellScrapOnFloor;
        PrioritySellList = string.Join(", ", configManager.PrioritySellList);
        DontSellList = string.Join(", ", configManager.DontSellList);
        SellList = string.Join(", ", configManager.SellList);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Sell Settings
        serializer.SerializeValue(ref SellGifts);
        serializer.SerializeValue(ref SellShotguns);
        serializer.SerializeValue(ref SellAmmo);
        serializer.SerializeValue(ref SellKnives);
        serializer.SerializeValue(ref SellPickles);

        // Advanced Sell Settings
        serializer.SerializeValue(ref SellScrapWorthZero);
        serializer.SerializeValue(ref OnlySellScrapOnFloor);
        serializer.SerializeValue(ref PrioritySellList);
        serializer.SerializeValue(ref DontSellList);
        serializer.SerializeValue(ref SellList);
    }
}
