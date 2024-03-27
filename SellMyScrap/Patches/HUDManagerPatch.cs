using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal class HUDManagerPatch
{
    [HarmonyPatch("DisplayCreditsEarning")]
    [HarmonyPrefix]
    [HarmonyPriority(500)]
    static bool DisplayCreditsEarningPatch(ref HUDManager __instance, int creditsEarned, GrabbableObject[] objectsSold, int newGroupCredits, ref Coroutine ___scrollRewardTextCoroutine)
    {
        SellMyScrapBase.mls.LogInfo($"Earned ${creditsEarned}; sold {objectsSold.Length} items; new credits amount: ${newGroupCredits}");

        string message = ScrapHelper.GetScrapMessage(objectsSold.ToList());

        int lines = message.Split('\n').Length;
        int amount = lines < 8 ? 8 - lines : 0;

        for (int i = 0; i < amount; i++)
        {
            message += $"\n\t";
        }

        __instance.moneyRewardsListText.text = message;
        __instance.moneyRewardsTotalText.text = $"TOTAL: ${creditsEarned}";
        __instance.moneyRewardsAnimator.SetTrigger("showRewards");
        __instance.rewardsScrollbar.value = 1f;

        if (lines >= 9)
        {
            if (___scrollRewardTextCoroutine != null)
            {
                __instance.StopCoroutine(___scrollRewardTextCoroutine);
            }

            ___scrollRewardTextCoroutine = __instance.StartCoroutine(ScrollRewardsListText(__instance.rewardsScrollbar));
        }

        return false;
    }

    private static IEnumerator ScrollRewardsListText(Scrollbar rewardsScrollbar, float duration = 3f)
    {
        yield return new WaitForSeconds(1.5f);

        float timer = 0f;

        rewardsScrollbar.value = 1f;

        while (timer < duration)
        {
            if (timer > duration) timer = duration;

            float percent = (1f / duration) * timer;
            rewardsScrollbar.value = 1f - percent;

            yield return null;
            timer += Time.deltaTime;
        }

        rewardsScrollbar.value = 0f;
    }
}
