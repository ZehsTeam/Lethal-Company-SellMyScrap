using System;
using System.Globalization;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class StringExtensions
{
    public static T ConvertTo<T>(this string s)
    {
        object result = typeof(T) switch
        {
            Type t when t == typeof(int) && int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i) => i,
            Type t when t == typeof(float) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) => f,
            Type t when t == typeof(double) && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            Type t when t == typeof(bool) && bool.TryParse(s, out var b) => b,
            Type t when t == typeof(string) => s,
            Type t when t.IsEnum && Enum.TryParse(t, s, ignoreCase: true, out var e) => e,
            _ => throw new NotSupportedException($"Unsupported value type: {typeof(T)}")
        };

        return (T)result;
    }

    public static bool TryConvertTo<T>(this string s, out T result)
    {
        try
        {
            result = ConvertTo<T>(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public static bool EqualsAny(this string s, string[] values, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        return values.Any(value => s.Equals(value, comparisonType));
    }

    public static bool ContainsAny(this string s, string[] values, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        return values.Any(value => s.Contains(value, comparisonType));
    }

    public static bool StartsWithAny(this string s, string[] values, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        return values.Any(value => s.StartsWith(value, comparisonType));
    }
}
