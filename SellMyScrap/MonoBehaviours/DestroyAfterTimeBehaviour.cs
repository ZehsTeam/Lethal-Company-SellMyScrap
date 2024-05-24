using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

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
