using Newtonsoft.Json;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    // Sell Settings
    public bool sellGifts;
    public bool sellShotguns;
    public bool sellAmmo;
    public bool sellKnives;
    public bool sellPickles;

    // Advanced Sell Settings
    public bool sellScrapWorthZero;
    public bool onlySellScrapOnFloor;
    public string dontSellListJson;
    public string sellListJson;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfigManager configManager)
    {
        // Sell Settings
        sellGifts = configManager.SellGifts;
        sellShotguns = configManager.SellShotguns;
        sellAmmo = configManager.SellAmmo;
        sellKnives = configManager.SellKnives;
        sellPickles = configManager.SellPickles;

        // Advanced Sell Settings
        sellScrapWorthZero = configManager.SellScrapWorthZero;
        onlySellScrapOnFloor = configManager.OnlySellScrapOnFloor;
        dontSellListJson = JsonConvert.SerializeObject(configManager.DontSellListJson);
        sellListJson = JsonConvert.SerializeObject(configManager.SellListJson);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Sell Settings
        serializer.SerializeValue(ref sellGifts);
        serializer.SerializeValue(ref sellShotguns);
        serializer.SerializeValue(ref sellAmmo);
        serializer.SerializeValue(ref sellKnives);
        serializer.SerializeValue(ref sellPickles);

        // Advanced Sell Settings
        serializer.SerializeValue(ref sellScrapWorthZero);
        serializer.SerializeValue(ref onlySellScrapOnFloor);
        serializer.SerializeValue(ref dontSellListJson);
        serializer.SerializeValue(ref sellListJson);
    }
}
