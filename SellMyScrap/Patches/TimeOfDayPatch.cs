using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDayPatch
{
    private static int _daysUntilDeadline = 3;
    private static int _preDaysUntilDeadline = 3;
    private static int _postDaysUntilDeadline = 3;

    [HarmonyPatch(nameof(TimeOfDay.SetBuyingRateForDay))]
    [HarmonyPrefix]
    private static void SetBuyingRateForDayPatchPrefix()
    {
        _preDaysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;
    }

    [HarmonyPatch(nameof(TimeOfDay.SetBuyingRateForDay))]
    [HarmonyPostfix]
    private static void SetBuyingRateForDayPatchPostfix()
    {
        _postDaysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;

        SetDaysUntilDeadline();
    }

    private static void SetDaysUntilDeadline()
    {
        if (_postDaysUntilDeadline == 0)
        {
            if (_preDaysUntilDeadline == 0)
            {
                _daysUntilDeadline = 0;
                return;
            }

            _daysUntilDeadline = -1;
            return;
        }

        _daysUntilDeadline = _postDaysUntilDeadline;
    }

    public static int GetDaysUntilDeadline()
    {
        return _daysUntilDeadline;
    }
}
