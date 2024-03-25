using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

internal class Utils
{
    public static bool checkOvertimeBonus = false;

    private static int calculatedOvertimeBonus = 0;

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
        int overtimeBonus = valueOver / 5 + 15 * daysUntilDeadline + SellMyScrapBase.Instance.ConfigManager.OvertimeBonusOffset;

        calculatedOvertimeBonus = overtimeBonus;
        checkOvertimeBonus = true;

        SellMyScrapBase.mls.LogInfo($"\n\nGetOvertimeBonus();\ndaysUntilDeadline: {daysUntilDeadline}\novertimeBonus: {overtimeBonus}\n");

        return overtimeBonus;
    }

    public static void CheckOvertimeBonus(int realOvertimeBonus)
    {
        if (!checkOvertimeBonus) return;
        checkOvertimeBonus = false;

        int overtimeBonusOffset = SellMyScrapBase.Instance.ConfigManager.OvertimeBonusOffset;

        if (overtimeBonusOffset != 0 && calculatedOvertimeBonus - overtimeBonusOffset == realOvertimeBonus)
        {
            SetOvertimeBonusOffset(0);
            return;
        }

        int offset = (calculatedOvertimeBonus - overtimeBonusOffset - realOvertimeBonus) * -1;
        if (offset == 0) return;

        SetOvertimeBonusOffset(offset);
    }

    private static void SetOvertimeBonusOffset(int overtimeBonusOffset)
    {
        string headerText = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}";
        string bodyText = $"Adjusted the overtimeBonusOffset for the sell confirmation screen.\n\novertimeBonusOffset: {overtimeBonusOffset}";

        DisplayTip(headerText, bodyText);
        SellMyScrapBase.mls.LogInfo($"\n\n{bodyText}\n");

        SellMyScrapBase.Instance.ConfigManager.OvertimeBonusOffset = overtimeBonusOffset;
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
}
