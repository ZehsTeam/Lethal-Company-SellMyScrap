using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

internal class ScrapEaterManager
{
    private static List<ScrapEater> scrapEaters = new List<ScrapEater>();
    public static List<GrabbableObject> scrapToSuck = new List<GrabbableObject>();

    public static ScrapEater activeScrapEater;
    public static Transform mouthTransform => activeScrapEater == null ? null : activeScrapEater.mouthTransform;

    public static void Initialize()
    {
        scrapEaters = [
            new OctolarScrapEater(),
            new TakeyScrapEater()
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
    }

    public static void StartRandomScrapEaterOnServer()
    {
        int index = GetRandomScrapEaterIndex();
        int slideMaterialIndex = scrapEaters[index].GetRandomSlideMaterialIndex();

        PluginNetworkBehaviour.Instance.StartScrapEaterClientRpc(index, slideMaterialIndex);
    }

    public static void StartScrapEaterOnClient(int index, int slideMaterialIndex)
    {
        ScrapEater scrapEater = scrapEaters[index];
        activeScrapEater = scrapEater;
        scrapEater.startCoroutine = StartOfRound.Instance.StartCoroutine(scrapEater.Start(slideMaterialIndex));
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
