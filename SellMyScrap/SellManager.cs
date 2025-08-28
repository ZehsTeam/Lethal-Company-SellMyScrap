using com.github.zehsteam.SellMyScrap.Data;
using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using com.github.zehsteam.SellMyScrap.Objects;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal static class SellManager
{
    public static ScrapToSell ScrapToSell { get; private set; }
    public static SellRequest SellRequest { get; private set; }

    public static ScrapToSell GetScrapToSell(SellCommandRequest sellRequest)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(sellRequest);
        return ScrapToSell;
    }

    public static ScrapToSell GetScrapToSell(string[] sellList, bool onlyUseShipInventory = false)
    {
        ScrapToSell = ScrapHelper.GetScrapToSell(sellList);
        return ScrapToSell;
    }

    public static ScrapToSell SetScrapToSell(List<ItemData> items)
    {
        ScrapToSell = new ScrapToSell(items);
        return ScrapToSell;
    }

    #region SellRequest Methods
    public static void CreateSellRequest(SellType sellType, int value, int requestedValue, ConfirmationStatus confirmationType, int scrapEaterIndex = -2, int scrapEaterVariantIndex = -1)
    {
        SellRequest = new SellRequest(sellType, value, requestedValue, confirmationType, scrapEaterIndex, scrapEaterVariantIndex);

        string message = $"Created sell request. {ScrapToSell.ItemCount} items for ${value}.";

        if (scrapEaterIndex >= 0)
        {
            message += $" (ScrapEaterIndex: {scrapEaterIndex}, ScrapEaterVariantIndex: {scrapEaterVariantIndex})";
        }

        Logger.LogInfo(message);
    }

    public static void ConfirmSellRequest()
    {
        if (ScrapToSell == null || SellRequest == null) return;

        SellRequest.ConfirmationStatus = ConfirmationStatus.Confirmed;

        Logger.LogInfo($"Attempting to sell {ScrapToSell.ItemCount} items for ${ScrapToSell.TotalScrapValue}.");

        if (NetworkUtils.IsServer)
        {
            ConfirmSellRequestOnServer();
        }
        else
        {
            ConfirmSellRequestOnClient();
        }

        SellRequest = null;
    }

    private static void ConfirmSellRequestOnServer()
    {
        StartOfRound.Instance.StartCoroutine(PerformSellOnServer());
    }

    private static void ConfirmSellRequestOnClient()
    {
        PluginNetworkBehaviour.Instance.PerformSellServerRpc(ScrapToSell, SellRequest.SellType, SellRequest.ScrapEaterIndex);
    }

    public static void CancelSellRequest()
    {
        SellRequest = null;
        ScrapToSell = null;
    }
    #endregion

    public static void PerformSellOnServerFromClient(ScrapToSell scrapToSell, SellType sellType, int scrapEaterIndex = -2, int scrapEaterVariantIndex = -1)
    {
        ScrapToSell = scrapToSell;
        CreateSellRequest(sellType, ScrapToSell.TotalScrapValue, ScrapToSell.TotalScrapValue, ConfirmationStatus.AwaitingConfirmation, scrapEaterIndex, scrapEaterVariantIndex);
        ConfirmSellRequest();
    }

    public static IEnumerator PerformSellOnServer()
    {
        if (ScrapToSell == null || SellRequest == null) yield return null;
        if (SellRequest.ConfirmationStatus != ConfirmationStatus.Confirmed) yield return null;

        if (DepositItemsDeskHelper.Instance == null)
        {
            Logger.LogError($"Could not find depositItemsDesk. Are you landed at The Company building?");
            yield break;
        }

        int scrapEaterIndex = SellRequest.ScrapEaterIndex;
        int scrapEaterVariantIndex = SellRequest.ScrapEaterVariantIndex;

        List<GrabbableObject> grabbableObjects = ScrapToSell.GrabbableObjects;

        if (ShipInventoryProxy.Enabled && ScrapToSell.ShipInventoryItems.Length > 0)
        {
            ShipInventoryProxy.SpawnItemsOnServer(ScrapToSell.ShipInventoryItems);

            yield return new WaitUntil(() => !ShipInventoryProxy.IsSpawning);

            if (ShipInventoryProxy.SpawnItemsStatus == SpawnItemsStatus.Success)
            {
                grabbableObjects.AddRange(ShipInventoryProxy.GetSpawnedGrabbableObjects());
                ShipInventoryProxy.ClearSpawnedGrabbableObjectsCache();
            }
            else if (ShipInventoryProxy.SpawnItemsStatus == SpawnItemsStatus.Failed)
            {
                HUDManager.Instance.DisplayTip("SellMyScrap", "Failed to spawn items from ShipInventory!", isWarning: true);
                yield break;
            }
            else if (ShipInventoryProxy.SpawnItemsStatus == SpawnItemsStatus.Busy)
            {
                HUDManager.Instance.DisplayTip("SellMyScrap", "Failed to spawn items from ShipInventory! Chute is busy.", isWarning: true);
                yield break;
            }
        }

        // Try to show a scrap eater if the ship is not leaving.
        if (!StartOfRound.Instance.shipIsLeaving)
        {
            if (scrapEaterIndex == -1)
            {
                ScrapEaterManager.StartRandomScrapEaterOnServer(grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }

            if (scrapEaterIndex > -1 && ScrapEaterManager.HasScrapEater(scrapEaterIndex))
            {
                ScrapEaterManager.StartScrapEaterOnServer(scrapEaterIndex, grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }

            if (ScrapEaterManager.CanUseScrapEater())
            {
                ScrapEaterManager.StartRandomScrapEaterOnServer(grabbableObjects, scrapEaterVariantIndex);
                yield break;
            }
        }

        DepositItemsDeskHelper.PlaceItemsOnCounter(grabbableObjects);
        PluginNetworkBehaviour.Instance.PlaceItemsOnCounterClientRpc(NetworkUtils.GetNetworkObjectReferences(grabbableObjects));
        yield return new WaitForSeconds(0.5f);
        DepositItemsDeskHelper.SellItems_Server();

        ScrapToSell = null;
    }
}
