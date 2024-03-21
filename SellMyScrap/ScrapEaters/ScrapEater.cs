using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

public class ScrapEater
{
    public GameObject spawnPrefab;
    public int spawnWeight = 1;

    public ScrapEater(GameObject spawnPrefab, int spawnWeight)
    {
        this.spawnPrefab = spawnPrefab;
        this.spawnWeight = spawnWeight;
    }
}
