using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<T> StringToCollection<T>(string s, string separator = ",")
    {
        if (string.IsNullOrEmpty(s))
            return [];

        return s.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Select(x => x.TryConvertTo(out T result) ? result : default)
            .Where(x => x is not null);
    }

    public static string CollectionToString<T>(IEnumerable<T> value, string separator = ", ")
    {
        if (value == null || !value.Any())
            return string.Empty;

        return string.Join(separator, value
            .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.ToString()))
            .Select(x => x.ToString().Trim()));
    }

    public static List<List<T>> SplitList<T>(this List<T> list, int numberOfLists)
    {
        List<List<T>> result = [];

        int count = list.Count;
        int size = Mathf.CeilToInt(count / (float)numberOfLists);

        for (int i = 0; i < numberOfLists; i++)
        {
            List<T> sublist = list.GetRange(i * size, Mathf.Min(size, count - i * size));
            result.Add(sublist);
        }

        return result;
    }
}
