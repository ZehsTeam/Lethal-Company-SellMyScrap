using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

internal class Utils
{
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

    public static int GetOvertimeBonus(int value)
    {
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled + value;
        if (quotaFulfilled <= profitQuota) return 0;

        int valueOver = quotaFulfilled - profitQuota;
        int daysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;
        if (daysUntilDeadline < 0) daysUntilDeadline = 0;
        int overtimeBonus = (valueOver / 5) + (15 * daysUntilDeadline);

        SellMyScrapBase.mls.LogInfo($"\n\nGetOvertimeBonus();\ndaysUntilDeadline: {daysUntilDeadline}\novertimeBonus: {overtimeBonus}\n");

        return overtimeBonus;
    }

    public static void DisplayNotification(string displayText)
    {
        HUDManager.Instance.globalNotificationAnimator.SetTrigger("TriggerNotif");
        HUDManager.Instance.globalNotificationText.text = displayText;
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
    }

    public static void DisplayTip(string headerText, string bodyText, bool isWarning = false)
    {
        HUDManager.Instance.tipsPanelHeader.text = headerText;
        HUDManager.Instance.tipsPanelBody.text = bodyText;

        if (isWarning)
        {
            HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerWarning");
            RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.warningSFX, randomize: false);
        }
        else
        {
            HUDManager.Instance.tipsPanelAnimator.SetTrigger("TriggerHint");
            RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, HUDManager.Instance.tipsSFX, randomize: false);
        }
    }

    public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, int damage = 20, float minDamageRange = 0f, float maxDamageRange = 1f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
    {
        Debug.Log("Spawning explosion at pos: {explosionPosition}");

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
                    Debug.Log("Setting off other mine");
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

        mine.mineAudio.pitch = Random.Range(0.75f, 1.07f);
        mine.hasExploded = true;

        yield return new WaitForSeconds(0.2f);

        mine.SetOffMineAnimation();
    }
}
