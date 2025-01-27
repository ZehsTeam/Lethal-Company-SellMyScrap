using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class DepositItemsDeskHelper
{
    public static DepositItemsDesk Instance { get; private set; }

    public static void SetInstance(DepositItemsDesk instance)
    {
        Instance = instance;
    }

    public static void SellItems_Server()
    {
        if (Instance == null) return;

        DepositItemsDeskPatch.SpeakInShip = true;

        Instance.SellItemsOnServer();
    }

    public static void PlaceItemsOnCounter(List<GrabbableObject> grabbableObjects)
    {
        if (Instance == null) return;

        grabbableObjects.ForEach(PlaceItemOnCounter);
    }

    public static void PlaceItemOnCounter(GrabbableObject grabbableObject)
    {
        if (Instance == null || grabbableObject == null)
        {
            return;
        }

        if (Instance.itemsOnCounter.Contains(grabbableObject))
        {
            return;
        }

        Instance.itemsOnCounter.Add(grabbableObject);

        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        Instance.itemsOnCounterNetworkObjects.Add(networkObject);

        grabbableObject.EnablePhysics(false);
        grabbableObject.EnableItemMeshes(false);

        grabbableObject.transform.SetParent(Instance.deskObjectsContainer.transform);
        grabbableObject.transform.localPosition = Vector3.zero;
    }

    public static void PlaceRagdollOnCounter(RagdollGrabbableObject ragdollGrabbableObject)
    {
        if (Instance == null || ragdollGrabbableObject == null)
        {
            return;
        }

        if (Instance.itemsOnCounter.Contains(ragdollGrabbableObject))
        {
            return;
        }

        Instance.itemsOnCounter.Add(ragdollGrabbableObject);

        NetworkObject networkObject = ragdollGrabbableObject.gameObject.GetComponent<NetworkObject>();
        Instance.itemsOnCounterNetworkObjects.Add(networkObject);

        ragdollGrabbableObject.EnablePhysics(false);
        ragdollGrabbableObject.EnableItemMeshes(false);

        Transform ragdollTransform = ragdollGrabbableObject.ragdoll.transform;

        foreach (var renderer in ragdollTransform.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }

        foreach (var renderer in ragdollTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.enabled = false;
        }

        ragdollTransform.SetParent(Instance.deskObjectsContainer.transform);
        ragdollTransform.localPosition = Vector3.zero;
    }
}
