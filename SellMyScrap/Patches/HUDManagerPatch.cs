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
    [HarmonyPriority(int.MaxValue)]
    static bool DisplayCreditsEarning(ref HUDManager __instance, int creditsEarned, GrabbableObject[] objectsSold, int newGroupCredits, ref Coroutine ___scrollRewardTextCoroutine)
    {
        SellMyScrapBase.mls.LogInfo($"Earned {creditsEarned}; sold {objectsSold.Length} items; new credits amount: {newGroupCredits}");

        ScrapToSell scrapToSell = new ScrapToSell(objectsSold.ToList());
        string message = scrapToSell.GetListAsString();

        __instance.moneyRewardsListText.text = message;
        __instance.moneyRewardsTotalText.text = $"TOTAL: ${creditsEarned}";
        __instance.moneyRewardsAnimator.SetTrigger("showRewards");
        __instance.rewardsScrollbar.value = 1f;

        if (message.Split('\n').Length > 8)
        {
            if (___scrollRewardTextCoroutine != null)
            {
                __instance.StopCoroutine(___scrollRewardTextCoroutine);
            }

            ___scrollRewardTextCoroutine = __instance.StartCoroutine(ScrollRewardsListText(__instance.rewardsScrollbar));
        }

        return false;
    }

    private static IEnumerator ScrollRewardsListText(Scrollbar rewardsScrollbar)
    {
        yield return new WaitForSeconds(1.5f);

        float duration = 3f;
        float timer = 0f;

        rewardsScrollbar.value = 1f;

        while (timer < duration)
        {
            rewardsScrollbar.value = MapValueToNewRange(timer, 0f, duration, 1f, 0f);

            timer += Time.deltaTime;
            if (timer > duration) timer = duration;

            yield return new WaitForEndOfFrame();
        }

        rewardsScrollbar.value = 0f;
    }

    private static float MapValueToNewRange(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }
}
