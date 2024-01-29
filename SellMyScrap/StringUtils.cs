using System;

namespace Zehs.SellMyScrap;

internal class StringUtils
{
    public static string GetStringWithSpacingInBetween(string a, string b, int maxLength)
    {
        return $"{a}{new String(' ', maxLength - a.Length)} {b}";
    }

    public static string GetStringWithCharInBetween(string a, string b, char c, int maxLength)
    {
        if (maxLength - a.Length < 3) return GetStringWithSpacingInBetween(a, b, maxLength);

        string result = $"{a} ";
        result.PadRight(maxLength - 1, c);
        result += $" {b}";

        return result;
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
}
