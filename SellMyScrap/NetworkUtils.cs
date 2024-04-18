using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

public class NetworkUtils
{
    public static string ConvertIntListToString(List<int> list)
    {
        return string.Join(',', list);
    }

    public static List<int> CovertStringToIntList(string list)
    {
        return list.Split(',').Select(int.Parse).ToList();
    }

    public static string GetNetworkObjectIdsString(List<GrabbableObject> grabbableObjects)
    {
        return ConvertIntListToString(GetNetworkObjectIds(grabbableObjects));
    }

    public static List<int> GetNetworkObjectIds(List<GrabbableObject> grabbableObjects)
    {
        List<int> intList = new List<int>();

        grabbableObjects.ForEach(grabbableObject =>
        {
            int networkObjectId = GetNetworkObjectId(grabbableObject);
            if (networkObjectId == -1) return;

            intList.Add(networkObjectId);
        });

        return intList;
    }

    public static int GetNetworkObjectId(GrabbableObject grabbableObject)
    {
        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        return networkObject is null ? -1 : (int)networkObject.NetworkObjectId;
    }

    public static List<GrabbableObject> GetGrabbableObjects(string networkObjectIds)
    {
        return GetGrabbableObjects(CovertStringToIntList(networkObjectIds));
    }

    public static List<GrabbableObject> GetGrabbableObjects(List<int> networkObjectIds)
    {
        List<GrabbableObject> grabbableObjects = new List<GrabbableObject>();

        networkObjectIds.ForEach(networkObjectId =>
        {
            GrabbableObject grabbableObject = GetGrabbableObject(networkObjectId);
            if (grabbableObject is null) return;

            grabbableObjects.Add(grabbableObject);
        });

        return grabbableObjects;
    }

    public static GrabbableObject GetGrabbableObject(int networkObjectId)
    {
        NetworkObject networkObject = GetNetworkObject(networkObjectId);
        if (networkObject is null) return null;

        return networkObject.gameObject.GetComponent<GrabbableObject>();
    }

    public static NetworkObject GetNetworkObject(int networkObjectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue((ulong)networkObjectId, out NetworkObject networkObject);
        return networkObject;
    }
}
