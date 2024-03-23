using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    [HarmonyPrefix]
    static void SyncNewProfitQuotaClientRpcPatch(ref int overtimeBonus)
    {
        Utils.LogOvertimeBonusInfo(0, "SyncNewProfitQuotaClientRpc(); TimeOfDay.cs", overtimeBonus);
    }
}
