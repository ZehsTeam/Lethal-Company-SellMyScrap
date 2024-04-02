using Newtonsoft.Json;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    // Sell Settings
    public bool sellGifts;
    public bool sellShotguns;
    public bool sellAmmo;
    public bool sellKnife;
    public bool sellPickles;

    // Advanced Sell Settings
    public bool sellScrapWorthZero;
    public bool onlySellScrapOnFloor;
    public string dontSellListJson;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfig config)
    {
        // Sell Settings
        sellGifts = config.SellGifts;
        sellShotguns = config.SellShotguns;
        sellAmmo = config.SellAmmo;
        sellKnife = config.SellKnife;
        sellPickles = config.SellPickles;

        // Advanced Sell Settings
        sellScrapWorthZero = config.SellScrapWorthZero;
        onlySellScrapOnFloor = config.OnlySellScrapOnFloor;
        dontSellListJson = JsonConvert.SerializeObject(config.DontSellListJson);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Sell Settings
        serializer.SerializeValue(ref sellGifts);
        serializer.SerializeValue(ref sellShotguns);
        serializer.SerializeValue(ref sellAmmo);
        serializer.SerializeValue(ref sellKnife);
        serializer.SerializeValue(ref sellPickles);

        // Advanced Sell Settings
        serializer.SerializeValue(ref sellScrapWorthZero);
        serializer.SerializeValue(ref onlySellScrapOnFloor);
        serializer.SerializeValue(ref dontSellListJson);
    }
}
