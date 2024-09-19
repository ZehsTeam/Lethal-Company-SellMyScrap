using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Serializable]
public class ScrapToSell : INetworkSerializable
{
    public List<ItemData> ItemDataList
    {
        get
        {
            if (!_setItemDataList)
            {
                SetItemDataList();
            }

            return _itemDataList;
        }
        set
        {
            _itemDataList = value;
            _setItemDataList = true;
        }
    }

    public List<GrabbableObject> GrabbableObjects
    {
        get
        {
            if (!_setGrabbableObjects)
            {
                SetGrabbableObjects();
            }

            return _grabbableObjects;
        }
        set
        {
            _grabbableObjects = value;
            _setGrabbableObjects = true;
        }
    }

    public int ItemCount => GetItemCount();
    public int TotalScrapValue => GetTotalScrapValue();
    public int RealTotalScrapValue => ScrapHelper.GetRealValue(TotalScrapValue);

    public NetworkObjectReference[] NetworkObjectReferences = [];
    public ItemDataProxy[] ItemDataProxies = [];

    private List<ItemData> _itemDataList;
    private bool _setItemDataList;
    private List<GrabbableObject> _grabbableObjects;
    private bool _setGrabbableObjects;

    public ScrapToSell()
    {

    }

    public ScrapToSell(List<ItemData> items)
    {
        ItemDataList = items;
        GrabbableObjects = items.Where(_ => _.GrabbableObject != null).Select(_ => _.GrabbableObject).ToList();
        ItemDataProxies = items.Where(_ => _.ItemDataProxy != null).Select(_ => _.ItemDataProxy).ToArray();

        SetNetworkObjectReferences();
    }

    private void SetNetworkObjectReferences()
    {
        List<NetworkObjectReference> networkObjectReferences = [];

        foreach (var grabbableObject in GrabbableObjects)
        {
            if (grabbableObject.TryGetComponent(out NetworkObject networkObject))
            {
                networkObjectReferences.Add(networkObject);
            }
        }

        NetworkObjectReferences = networkObjectReferences.ToArray();
    }

    private void SetItemDataList()
    {
        ItemDataList = ScrapHelper.GetItemDataList(GrabbableObjects, ItemDataProxies);
    }

    private void SetGrabbableObjects()
    {
        GrabbableObjects = [];

        foreach (var networkObjectReference in NetworkObjectReferences)
        {
            if (!networkObjectReference.TryGet(out NetworkObject networkObject)) continue;

            if (networkObject.TryGetComponent(out GrabbableObject grabbableObject))
            {
                GrabbableObjects.Add(grabbableObject);
            }
        }
    }

    private int GetItemCount()
    {
        return GrabbableObjects.Count + ItemDataProxies.Length;
    }

    private int GetTotalScrapValue()
    {
        return GrabbableObjects.Sum(_ => _.scrapValue) + ItemDataProxies.Sum(_ => _.ScrapValue);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NetworkObjectReferences);
        serializer.SerializeValue(ref ItemDataProxies);
    }
}
