using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.IO;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal static class Content
{
    // Network Handler
    public static GameObject NetworkHandlerPrefab { get; private set; }

    // Scrap Eaters
    public static GameObject OctolarScrapEaterPrefab { get; private set; }
    public static GameObject TakeyScrapEaterPrefab { get; private set; }
    public static GameObject MaxwellScrapEaterPrefab { get; private set; }
    public static GameObject YippeeScrapEaterPrefab { get; private set; }
    public static GameObject CookieFumoScrapEaterPrefab { get; private set; }
    public static GameObject PsychoScrapEaterPrefab { get; private set; }
    public static GameObject ZombiesScrapEaterPrefab { get; private set; }

    public static void Load()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        AssetBundle assetBundle = LoadAssetBundle("sellmyscrap_assets");
        if (assetBundle == null) return;

        // Network Handler
        NetworkHandlerPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "NetworkHandler");
        NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

        // Scrap Eaters
        OctolarScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "OctolarScrapEater");
        TakeyScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "TakeyScrapEater");
        MaxwellScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "MaxwellScrapEater");
        YippeeScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "YippeeScrapEater");
        CookieFumoScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "CookieFumoScrapEater");
        PsychoScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "PsychoScrapEater");
        ZombiesScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>(assetBundle, "ZombiesScrapEater");

        Plugin.logger.LogInfo("Successfully loaded assets from AssetBundle!");
    }

    private static AssetBundle LoadAssetBundle(string fileName)
    {
        try
        {
            var dllFolderPath = Path.GetDirectoryName(Plugin.Instance.Info.Location);
            var assetBundleFilePath = Path.Combine(dllFolderPath, fileName);
            return AssetBundle.LoadFromFile(assetBundleFilePath);
        }
        catch (System.Exception e)
        {
            Plugin.logger.LogError($"Failed to load AssetBundle \"{fileName}\". {e}");
        }

        return null;
    }

    private static T LoadAssetFromAssetBundle<T>(AssetBundle assetBundle, string name) where T : Object
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Plugin.logger.LogError($"Failed to load asset from AssetBundle. Name is null or whitespace.");
            return null;
        }

        if (assetBundle == null)
        {
            Plugin.logger.LogError($"Failed to load asset \"{name}\" from AssetBundle. AssetBundle is null.");
            return null;
        }

        T asset = assetBundle.LoadAsset<T>(name);

        if (asset == null)
        {
            Plugin.logger.LogError($"Failed to load asset \"{name}\" from AssetBundle. Asset is null.");
            return null;
        }

        return asset;
    }
}
