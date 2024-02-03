using com.github.zehsteam.SellMyScrap.Patches;
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
    public void RequestSellServerRpc(string username, int value, int amount, ServerRpcParams serverRpcParams = default)
    {
        string message = $"Player {username} has requested to sell {amount} items for a total of ${value}";
        SellMyScrapBase.mls.LogInfo(message);
        SellMyScrapBase.Instance.DisplayGlobalNotification(message);

        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetAllowedScrapToSell(value);
        SellMyScrapBase.Instance.CreateSellRequest(SellType.None, scrapToSell.value, value, ConfirmationType.AwaitingConfirmation);
        SellMyScrapBase.Instance.ConfirmSellRequest();
    }

    [ClientRpc]
    public void SoldFromTerminalClientRpc()
    {
        DepositItemsDeskPatch.speakInShip = true;
    }
}
