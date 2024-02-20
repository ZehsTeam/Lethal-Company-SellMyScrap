using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class NetworkObjectManagerPatch
{
    public static GameObject networkPrefab;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void StartPatch()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        if (networkPrefab != null) return;

        try
        {
            var dllFolderPath = System.IO.Path.GetDirectoryName(SellMyScrapBase.Instance.Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "sellmyscrap_assets");
            AssetBundle MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            networkPrefab = MainAssetBundle.LoadAsset<GameObject>("NetworkHandler");
            networkPrefab.AddComponent<PluginNetworkBehaviour>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);

            SellMyScrapBase.mls.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
