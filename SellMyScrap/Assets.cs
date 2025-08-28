using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.SellMyScrap;

internal static class Assets
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
        string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string assetBundleFileName = "sellmyscrap_assets";
        string assetBundlePath = Path.Combine(pluginFolder, assetBundleFileName);

        if (!File.Exists(assetBundlePath))
        {
            Logger.LogFatal($"Failed to load assets. AssetBundle file could not be found at path \"{assetBundlePath}\". Make sure the \"{assetBundleFileName}\" file is in the same folder as the mod's DLL file.");
            return;
        }

        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);

        if (assetBundle == null)
        {
            Logger.LogFatal($"Failed to load assets. AssetBundle is null.");
            return;
        }

        HandleAssetBundleLoaded(assetBundle);
    }

    private static void HandleAssetBundleLoaded(AssetBundle assetBundle)
    {
        // Prefabs
        NetworkHandlerPrefab = LoadAsset<GameObject>("NetworkHandler", assetBundle);
        NetworkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();
        OctolarScrapEaterPrefab = LoadAsset<GameObject>("OctolarScrapEater", assetBundle);
        TakeyScrapEaterPrefab = LoadAsset<GameObject>("TakeyScrapEater", assetBundle);
        MaxwellScrapEaterPrefab = LoadAsset<GameObject>("MaxwellScrapEater", assetBundle);
        YippeeScrapEaterPrefab = LoadAsset<GameObject>("YippeeScrapEater", assetBundle);
        CookieFumoScrapEaterPrefab = LoadAsset<GameObject>("CookieFumoScrapEater", assetBundle);
        PsychoScrapEaterPrefab = LoadAsset<GameObject>("PsychoScrapEater", assetBundle);
        ZombiesScrapEaterPrefab = LoadAsset<GameObject>("ZombiesScrapEater", assetBundle);
        WolfyScrapEaterPrefab = LoadAsset<GameObject>("WolfyScrapEater", assetBundle);

        // AudioClips
        BrainRotIntroSpeechSFX = LoadAsset<AudioClip>("BrainRotIntroSpeechSFX", assetBundle);
    }

    private static T LoadAsset<T>(string name, AssetBundle assetBundle) where T : Object
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" from AssetBundle. Name is null or whitespace.");
            return null;
        }

        if (assetBundle == null)
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. AssetBundle is null.");
            return null;
        }

        T asset = assetBundle.LoadAsset<T>(name);

        if (asset == null)
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. No asset found with that type and name.");
            return null;
        }

        return asset;
    }

    private static bool TryLoadAsset<T>(string name, AssetBundle assetBundle, out T asset) where T : Object
    {
        asset = LoadAsset<T>(name, assetBundle);
        return asset != null;
    }
}
