using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    [HarmonyPatch("SetNewProfitQuota")]
    [HarmonyPrefix]
    static void SetNewProfitQuotaPatch()
    {
        Utils.LogOvertimeBonusInfo(0, "SetNewProfitQuota(); TimeOfDay.cs");
    }
}
