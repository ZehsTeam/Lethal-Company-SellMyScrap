﻿using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal static class Content
{
    // Network Handler
    public static GameObject NetworkHandlerPrefab;

    // Scrap Eaters
    public static GameObject OctolarScrapEaterPrefab;
    public static GameObject TakeyScrapEaterPrefab;
    public static GameObject MaxwellScrapEaterPrefab;
    public static GameObject YippeeScrapEaterPrefab;
    public static GameObject CookieFumoScrapEaterPrefab;
    public static GameObject PsychoScrapEaterPrefab;
    public static GameObject ZombiesScrapEaterPrefab;

    // Sprites
    public static Sprite ModIcon;

    public static void Load()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        try
        {
            var dllFolderPath = System.IO.Path.GetDirectoryName(Plugin.Instance.Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "sellmyscrap_assets");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // Network Handler
            NetworkHandlerPrefab = assetBundle.LoadAsset<GameObject>("NetworkHandler");
            NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

            // Scrap Eaters
            OctolarScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("OctolarScrapEater");
            TakeyScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("TakeyScrapEater");
            MaxwellScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("MaxwellScrapEater");
            YippeeScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("YippeeScrapEater");
            CookieFumoScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("CookieFumoScrapEater");
            PsychoScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("PsychoScrapEater");
            ZombiesScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("ZombiesScrapEater");

            // Sprites
            ModIcon = assetBundle.LoadAsset<Sprite>("ModIcon");

            Plugin.logger.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            Plugin.logger.LogError($"Failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
