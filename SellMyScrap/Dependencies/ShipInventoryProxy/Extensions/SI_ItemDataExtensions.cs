using System.Runtime.CompilerServices;
using SI_ItemData = ShipInventoryUpdated.Objects.ItemData;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Extensions;

internal static class SI_ItemDataExtensions
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool IsScrap(this SI_ItemData itemData)
    {
        return itemData.GetItem().isScrap;
    }
}
