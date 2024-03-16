using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

internal class TakeyScrapEater : ScrapEater
{
    public TakeyScrapEater()
    {
        spawnPrefab = Content.takeyPrefab;

        startPosition = new Vector3(-8.9f, 0f, -3.2f);
        endPosition = new Vector3(-8.9f, 0f, -6.8f);
        rotationOffset = new Vector3(0f, 90f, 0f);
        size = 4f;

        slideDuration = 3f;
        suckDuration = 3.5f;
        waitDuration = 2f;

        slideSFX = Content.stoneSlideSFX;
        eatSFX = Content.minecraftEatSFX;

        defaultMaterial = Content.takeyNormalMaterial;
        //slideMaterials = [Content., Content.];
        //suckMaterial = Content.;
    }

    public override int GetSpawnWeight()
    {
        return SellMyScrapBase.Instance.ConfigManager.TakeySpawnWeight;
    }
}
