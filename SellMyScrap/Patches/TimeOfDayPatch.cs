using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    [HarmonyPatch("SetNewProfitQuota")]
    [HarmonyPrefix]
    static void SetNewProfitQuotaPatch(ref TimeOfDay __instance)
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        if (__instance.daysUntilDeadline < 0)
        {
            __instance.daysUntilDeadline = 0;
        }
    }

    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    [HarmonyPrefix]
    static void SyncNewProfitQuotaClientRpcPatch(ref TimeOfDay __instance, ref int overtimeBonus)
    {
        Utils.CheckOvertimeBonus(overtimeBonus);
        SellMyScrapBase.mls.LogInfo($"\n\nSyncNewProfitQuotaClientRpc();\ndaysUntilDeadline: {__instance.daysUntilDeadline}\novertimeBonus: {overtimeBonus}\n");
    }
}
