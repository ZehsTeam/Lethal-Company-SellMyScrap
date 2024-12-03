using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ScrapEaterBehaviour : NetworkBehaviour
{
    [Header("Scrap Eater")]
    [Space(5f)]
    public GameObject modelObject;

    [HideInInspector]
    public List<GrabbableObject> targetScrap = [];
    [HideInInspector]
    public int TargetVariantIndex = -1;

    private int _clientsReceivedTargetScrap;
    private int _clientsFinishedAnimation;

    protected virtual void Start()
    {
        modelObject.SetActive(false);

        if (NetworkUtils.IsServer)
        {
            if (GameNetworkManager.Instance.connectedPlayers > 1)
            {
                SetTargetScrapClientRpc(NetworkUtils.GetNetworkObjectReferences(targetScrap));
            }
            else
            {
                StartCoroutine(StartEventOnLocalClient());
            }
        }
    }

    public void SetData(List<GrabbableObject> targetScrap, int variantIndex = -1)
    {
        this.targetScrap = targetScrap;
        this.targetScrap.ForEach(item => item.grabbable = false);

        TargetVariantIndex = variantIndex;
    }

    [ClientRpc]
    protected void SetTargetScrapClientRpc(NetworkObjectReference[] networkObjectReferences)
    {
        if (NetworkUtils.IsServer)
        {
            ReceivedTargetScrapServerRpc();
            return;
        }

        targetScrap = NetworkUtils.GetGrabbableObjects(networkObjectReferences);
        targetScrap.ForEach(item => item.grabbable = false);

        ReceivedTargetScrapServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ReceivedTargetScrapServerRpc()
    {
        _clientsReceivedTargetScrap++;

        if (_clientsReceivedTargetScrap >= GameNetworkManager.Instance.connectedPlayers)
        {
            _clientsReceivedTargetScrap = 0;
            OnAllClientsReceivedTargetScrap();
        }
    }

    /// <summary>
    /// Only gets called on the Host/Server
    /// </summary>
    protected virtual void OnAllClientsReceivedTargetScrap()
    {
        StartEventClientRpc();
    }

    [ClientRpc]
    protected virtual void StartEventClientRpc()
    {
        StartCoroutine(StartEventOnLocalClient());
    }

    protected virtual IEnumerator StartEventOnLocalClient()
    {
        modelObject.SetActive(true);
        yield return StartCoroutine(StartAnimation());
        modelObject.SetActive(false);

        AddTargetScrapToDepositItemsDesk();
        AnimationFinishedServerRpc();
    }

    /// <summary>
    /// Do your animation in here.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator StartAnimation()
    {
        // Do your animation in here.

        yield break;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void AnimationFinishedServerRpc()
    {
        _clientsFinishedAnimation++;

        if (_clientsFinishedAnimation >= GameNetworkManager.Instance.connectedPlayers)
        {
            _clientsFinishedAnimation = 0;
            OnAllClientsFinishedAnimation();
        }
    }

    /// <summary>
    /// Only gets called on the Host/Server
    /// </summary>
    protected virtual void OnAllClientsFinishedAnimation()
    {
        SellTargetScrapOnServer();
        GetComponent<NetworkObject>().Despawn();
    }

    protected void AddTargetScrapToDepositItemsDesk()
    {
        if (targetScrap.Count == 0) return;

        DepositItemsDeskPatch.PlaceItemsOnCounter(targetScrap);
        targetScrap.Clear();
    }

    protected void SellTargetScrapOnServer()
    {
        if (!NetworkUtils.IsServer) return;

        DepositItemsDeskPatch.SellItemsOnServer();
    }

    protected Transform GetHangarShipTransform()
    {
        return ScrapHelper.HangarShipTransform;
    }
}
