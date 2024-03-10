using com.github.zehsteam.SellMyScrap.Patches;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

public class SuckBehaviour : MonoBehaviour
{
    private GrabbableObject grabbableObject;

    private Vector3 startPosition;
    private Vector3 endPosition => Octolar.mouth.position;

    private float duration = 3f;
    private float timer = 0f;

    private bool isOnCounter = false;

    void Start()
    {
        grabbableObject = GetComponent<GrabbableObject>();
        grabbableObject.grabbable = false;

        startPosition = transform.position;
    }

    void Update()
    {
        if (isOnCounter) return;

        bool isTimerComplete = timer > duration;
        if (isTimerComplete) timer = duration;

        bool isWithinDistance = Vector3.Distance(transform.position, endPosition) < 0.1f;

        if (isTimerComplete || isWithinDistance)
        {
            DepositItemsDeskPatch.PlaceItemOnCounter(grabbableObject);
            isOnCounter = true;
            return;
        }

        float percent = (1f / duration) * timer;
        transform.position = startPosition + (endPosition - startPosition) * percent;

        timer += Time.deltaTime;
    }
}
