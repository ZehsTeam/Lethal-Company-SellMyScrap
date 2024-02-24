using System.Collections.Generic;

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

    public static string[] GetArrayToLower(string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i] = list[i].ToLower();
        }

        return list;
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
}
