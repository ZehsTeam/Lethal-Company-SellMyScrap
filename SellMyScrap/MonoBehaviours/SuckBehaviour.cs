using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class SuckBehaviour : MonoBehaviour
{
    protected GrabbableObject grabbableObject;

    public virtual void Start()
    {
        grabbableObject = GetComponent<GrabbableObject>();
        grabbableObject.grabbable = false;
        grabbableObject.EnablePhysics(false);
    }

    public virtual void StartEvent(Transform targetTransform, float duration)
    {
        StartCoroutine(StartAnimation(targetTransform, duration));
    }

    protected virtual IEnumerator StartAnimation(Transform targetTransform, float duration)
    {
        Vector3 startPosition = transform.position;
        yield return StartCoroutine(MoveToPosition(startPosition, targetTransform, duration));
        PlaceItemOnCounter();
    }

    protected virtual IEnumerator MoveToPosition(Vector3 from, Transform toTransform, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (toTransform == null)
            {
                yield return null;
                timer += Time.deltaTime;
                continue;
            }

            float percent = (1f / duration) * timer;
            Vector3 position = from + (toTransform.position - from) * percent;

            transform.position = position;

            yield return null;
            timer += Time.deltaTime;
        }

        transform.localPosition = toTransform.position;
    }

    protected void PlaceItemOnCounter()
    {
        DepositItemsDeskPatch.PlaceItemOnCounter(grabbableObject);
    }
}
