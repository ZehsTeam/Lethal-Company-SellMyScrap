using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    [HarmonyPatch("SetNewProfitQuota")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.LowerThanNormal)]
    static bool SetNewProfitQuotaPatch(ref TimeOfDay __instance)
    {
        if (!SellMyScrapBase.IsHostOrServer) return true;
        if (!SellMyScrapBase.Instance.ConfigManager.OverrideSetNewProfitQuota) return true;

        if (__instance.daysUntilDeadline < 0)
        {
            __instance.daysUntilDeadline = 0;
        }

        int overtimeBonus = Utils.GetOvertimeBonus(0);

        __instance.timesFulfilledQuota++;
        float num2 = Mathf.Clamp(1f + (float)__instance.timesFulfilledQuota * ((float)__instance.timesFulfilledQuota / __instance.quotaVariables.increaseSteepness), 0f, 10000f);
        num2 = __instance.quotaVariables.baseIncrease * num2 * (__instance.quotaVariables.randomizerCurve.Evaluate(Random.Range(0f, 1f)) * __instance.quotaVariables.randomizerMultiplier + 1f);
        __instance.profitQuota = (int)Mathf.Clamp((float)__instance.profitQuota + num2, 0f, 1E+09f);
        __instance.quotaFulfilled = 0;
        __instance.timeUntilDeadline = __instance.totalTime * 4f;
        __instance.SyncNewProfitQuotaClientRpc(__instance.profitQuota, overtimeBonus, __instance.timesFulfilledQuota);

        return false;
    }

    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    [HarmonyPrefix]
    static void SyncNewProfitQuotaClientRpcPatch(ref TimeOfDay __instance, ref int overtimeBonus)
    {
        SellMyScrapBase.mls.LogInfo($"\n\nSyncNewProfitQuotaClientRpc();\ndaysUntilDeadline: {__instance.daysUntilDeadline}\novertimeBonus: ${overtimeBonus}\n");
    }
}
