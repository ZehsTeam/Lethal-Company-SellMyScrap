using System;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ScrapEater
{
    public GameObject SpawnPrefab;
    public Func<int> GetSpawnWeight;

    public ScrapEater(GameObject spawnPrefab, Func<int> getSpawnWeight)
    {
        SpawnPrefab = spawnPrefab;
        GetSpawnWeight = getSpawnWeight;
    }
}
