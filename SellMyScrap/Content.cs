using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.IO;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal static class Content
{
    // Prefabs
    public static GameObject NetworkHandlerPrefab { get; private set; }
    public static GameObject OctolarScrapEaterPrefab { get; private set; }
    public static GameObject TakeyScrapEaterPrefab { get; private set; }
    public static GameObject MaxwellScrapEaterPrefab { get; private set; }
    public static GameObject YippeeScrapEaterPrefab { get; private set; }
    public static GameObject CookieFumoScrapEaterPrefab { get; private set; }
    public static GameObject PsychoScrapEaterPrefab { get; private set; }
    public static GameObject ZombiesScrapEaterPrefab { get; private set; }
    public static GameObject WolfyScrapEaterPrefab { get; private set; }

    // AudioClips
    public static AudioClip BrainRotIntroSpeechSFX { get; private set; }

    public static void Load()
    {
        LoadAssetsFromAssetBundle();
    }

    private static void LoadAssetsFromAssetBundle()
    {
        AssetBundle assetBundle = LoadAssetBundle("sellmyscrap_assets");
        if (assetBundle == null) return;

        // Prefabs
        NetworkHandlerPrefab = LoadAssetFromAssetBundle<GameObject>("NetworkHandler", assetBundle);
        NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();
        OctolarScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("OctolarScrapEater", assetBundle);
        TakeyScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("TakeyScrapEater", assetBundle);
        MaxwellScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("MaxwellScrapEater", assetBundle);
        YippeeScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("YippeeScrapEater", assetBundle);
        CookieFumoScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("CookieFumoScrapEater", assetBundle);
        PsychoScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("PsychoScrapEater", assetBundle);
        ZombiesScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("ZombiesScrapEater", assetBundle);
        WolfyScrapEaterPrefab = LoadAssetFromAssetBundle<GameObject>("WolfyScrapEater", assetBundle);

        // AudioClips
        BrainRotIntroSpeechSFX = LoadAssetFromAssetBundle<AudioClip>("BrainRotIntroSpeechSFX", assetBundle);

        Plugin.Logger.LogInfo("Successfully loaded assets from AssetBundle!");
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
            Plugin.Logger.LogError($"Failed to load AssetBundle \"{fileName}\". {e}");
        }

        return null;
    }

    private static T LoadAssetFromAssetBundle<T>(string name, AssetBundle assetBundle) where T : Object
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" from AssetBundle. Name is null or whitespace.");
            return null;
        }

        if (assetBundle == null)
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. AssetBundle is null.");
            return null;
        }

        T asset = assetBundle.LoadAsset<T>(name);

        if (asset == null)
        {
            Plugin.Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. No asset found with that type and name.");
            return null;
        }

        return asset;
    }
}
