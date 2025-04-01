using BepInEx;
using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal static class Utils
{
    public static string GetEnumName<T>(T value) where T : Enum
    {
        return Enum.GetName(typeof(T), value) ?? string.Empty;
    }

    public static string GetPluginDirectoryPath()
    {
        return Path.GetDirectoryName(Plugin.Instance.Info.Location);
    }

    public static string GetConfigDirectoryPath()
    {
        return Paths.ConfigPath;
    }

    public static string GetGlobalConfigDirectoryPath()
    {
        return Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
    }

    public static ConfigFile CreateConfigFile(string directoryPath, string name = null, bool saveOnInit = false)
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(Plugin.Instance);
        name ??= metadata.GUID;
        name += ".cfg";
        return new ConfigFile(Path.Combine(directoryPath, name), saveOnInit, metadata);
    }

    public static ConfigFile CreateLocalConfigFile(string name = null, bool saveOnInit = false)
    {
        name ??= $"{MyPluginInfo.PLUGIN_GUID}-{name}";
        return CreateConfigFile(Paths.ConfigPath, name, saveOnInit);
    }

    public static ConfigFile CreateGlobalConfigFile(string name = null, bool saveOnInit = false)
    {
        name ??= "global";
        return CreateConfigFile(GetGlobalConfigDirectoryPath(), name, saveOnInit);
    }

    public static bool RandomPercent(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value * 100f <= percent;
    }

    public static bool ArrayContains(string[] array, string value, bool matchCase = false)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;

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
                    EnemyAIHelper.HitEnemyOnLocalClient(enemyAICollisionDetect.mainScript, force: enemyHitForce, playerWhoHit: attacker);
                    EnemyAIHelper.HitFromExplosion(enemyAICollisionDetect.mainScript, distanceFromExplosion2);
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
        throw new InvalidOperationException("Weights are not properly specified.");
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

    public static IEnumerable<T> StringToCollection<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Enumerable.Empty<T>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Select(x => TryConvertStringToType(x, out T result) ? result : default)
            .Where(x => x is not null);
    }
    
    public static string CollectionToString<T>(IEnumerable<T> value)
    {
        if (value == null || !value.Any())
        {
            return string.Empty;
        }

        return string.Join(", ", value
            .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.ToString()))
            .Select(x => x.ToString().Trim()));
    }

    public static bool TryConvertStringToType<T>(string value, out T result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        try
        {
            Type targetType = typeof(T);

            // Handle Enums
            if (targetType.IsEnum && Enum.TryParse(targetType, value.Trim(), true, out object enumResult))
            {
                result = (T)enumResult;
                return true;
            }

            // Handle GUIDs
            if (targetType == typeof(Guid) && Guid.TryParse(value.Trim(), out Guid guidResult))
            {
                result = (T)(object)guidResult;
                return true;
            }

            // Handle nullable types
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            result = (T)Convert.ChangeType(value.Trim(), underlyingType);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public static bool StringEquals(string input, string[] values, bool matchCase = true)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;
        return values.Any(value => input.Equals(value, comparisonType));
    }

    public static bool StringContains(string input, string[] values, bool matchCase = true)
    {
        StringComparison comparisonType = matchCase ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;
        return values.Any(value => input.Contains(value, comparisonType));
    }
}
