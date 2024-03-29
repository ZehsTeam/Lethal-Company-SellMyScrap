﻿using System;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

public class ScrapEater
{
    public GameObject spawnPrefab;
    public Func<int> GetSpawnWeight;

    public ScrapEater(GameObject spawnPrefab, Func<int> GetSpawnWeight)
    {
        this.spawnPrefab = spawnPrefab;
        this.GetSpawnWeight = GetSpawnWeight;
    }
}
