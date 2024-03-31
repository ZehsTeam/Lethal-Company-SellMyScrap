using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class ScrapEaterBehaviour : NetworkBehaviour
{
    public bool IsHostOrServer => IsHost || IsServer;

    public GameObject modelObject = null;

    [HideInInspector]
    public List<GrabbableObject> targetScrap = new List<GrabbableObject>();

    private string targetScrapNetworkObjectIdsString;
    private int clientsReceivedTargetScrap = 0;
    private int clientsFinishedAnimation = 0;

    protected virtual void Start()
    {
        modelObject.SetActive(false);

        if (IsHostOrServer)
        {
            SetTargetScrapClientRpc(targetScrapNetworkObjectIdsString);
        }
    }

    public void SetTargetScrapNetworkObjectIdsString(string networkObjectIdsString)
    {
        targetScrapNetworkObjectIdsString = networkObjectIdsString;
    }

    [ClientRpc]
    public void SetTargetScrapClientRpc(string networkObjectIdsString)
    {
        targetScrap = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);

        targetScrap.ForEach(item =>
        {
            item.grabbable = false;
        });

        ReceivedTargetScrapServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ReceivedTargetScrapServerRpc()
    {
        clientsReceivedTargetScrap++;

        if (clientsReceivedTargetScrap >= GameNetworkManager.Instance.connectedPlayers)
        {
            clientsReceivedTargetScrap = 0;
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

    protected virtual IEnumerator StartAnimation()
    {
        // Do your animation in here.

        yield break;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void AnimationFinishedServerRpc()
    {
        clientsFinishedAnimation++;

        if (clientsFinishedAnimation >= GameNetworkManager.Instance.connectedPlayers)
        {
            clientsFinishedAnimation = 0;
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
        DepositItemsDeskPatch.SellItemsOnServer();
    }

    protected Transform GetHangarShipTransform()
    {
        return ScrapHelper.HangarShip.transform;
    }
}
