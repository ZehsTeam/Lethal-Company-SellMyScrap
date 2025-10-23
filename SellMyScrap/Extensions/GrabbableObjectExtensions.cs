using com.github.zehsteam.SellMyScrap.Helpers;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class GrabbableObjectExtensions
{
    public static bool IsOnShipFloor(this GrabbableObject grabbableObject)
    {
        float objectVerticalOffset = grabbableObject.itemProperties.verticalOffset;
        float objectBottomY = grabbableObject.transform.position.y - objectVerticalOffset;

        Transform shipTransform = ScrapHelper.ShipTransform;
        float shipFloorY = shipTransform.position.y;

        float yOffset = objectBottomY - shipFloorY;

        float min = -0.1f;
        float max = 0.1f;

        return yOffset >= min && yOffset <= max;
    }
}
