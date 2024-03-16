using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class Content
{
    // NetworkHandler
    public static GameObject networkHandlerPrefab;

    // SFX
    public static AudioClip squidwardWalkSFX;
    public static AudioClip stoneSlideSFX;
    public static AudioClip minecraftEatSFX;

    // Octolar
    public static GameObject octolarPrefab;
    public static Material octolarNormalMaterial;
    public static Material octolarSuckMaterial;
    public static Material octolarSusMaterial;

    // Takey
    public static GameObject takeyPrefab;
    public static Material takeyNormalMaterial;

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

            // SFX
            squidwardWalkSFX = MainAssetBundle.LoadAsset<AudioClip>("SquidwardWalkSFX");
            stoneSlideSFX = MainAssetBundle.LoadAsset<AudioClip>("StoneSlideSFX");
            minecraftEatSFX = MainAssetBundle.LoadAsset<AudioClip>("MinecraftEatSFX");

            // Octolar
            octolarPrefab = MainAssetBundle.LoadAsset<GameObject>("Octolar");
            octolarNormalMaterial = MainAssetBundle.LoadAsset<Material>("OctolarNormalMaterial");
            octolarSuckMaterial = MainAssetBundle.LoadAsset<Material>("OctolarSuckMaterial");
            octolarSusMaterial = MainAssetBundle.LoadAsset<Material>("OctolarSusMaterial");

            // Takey
            takeyPrefab = MainAssetBundle.LoadAsset<GameObject>("Takey");
            takeyNormalMaterial = MainAssetBundle.LoadAsset<Material>("TakeyNormalMaterial");

            SellMyScrapBase.mls.LogInfo("Successfully loaded assets from AssetBundle!");
        }
        catch (System.Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to load assets from AssetBundle.\n\n{e}");
        }
    }
}
