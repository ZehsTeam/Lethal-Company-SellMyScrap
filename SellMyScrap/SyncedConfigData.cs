using Newtonsoft.Json;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

[Serializable]
public class SyncedConfigData : INetworkSerializable
{
    public bool sellGifts;
    public bool sellShotguns;
    public bool sellAmmo;
    public bool sellPickles;
    public bool sellScrapWorthZero;
    public bool onlySellScrapOnFloor;
    public string dontSellListJson;

    public SyncedConfigData() { }

    public SyncedConfigData(SyncedConfig config)
    {
        sellGifts = config.SellGifts;
        sellShotguns = config.SellShotguns;
        sellAmmo = config.SellAmmo;
        sellPickles = config.SellPickles;
        sellScrapWorthZero = config.SellScrapWorthZero;
        onlySellScrapOnFloor = config.OnlySellScrapOnFloor;
        dontSellListJson = JsonConvert.SerializeObject(config.DontSellListJson);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref sellGifts);
        serializer.SerializeValue(ref sellShotguns);
        serializer.SerializeValue(ref sellAmmo);
        serializer.SerializeValue(ref sellPickles);
        serializer.SerializeValue(ref sellScrapWorthZero);
        serializer.SerializeValue(ref onlySellScrapOnFloor);
        serializer.SerializeValue(ref dontSellListJson);
    }
}
