using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DestroyAfterTimeBehaviour : MonoBehaviour
{
    public float duration = 5f;

    private void Start()
    {
        StartCoroutine(DestoryAfterTime());
    }

    private IEnumerator DestoryAfterTime()
    {
        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
    }
}
