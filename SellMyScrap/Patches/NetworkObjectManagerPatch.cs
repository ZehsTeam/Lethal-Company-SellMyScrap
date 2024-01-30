using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class NetworkObjectManagerPatch
    {
        public static GameObject networkPrefab;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start()
        {
            if (networkPrefab != null) return;

            var dllFolderPath = System.IO.Path.GetDirectoryName(SellMyScrapBase.Instance.Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "sellmyscrap_assets");
            AssetBundle MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            networkPrefab = (GameObject)MainAssetBundle.LoadAsset("NetworkHandler");
            networkPrefab.AddComponent<MainNetworkBehaviour>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }
    }
}
