using System;

namespace com.github.zehsteam.SellMyScrap;

internal class Utils
{
    public static string GetStringWithSpacingInBetween(string a, string b, int maxLength)
    {
        return $"{a}{new String(' ', maxLength - a.Length)} {b}";
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

    public static bool IsInt(string text)
    {
        float result;

        if (float.TryParse(text, out result))
        {
            if (result == Math.Floor(result)) return true;

            return false;
        }

        return false;
    }
}
