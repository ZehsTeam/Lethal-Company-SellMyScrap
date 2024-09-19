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

    public List<GrabbableObject> GrabbableObjects => [.. ShipGrabbableObjects, .. VehicleGrabbableObjects];

    public List<GrabbableObject> ShipGrabbableObjects
    {
        get
        {
            if (!_setShipGrabbableObjects)
            {
                SetShipGrabbableObjects();
            }

            return _shipGrabbableObjects;
        }
        set
        {
            _shipGrabbableObjects = value;
            _setShipGrabbableObjects = true;
        }
    }

    public List<GrabbableObject> VehicleGrabbableObjects
    {
        get
        {
            if (!_setVehicleGrabbableObjects)
            {
                SetVehicleGrabbableObjects();
            }

            return _vehicleGrabbableObjects;
        }
        set
        {
            _vehicleGrabbableObjects = value;
            _setVehicleGrabbableObjects = true;
        }
    }

    public int ItemCount => GetItemCount();
    public int TotalScrapValue => GetTotalScrapValue();
    public int RealTotalScrapValue => ScrapHelper.GetRealValue(TotalScrapValue);

    public NetworkObjectReference[] ShipNetworkObjectReferences = [];
    public NetworkObjectReference[] VehicleNetworkObjectReferences = [];
    public ShipInventoryItemData[] ShipInventoryItems = [];

    private List<ItemData> _itemDataList = [];
    private bool _setItemDataList;
    private List<GrabbableObject> _shipGrabbableObjects = [];
    private bool _setShipGrabbableObjects;
    private List<GrabbableObject> _vehicleGrabbableObjects = [];
    private bool _setVehicleGrabbableObjects;

    public ScrapToSell()
    {

    }

    public ScrapToSell(List<ItemData> items)
    {
        ItemDataList = items;
        ShipGrabbableObjects = items.Where(x => x.GrabbableObject != null && x.ItemLocation == ItemLocation.Ship).Select(x => x.GrabbableObject).ToList();
        VehicleGrabbableObjects = items.Where(x => x.GrabbableObject != null && x.ItemLocation == ItemLocation.Vehicle).Select(x => x.GrabbableObject).ToList();
        ShipInventoryItems = items.Where(x => x.ShipInventoryItemData != null).Select(x => x.ShipInventoryItemData).ToArray();

        SetShipNetworkObjectReferences();
        SetVehicleNetworkObjectReferences();
    }

    private void SetShipNetworkObjectReferences()
    {
        List<NetworkObjectReference> networkObjectReferences = [];

        foreach (var grabbableObject in ShipGrabbableObjects)
        {
            if (grabbableObject.TryGetComponent(out NetworkObject networkObject))
            {
                networkObjectReferences.Add(networkObject);
            }
        }

        ShipNetworkObjectReferences = networkObjectReferences.ToArray();
    }

    private void SetVehicleNetworkObjectReferences()
    {
        List<NetworkObjectReference> networkObjectReferences = [];

        foreach (var grabbableObject in VehicleGrabbableObjects)
        {
            if (grabbableObject.TryGetComponent(out NetworkObject networkObject))
            {
                networkObjectReferences.Add(networkObject);
            }
        }

        VehicleNetworkObjectReferences = networkObjectReferences.ToArray();
    }

    private void SetItemDataList()
    {
        ItemDataList = ScrapHelper.GetItemDataList(ShipGrabbableObjects, VehicleGrabbableObjects, ShipInventoryItems);
    }

    private void SetShipGrabbableObjects()
    {
        ShipGrabbableObjects = [];

        foreach (var networkObjectReference in ShipNetworkObjectReferences)
        {
            if (!networkObjectReference.TryGet(out NetworkObject networkObject)) continue;

            if (networkObject.TryGetComponent(out GrabbableObject grabbableObject))
            {
                ShipGrabbableObjects.Add(grabbableObject);
            }
        }
    }

    private void SetVehicleGrabbableObjects()
    {
        VehicleGrabbableObjects = [];

        foreach (var networkObjectReference in VehicleNetworkObjectReferences)
        {
            if (!networkObjectReference.TryGet(out NetworkObject networkObject)) continue;

            if (networkObject.TryGetComponent(out GrabbableObject grabbableObject))
            {
                VehicleGrabbableObjects.Add(grabbableObject);
            }
        }
    }

    private int GetItemCount()
    {
        return GrabbableObjects.Count + ShipInventoryItems.Length;
    }

    private int GetTotalScrapValue()
    {
        return GrabbableObjects.Sum(x => x.scrapValue) + ShipInventoryItems.Sum(x => x.ScrapValue);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ShipNetworkObjectReferences);
        serializer.SerializeValue(ref VehicleNetworkObjectReferences);
        serializer.SerializeValue(ref ShipInventoryItems);
    }
}
