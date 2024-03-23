using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

internal class ScrapEaterManager
{
    public static List<ScrapEater> scrapEaters = new List<ScrapEater>();
    public static List<GrabbableObject> scrapToSuck = new List<GrabbableObject>();

    public static void Initialize()
    {
        scrapEaters = [
            new ScrapEater(Content.octolarScrapEaterPrefab, () => {
                return SellMyScrapBase.Instance.ConfigManager.OctolarSpawnWeight;
            }),
            new ScrapEater(Content.takeyScrapEaterPrefab, () => {
                return SellMyScrapBase.Instance.ConfigManager.TakeySpawnWeight;
            }),
        ];
    }

    public static bool CanUseScrapEater()
    {
        int spawnChance = SellMyScrapBase.Instance.ConfigManager.ScrapEaterChance;
        return Random.Range(1, 100) <= spawnChance;
    }

    public static void SetScrapToSuckOnServer(List<GrabbableObject> scrap)
    {
        PluginNetworkBehaviour.Instance.SetScrapToSuckClientRpc(NetworkUtils.GetNetworkObjectIdsString(scrap));
    }

    public static void SetScrapToSuckOnClient(List<GrabbableObject> scrap)
    {
        scrapToSuck = scrap;

        scrap.ForEach(item =>
        {
            item.grabbable = false;
        });
    }

    public static void StartRandomScrapEaterOnServer()
    {
        int index = GetRandomScrapEaterIndex();
        if (index == -1) return;

        ScrapEaterBehaviour scrapEaterBehaviour = scrapEaters[index].spawnPrefab.GetComponent<ScrapEaterBehaviour>();
        int slideMaterialVariant = Random.Range(0, scrapEaterBehaviour.slideMaterialVariants.Length);

        PluginNetworkBehaviour.Instance.StartScrapEaterClientRpc(index, slideMaterialVariant);
    }

    public static void StartScrapEaterOnClient(int index, int slideMaterialIndex)
    {
        GameObject gameObject = Object.Instantiate(scrapEaters[index].spawnPrefab);
        gameObject.transform.SetParent(ScrapHelper.HangarShip.transform);

        ScrapEaterBehaviour scrapEaterBehaviour = gameObject.GetComponent<ScrapEaterBehaviour>();
        scrapEaterBehaviour.StartCoroutine(scrapEaterBehaviour.StartAnimation(slideMaterialIndex));
    }

    private static int GetRandomScrapEaterIndex()
    {
        List<(int index, int weight)> weightedItems = new List<(int index, int weight)>();

        for (int i = 0; i < scrapEaters.Count; i++)
        {
            weightedItems.Add((i, scrapEaters[i].GetSpawnWeight()));
        }

        int totalWeight = 0;
        foreach (var (_, weight) in weightedItems)
        {
            totalWeight += weight;
        }

        if (totalWeight == 0) return -1;

        int randomNumber = Random.Range(0, totalWeight);

        int cumulativeWeight = 0;
        foreach (var (index, weight) in weightedItems)
        {
            cumulativeWeight += weight;
            if (randomNumber < cumulativeWeight)
            {
                return index;
            }
        }

        // This should never happen if weights are correctly specified
        throw new System.InvalidOperationException("Weights are not properly specified.");
    }
}
