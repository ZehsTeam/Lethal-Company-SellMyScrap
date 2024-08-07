﻿using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ScrapEaterBehaviour : NetworkBehaviour
{
    protected bool IsHostOrServer => IsHost || IsServer;

    [Header("Scrap Eater")]
    [Space(5f)]
    public GameObject modelObject = null;

    [HideInInspector]
    public List<GrabbableObject> targetScrap = [];

    private int _clientsReceivedTargetScrap = 0;
    private int _clientsFinishedAnimation = 0;

    protected virtual void Start()
    {
        modelObject.SetActive(false);

        if (IsHostOrServer)
        {
            if (GameNetworkManager.Instance.connectedPlayers > 1)
            {
                SetTargetScrapClientRpc(NetworkUtils.GetNetworkObjectIdsString(targetScrap));
            }
            else
            {
                StartCoroutine(StartEventOnLocalClient());
            }
        }
    }

    public void SetTargetScrapOnServer(List<GrabbableObject> targetScrap)
    {
        this.targetScrap = targetScrap;
        this.targetScrap.ForEach(item => item.grabbable = false);
    }

    [ClientRpc]
    protected void SetTargetScrapClientRpc(string networkObjectIdsString)
    {
        if (!IsHostOrServer)
        {
            targetScrap = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);
            targetScrap.ForEach(item => item.grabbable = false);
        }
        
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
        if (!IsHostOrServer) return;

        DepositItemsDeskPatch.SellItemsOnServer();
    }

    protected Transform GetHangarShipTransform()
    {
        return ScrapHelper.HangarShip.transform;
    }
}
