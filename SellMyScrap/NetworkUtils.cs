using System.Collections.Generic;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class NetworkUtils
{
    public static bool IsConnected
    {
        get
        {
            if (NetworkManager.Singleton == null)
            {
                return false;
            }

            return NetworkManager.Singleton.IsConnectedClient;
        }
    }

    public static bool IsServer
    {
        get
        {
            if (NetworkManager.Singleton == null)
            {
                return false;
            }

            return NetworkManager.Singleton.IsServer;
        }
    }

    public static bool IsHost
    {
        get
        {
            if (NetworkManager.Singleton == null)
            {
                return false;
            }

            return NetworkManager.Singleton.IsHost;
        }
    }

    public static ulong GetLocalClientId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public static bool IsLocalClientId(ulong clientId)
    {
        return clientId == GetLocalClientId();
    }

    public static NetworkObjectReference[] GetNetworkObjectReferences(List<GrabbableObject> grabbableObjects)
    {
        if (grabbableObjects == null || grabbableObjects.Count == 0) return [];

        List<NetworkObjectReference> networkObjectReferences = [];

        foreach (var grabbableObject in grabbableObjects)
        {
            if (grabbableObject.TryGetComponent(out NetworkObject networkObject))
            {
                networkObjectReferences.Add(networkObject);
            }
        }

        return networkObjectReferences.ToArray();
    }

    public static List<GrabbableObject> GetGrabbableObjects(NetworkObjectReference[] networkObjectReferences)
    {
        if (networkObjectReferences == null || networkObjectReferences.Length == 0) return [];

        List<GrabbableObject> grabbableObjects = [];

        foreach (var networkObjectReference in networkObjectReferences)
        {
            if (!networkObjectReference.TryGet(out NetworkObject networkObject)) continue;

            if (networkObject.TryGetComponent(out GrabbableObject grabbableObject))
            {
                grabbableObjects.Add(grabbableObject);
            }
        }

        return grabbableObjects;
    }
}
