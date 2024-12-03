using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, int damage = 80, float minDamageRange = 0f, float maxDamageRange = 6.4f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
    {
        Transform holder = null;

        if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null && RoundManager.Instance.mapPropsContainer.transform != null)
        {
            holder = RoundManager.Instance.mapPropsContainer.transform;
        }

        if (spawnExplosionEffect)
        {
            Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), holder).SetActive(true);
        }

        float distanceFromExplosion = Vector3.Distance(PlayerUtils.GetLocalPlayerScript().transform.position, explosionPosition);

        if (distanceFromExplosion < 14f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
        else if (distanceFromExplosion < 25f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
        }

        Collider[] colliders = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);

        PlayerControllerB playerScript = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            float distanceFromExplosion2 = Vector3.Distance(explosionPosition, colliders[i].transform.position);

            if (distanceFromExplosion2 > 4f && Physics.Linecast(explosionPosition, colliders[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            if (colliders[i].gameObject.layer == 3)
            {
                playerScript = colliders[i].gameObject.GetComponent<PlayerControllerB>();

                if (playerScript != null && playerScript.IsOwner)
                {
                    float damageMultiplier = 1f - Mathf.Clamp01((distanceFromExplosion2 - minDamageRange) / (maxDamageRange - minDamageRange));
                    Vector3 kickDirection = (playerScript.transform.position - explosionPosition).normalized;

                    if (playerScript.TryGetComponent(out Rigidbody rigidbody))
                    {
                        rigidbody.AddForce(kickDirection * 500f);
                    }

                    Vector3 bodyVelocity = Vector3.Normalize((playerScript.transform.position + Vector3.up * 0.75f - explosionPosition) * 100f) * 30f;

                    playerScript.DamagePlayer((int)(damage * damageMultiplier), hasDamageSFX: true, callRPC: true, causeOfDeath, 0, fallDamage: false, bodyVelocity);
                }
            }
            else if (colliders[i].gameObject.layer == 21)
            {
                Landmine landmine = colliders[i].gameObject.GetComponentInChildren<Landmine>();

                if (landmine != null && !landmine.hasExploded && distanceFromExplosion2 < 6f)
                {
                    Plugin.Instance.LogInfoExtended("Setting off other mine");

                    landmine.StartCoroutine(landmine.TriggerOtherMineDelayed(landmine));
                }
            }
            else if (colliders[i].gameObject.layer == 19)
            {
                EnemyAICollisionDetect enemyAICollisionDetect = colliders[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();

                if (enemyAICollisionDetect != null && enemyAICollisionDetect.mainScript.IsOwner && distanceFromExplosion2 < 4.5f)
                {
                    enemyAICollisionDetect.mainScript.HitEnemyOnLocalClient(force: enemyHitForce, playerWhoHit: attacker);
                    enemyAICollisionDetect.mainScript.HitFromExplosion(distanceFromExplosion2);
                }
            }
        }

        int layerMask = ~LayerMask.GetMask("Room");
        layerMask = ~LayerMask.GetMask("Colliders");

        colliders = Physics.OverlapSphere(explosionPosition, 10f, layerMask);

        for (int j = 0; j < colliders.Length; j++)
        {
            if (colliders[j].TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddExplosionForce(70f, explosionPosition, 10f);
            }
        }
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

    public static bool TryParseValue<T>(string value, out T result)
    {
        try
        {
            if (typeof(T) == typeof(int) && int.TryParse(value, out var intValue))
            {
                result = (T)(object)intValue;
                return true;
            }

            if (typeof(T) == typeof(float) && float.TryParse(value, out var floatValue))
            {
                result = (T)(object)floatValue;
                return true;
            }

            if (typeof(T) == typeof(double) && double.TryParse(value, out var doubleValue))
            {
                result = (T)(object)doubleValue;
                return true;
            }

            if (typeof(T) == typeof(bool) && bool.TryParse(value, out var boolValue))
            {
                result = (T)(object)boolValue;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)value;
                return true;
            }
        }
        catch
        {
            // Ignored
        }

        result = default;
        return false;
    }

    public static T[] StringToArray<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        try
        {
            return value.Split(',').Select(x => (T)System.Convert.ChangeType(x.Trim(), typeof(T))).ToArray();
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"Failed to convert string to array of type {typeof(T)}. \"{value}\". {ex}");
        }

        return [];
    }

    public static string ArrayToString<T>(T[] value)
    {
        if (value == null || value.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", value.Select(x => x.ToString()));
    }

    public static bool StringEquals(string input, string[] values, bool matchCase = true)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

        foreach (var value in values)
        {
            if (input.Equals(value, comparisonType))
            {
                return true;
            }
        }

        return false;
    }

    public static bool StringContains(string input, string[] values, bool matchCase = true)
    {
        System.StringComparison comparisonType = matchCase ? System.StringComparison.CurrentCulture : System.StringComparison.OrdinalIgnoreCase;

        foreach (var value in values)
        {
            if (input.Contains(value, comparisonType))
            {
                return true;
            }
        }

        return false;
    }
}
