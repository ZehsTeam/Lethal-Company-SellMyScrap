using BepInEx;
using BepInEx.Configuration;
using com.github.zehsteam.SellMyScrap.Patches;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace com.github.zehsteam.SellMyScrap.Helpers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal static class Utils
{
    public static bool IsUnityVersion(string targetUnityVersion)
    {
        return Application.unityVersion.StartsWith(targetUnityVersion, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetPluginPersistentDataPath()
    {
        return Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
    }

    public static ConfigFile CreateConfigFile(BaseUnityPlugin plugin, string path, string name = null, bool saveOnInit = false)
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(plugin);
        name ??= metadata.GUID;
        name += ".cfg";
        return new ConfigFile(Path.Combine(path, name), saveOnInit, metadata);
    }

    public static ConfigFile CreateLocalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        return CreateConfigFile(plugin, Paths.ConfigPath, name, saveOnInit);
    }

    public static ConfigFile CreateGlobalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        string path = GetPluginPersistentDataPath();
        name ??= "global";
        return CreateConfigFile(plugin, path, name, saveOnInit);
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
                    Logger.LogInfo("Setting off other mine", extended: true);

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

    public static int GetRandomIndexFromWeightList(IEnumerable<int> weights)
    {
        if (weights == null)
            throw new ArgumentNullException(nameof(weights));

        int totalWeight = 0;
        int index = 0;

        foreach (var w in weights)
        {
            if (w < 0)
                throw new ArgumentException("Weights must be non-negative.");
            totalWeight += w;
        }

        if (totalWeight == 0)
            throw new ArgumentException("At least one weight must be greater than zero.");

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;
        index = 0;

        foreach (var w in weights)
        {
            cumulative += w;
            if (randomValue < cumulative)
                return index;
            index++;
        }

        // Safety fallback
        return index - 1;
    }
}
