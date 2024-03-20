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
        int daysUntilDeadline = Mathf.Max(TimeOfDay.Instance.daysUntilDeadline, 0);
        int overtimeBonus = (valueOver / 5) + (15 * daysUntilDeadline);

        if (IsLocalPlayerThorlar())
        {
            overtimeBonus -= 15;
        }

        return Mathf.Max(overtimeBonus, 0);
    }

    public static bool IsLocalPlayerThorlar()
    {
        ulong steamId = 76561197964616102; // Thorlar's Steam ID
        ulong localPlayerSteamId = StartOfRound.Instance.localPlayerController.playerSteamId;

        return steamId == localPlayerSteamId;
    }
}
