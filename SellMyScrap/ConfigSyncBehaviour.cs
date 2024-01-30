using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap
{
    internal class ConfigSyncBehaviour : NetworkBehaviour
    {
        public static ConfigSyncBehaviour Instance;

        void Awake()
        {
            Instance = this;
        }

        [ClientRpc]
        public void SendConfigToPlayerClientRpc(SyncedConfigData syncedConfigData, ClientRpcParams clientRpcParams = default)
        {
            if (NetworkManager.Singleton.IsServer) return;

            SellMyScrapBase.mls.LogInfo("Syncing config with host.");

            SellMyScrapBase.Instance.ConfigManager.RebindConfigs(syncedConfigData);
        }
    }
}
