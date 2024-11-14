using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class SuckBehaviour : MonoBehaviour
{
    protected GrabbableObject _grabbableObject;
    protected RagdollGrabbableObject _ragdollGrabbableObject;

    public virtual void StartEvent(Transform targetTransform, float duration)
    {
        _grabbableObject = GetComponent<GrabbableObject>();
        _grabbableObject.grabbable = false;
        _grabbableObject.parentObject = null;
        _grabbableObject.EnablePhysics(false);

        _ragdollGrabbableObject = GetComponent<RagdollGrabbableObject>();

        StartCoroutine(StartAnimation(targetTransform, duration));
    }

    protected virtual IEnumerator StartAnimation(Transform targetTransform, float duration)
    {
        if (_ragdollGrabbableObject == null)
        {
            yield return StartCoroutine(MoveToPosition(targetTransform, duration));
        }
        else
        {
            yield return StartCoroutine(MoveRagdollToPosition(targetTransform, duration));
        }

        PlaceItemOnCounter();
    }

    protected virtual IEnumerator MoveToPosition(Transform targetTransform, float duration)
    {
        Vector3 startPosition = transform.position;

        float timer = 0f;

        while (timer < duration)
        {
            if (targetTransform == null)
            {
                yield return null;
                timer += Time.deltaTime;
                continue;
            }

            float percent = (1f / duration) * timer;
            Vector3 position = startPosition + (targetTransform.position - startPosition) * percent;

            transform.position = position;

            yield return null;
            timer += Time.deltaTime;
        }

        if (targetTransform != null)
        {
            transform.position = targetTransform.position;
        }
    }

    protected virtual IEnumerator MoveRagdollToPosition(Transform targetTransform, float duration)
    {
        if (_ragdollGrabbableObject == null || !_ragdollGrabbableObject.foundRagdollObject)
        {
            Plugin.Logger.LogError("RagdollGrabbableObject is null or the ragdoll object was not found.");
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Transform ragdollTransform = _ragdollGrabbableObject.ragdoll.transform;
        Vector3 startPosition = ragdollTransform.position;

        float timer = 0f;

        while (timer < duration)
        {
            if (targetTransform == null)
            {
                yield return null;
                timer += Time.deltaTime;
                continue;
            }

            float percent = (1f / duration) * timer;
            Vector3 position = startPosition + (targetTransform.position - startPosition) * percent;

            ragdollTransform.position = position;

            yield return null;
            timer += Time.deltaTime;
        }

        if (targetTransform != null)
        {
            ragdollTransform.position = targetTransform.position;
        }
    }

    protected void PlaceItemOnCounter()
    {
        if (_ragdollGrabbableObject == null)
        {
            DepositItemsDeskPatch.PlaceItemOnCounter(_grabbableObject);
        }
        else
        {
            DepositItemsDeskPatch.PlaceRagdollOnCounter(_ragdollGrabbableObject);
        }
    }
}
