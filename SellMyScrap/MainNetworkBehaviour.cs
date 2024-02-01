using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap;

internal class MainNetworkBehaviour : NetworkBehaviour
{
    public static MainNetworkBehaviour Instance;

    void Awake()
    {
        Instance = this;
    }

    [ClientRpc]
    public void SendConfigToPlayerClientRpc(SyncedConfigData syncedConfigData, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.IsServer) return;

        SellMyScrapBase.mls.LogInfo("Syncing config with host.");

        SellMyScrapBase.Instance.ConfigManager.SetHostConfigData(syncedConfigData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSellServerRpc(int amount, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        SellMyScrapBase.mls.LogInfo($"Player {clientId} has requested to sell ${amount}");

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllowedScrapToSell(amount);
        SellMyScrapBase.Instance.CreateSellRequest(SellType.None, scrapToSell.value, amount, ConfirmationType.AwaitingConfirmation);
        SellMyScrapBase.Instance.ConfirmSellRequest();
    }
}
