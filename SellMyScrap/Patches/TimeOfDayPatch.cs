using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    private static int daysUntilDeadline = 3;
    private static int preDaysUntilDeadline = 3;
    private static int postDaysUntilDeadline = 3;

    [HarmonyPatch("SetBuyingRateForDay")]
    [HarmonyPrefix]
    static void SetBuyingRateForDayPatchPrefix()
    {
        preDaysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;
    }

    [HarmonyPatch("SetBuyingRateForDay")]
    [HarmonyPostfix]
    static void SetBuyingRateForDayPatchPostfix()
    {
        postDaysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;

        SetDaysUntilDeadline();
    }

    private static void SetDaysUntilDeadline()
    {
        if (postDaysUntilDeadline == 0)
        {
            if (preDaysUntilDeadline == 0)
            {
                daysUntilDeadline = 0;
                return;
            }

            daysUntilDeadline = -1;
            return;
        }

        daysUntilDeadline = postDaysUntilDeadline;
    }

    public static int GetDaysUntilDeadline()
    {
        return daysUntilDeadline;
    }
}
