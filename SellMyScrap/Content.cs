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
            var dllFolderPath = System.IO.Path.GetDirectoryName(SellMyScrapBase.Instance.Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "sellmyscrap_assets");
            AssetBundle AssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // NetworkHandler
            networkHandlerPrefab = AssetBundle.LoadAsset<GameObject>("NetworkHandler");
            networkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

            // Scrap Eaters
            octolarScrapEaterPrefab = AssetBundle.LoadAsset<GameObject>("OctolarScrapEater");
            takeyScrapEaterPrefab = AssetBundle.LoadAsset<GameObject>("TakeyScrapEater");
            maxwellScrapEaterPrefab = AssetBundle.LoadAsset<GameObject>("MaxwellScrapEater");
            yippeeScrapEaterPrefab = AssetBundle.LoadAsset<GameObject>("YippeeScrapEater");
            cookieFumoScrapEaterPrefab = AssetBundle.LoadAsset<GameObject>("CookieFumoScrapEater");

            SellMyScrapBase.mls.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
