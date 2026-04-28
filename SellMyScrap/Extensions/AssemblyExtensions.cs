using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class AssemblyExtensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null)
            return [];

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null);
        }
    }
}
