using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class SuckBehaviour : MonoBehaviour
{
    private GrabbableObject grabbableObject;

    void Start()
    {
        grabbableObject = GetComponent<GrabbableObject>();
        grabbableObject.grabbable = false;
        grabbableObject.EnablePhysics(false);
    }

    public IEnumerator StartAnimation(Transform mouthTransform, float duration)
    {
        Vector3 startPosition = transform.position;

        yield return StartCoroutine(MoveToPosition(startPosition, mouthTransform, duration));

        DepositItemsDeskPatch.PlaceItemOnCounter(grabbableObject);
    }

    private IEnumerator MoveToPosition(Vector3 from, Transform to, float duration)
    {
        float timer = 0f;

        while (timer <= duration)
        {
            float percent = (1f / duration) * timer;

            Vector3 endPosition = to == null ? new Vector3(0f, 0f, 0f) : to.position;
            transform.position = from + (endPosition - from) * percent;

            yield return null;
            timer += Time.deltaTime;
        }

        transform.localPosition = to.position;
    }
}
