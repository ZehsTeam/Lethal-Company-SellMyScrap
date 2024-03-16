using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

internal class OctolarScrapEater : ScrapEater
{
    public OctolarScrapEater()
    {
        spawnPrefab = Content.octolarPrefab;

        startPosition = new Vector3(-8.9f, 0f, -3.2f);
        endPosition = new Vector3(-8.9f, 0f, -6.8f);
        rotationOffset = new Vector3(0f, 90f, 0f);
        size = 4.5f;

        slideDuration = 3f;
        suckDuration = 3.5f;
        waitDuration = 2f;

        slideSFX = Content.squidwardWalkSFX;
        eatSFX = Content.minecraftEatSFX;

        defaultMaterial = Content.octolarNormalMaterial;
        slideMaterials = [Content.octolarNormalMaterial, Content.octolarSusMaterial];
        suckMaterial = Content.octolarSuckMaterial;
    }

    public override int GetSpawnWeight()
    {
        return SellMyScrapBase.Instance.ConfigManager.OctolarSpawnWeight;
    }
}
