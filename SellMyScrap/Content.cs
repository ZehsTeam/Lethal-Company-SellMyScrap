using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class Content
{
    // NetworkHandler
    public static GameObject networkHandlerPrefab;

    // Scrap Eaters
    public static GameObject octolarScrapEaterPrefab;
    public static GameObject takeyScrapEaterPrefab;
    public static GameObject maxwellScrapEaterPrefab;
    public static GameObject yippeeScrapEaterPrefab;
    public static GameObject cookieFumoScrapEaterPrefab;

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

            // NetworkHandler
            networkHandlerPrefab = assetBundle.LoadAsset<GameObject>("NetworkHandler");
            networkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

            // Scrap Eaters
            octolarScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("OctolarScrapEater");
            takeyScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("TakeyScrapEater");
            maxwellScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("MaxwellScrapEater");
            yippeeScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("YippeeScrapEater");
            cookieFumoScrapEaterPrefab = assetBundle.LoadAsset<GameObject>("CookieFumoScrapEater");

            Plugin.logger.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            Plugin.logger.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
