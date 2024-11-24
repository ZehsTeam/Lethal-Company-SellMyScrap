using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal static class Utils
{
    public static bool RandomPercent(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value <= percent * 0.01f;
    }

    public static string GetStringWithSpacingInBetween(string a, string b, int maxLength)
    {
        return $"{a}{new string(' ', maxLength - a.Length)} {b}";
    }

    public static string GetLongestStringFromArray(string[] array)
    {
        string longest = string.Empty;

        foreach (var item in array)
        {
            if (item.Length > longest.Length) longest = item;
        }

        return longest;
    }

    public static string[] GetArrayToLower(string[] array)
    {
        List<string> list = new List<string>();

        foreach (var item in array)
        {
            list.Add(item.ToLower());
        }

        return list.ToArray();
    }

    public static string GetItemFromList(List<string> list, string item)
    {
        foreach (var _item in list)
        {
            if (_item.ToLower() == item.ToLower())
            {
                return _item;
            }
        }

        return string.Empty;
    }

    public static bool ArrayContains(string[] array, string value, bool matchCase = false)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

        foreach (var item in array)
        {
            if (item.Equals(value, comparisonType))
            {
                return true;
            }
        }

        return false;
    }

    public static int GetOvertimeBonus(int value)
    {
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled + value;
        if (quotaFulfilled <= profitQuota) return 0;

        int valueOver = quotaFulfilled - profitQuota;
        int overtimeBonus = valueOver / 5 + 15 * TimeOfDayPatch.GetDaysUntilDeadline();

        return Mathf.Max(overtimeBonus, 0);
    }

    public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = true, int damage = 100, float minDamageRange = 0f, float maxDamageRange = 6.4f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
    {
        Plugin.Logger.LogInfo($"Spawning explosion at pos: {explosionPosition}");

        Transform holder = null;

        if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null && RoundManager.Instance.mapPropsContainer.transform != null)
        {
            holder = RoundManager.Instance.mapPropsContainer.transform;
        }

        if (spawnExplosionEffect)
        {
            Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), holder).SetActive(value: true);
        }

        float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
        if (num < 14f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
        else if (num < 25f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
        }

        Collider[] array = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);
        PlayerControllerB playerControllerB = null;

        for (int i = 0; i < array.Length; i++)
        {
            float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
            if (num2 > 4f && Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            if (array[i].gameObject.layer == 3)
            {
                playerControllerB = array[i].gameObject.GetComponent<PlayerControllerB>();
                if (playerControllerB != null && playerControllerB.IsOwner)
                {
                    // calculate damage based on distance, so if player is minDamageRange or closer, they take full damage
                    // if player is maxDamageRange or further, they take no damage
                    // distance is num2
                    float damageMultiplier = 1f - Mathf.Clamp01((num2 - minDamageRange) / (maxDamageRange - minDamageRange));

                    playerControllerB.DamagePlayer((int)(damage * damageMultiplier), causeOfDeath: causeOfDeath);
                }
            }
            else if (array[i].gameObject.layer == 21)
            {
                Landmine componentInChildren = array[i].gameObject.GetComponentInChildren<Landmine>();
                if (componentInChildren != null && !componentInChildren.hasExploded && num2 < 6f)
                {
                    Plugin.Logger.LogInfo("Setting off other mine");
                    StartOfRound.Instance.StartCoroutine(TriggerOtherMineDelayed(componentInChildren));
                }
            }
            else if (array[i].gameObject.layer == 19)
            {
                EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                {
                    componentInChildren2.mainScript.HitEnemyOnLocalClient(enemyHitForce, playerWhoHit: attacker);
                }
            }
        }

        int num3 = ~LayerMask.GetMask("Room");
        num3 = ~LayerMask.GetMask("Colliders");
        array = Physics.OverlapSphere(explosionPosition, 10f, num3);
        for (int j = 0; j < array.Length; j++)
        {
            Rigidbody component = array[j].GetComponent<Rigidbody>();
            if (component != null)
            {
                component.AddExplosionForce(70f, explosionPosition, 10f);
            }
        }
    }

    private static IEnumerator TriggerOtherMineDelayed(Landmine mine)
    {
        if (mine.hasExploded) yield break;

        mine.mineAudio.pitch = UnityEngine.Random.Range(0.75f, 1.07f);
        mine.hasExploded = true;

        yield return new WaitForSeconds(0.2f);

        mine.SetOffMineAnimation();
    }

    public static int GetRandomIndexFromWeightList(List<int> weightList)
    {
        List<(int index, int weight)> weightedItems = [];

        for (int i = 0; i < weightList.Count; i++)
        {
            int spawnWeight = weightList[i];
            if (spawnWeight <= 0) continue;

            weightedItems.Add((i, spawnWeight));
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

    public static List<List<T>> SplitList<T>(List<T> items, int numberOfLists)
    {
        List<List<T>> result = [];

        int count = items.Count;
        int size = Mathf.CeilToInt(count / (float)numberOfLists);

        for (int i = 0; i < numberOfLists; i++)
        {
            List<T> sublist = items.GetRange(i * size, Mathf.Min(size, count - i * size));
            result.Add(sublist);
        }

        return result;
    }

    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (Plugin.Instance != null)
        {
            return Plugin.Instance.StartCoroutine(routine);
        }

        if (GameNetworkManager.Instance != null)
        {
            return GameNetworkManager.Instance.StartCoroutine(routine);
        }

        Plugin.Logger.LogError("Failed to start coroutine. " + routine);

        return null;
    }

    public static string GetStringWithColor(string text, string colorHex)
    {
        return $"<color={colorHex}>{text}</color>";
    }
}
