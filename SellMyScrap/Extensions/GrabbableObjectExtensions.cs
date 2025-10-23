using com.github.zehsteam.SellMyScrap.Helpers;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class GrabbableObjectExtensions
{
    public static bool IsOnShipFloor(this GrabbableObject grabbableObject)
    {
        Collider collider = grabbableObject.GetComponent<Collider>();
        if (collider == null) return false;

        Transform shipTransform = ScrapHelper.ShipTransform;
        float shipFloorY = shipTransform.position.y;

        Bounds objectBounds = collider.bounds;
        float objectBottomY = objectBounds.center.y - objectBounds.extents.y;

        float yOffset = objectBottomY - shipFloorY;

        return yOffset >= -0.02f && yOffset <= 0.1f;
    }
}
