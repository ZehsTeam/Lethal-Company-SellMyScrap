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
            AssetBundle MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // NetworkHandler
            networkHandlerPrefab = MainAssetBundle.LoadAsset<GameObject>("NetworkHandler");
            networkHandlerPrefab.AddComponent<PluginNetworkBehaviour>();

            // Scrap Eaters
            octolarScrapEaterPrefab = MainAssetBundle.LoadAsset<GameObject>("OctolarScrapEater");
            takeyScrapEaterPrefab = MainAssetBundle.LoadAsset<GameObject>("TakeyScrapEater");

            SellMyScrapBase.mls.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
