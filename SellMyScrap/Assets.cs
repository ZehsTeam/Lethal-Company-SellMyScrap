using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class Assets
{
    // NetworkHandler
    public static GameObject networkHandlerPrefab;

    // Octolar
    public static GameObject octolarPrefab;
    public static Material octolarNormalMaterial;
    public static Material octolarSuckMaterial;
    public static Material octolarSusMaterial;
    public static AudioClip squidwardWalkSound;
    public static AudioClip minecraftEatSound;

    public static void Initialize()
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

            // Octolar
            octolarPrefab = MainAssetBundle.LoadAsset<GameObject>("Octolar");
            AddScanNodePropertiesOnOctolar();
            octolarNormalMaterial = MainAssetBundle.LoadAsset<Material>("OctolarNormalMaterial");
            octolarSuckMaterial = MainAssetBundle.LoadAsset<Material>("OctolarSuckMaterial");
            octolarSusMaterial = MainAssetBundle.LoadAsset<Material>("OctolarSusMaterial");
            squidwardWalkSound = MainAssetBundle.LoadAsset<AudioClip>("SquidwardWalkSound");
            minecraftEatSound = MainAssetBundle.LoadAsset<AudioClip>("MinecraftEatSound");

            SellMyScrapBase.mls.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }

    private static void AddScanNodePropertiesOnOctolar()
    {
        GameObject scanNode = octolarPrefab.transform.GetChild(2).gameObject;

        ScanNodeProperties scanNodeProperties = scanNode.AddComponent<ScanNodeProperties>();
        scanNodeProperties.headerText = "Octolar";
        scanNodeProperties.minRange = 1;
        scanNodeProperties.maxRange = 13;
        scanNodeProperties.nodeType = 1;
    }
}
