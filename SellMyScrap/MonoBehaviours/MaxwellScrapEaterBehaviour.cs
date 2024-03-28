using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class MaxwellScrapEaterBehaviour : ScrapEaterBehaviour
{
    [Header("Maxwell")]
    [Space(3f)]
    public GameObject bodyObject;
    public GameObject evilObject;
    public Animator danceAnimator;
    public AudioSource purrAudio;
    public AudioSource danceAudio;
    public AudioClip evilNoise;

    private bool isEvil = false;

    public override void Start()
    {
        base.Start();

        if (SellMyScrapBase.IsHostOrServer)
        {
            SetIsEvilClientRpc(Random.Range(1f, 100f) <= 50f);
        }
    }

    [ClientRpc]
    private void SetIsEvilClientRpc(bool isEvil)
    {
        this.isEvil = isEvil;
    }

    public override IEnumerator StartAnimation()
    {
        MaxwellIdle();

        Vector3 skyStartPosition = startPosition;
        skyStartPosition.y += 150f;
        transform.localPosition = skyStartPosition;

        yield return StartCoroutine(MoveToPosition(skyStartPosition, startPosition, 2f));

        yield return new WaitForSeconds(1f);

        PlaySFX(slideSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, slideDuration));
        StopSFX();
        yield return new WaitForSeconds(pauseDuration / 3f);

        MaxwellDance();
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        SuckScrapToSell();
        yield return new WaitForSeconds(suckDuration);

        PlaySFX(eatSFX);
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        MaxwellIdle();
        yield return new WaitForSeconds(pauseDuration / 3f);

        if (isEvil)
        {
            yield return StartCoroutine(StartEvilMaxwell());
            yield return new WaitForSeconds(3f);
            yield break;
        }

        PlaySFX(slideSFX);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, slideDuration));
        StopSFX();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveToPosition(startPosition, skyStartPosition, 2f));

        bodyObject.SetActive(false);
    }

    private IEnumerator StartEvilMaxwell()
    {
        bodyObject.SetActive(false);
        evilObject.SetActive(true);

        purrAudio.Stop();
        PlaySFX(evilNoise);
        yield return new WaitForSeconds(1.5f);

        Vector3 position = transform.position;
        position.y += 0.31f;
        Utils.CreateExplosion(position, true, damage: 100, maxDamageRange: 6.4f);

        foreach (var rb in evilObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;

            // apply force outwards from center
            rb.AddExplosionForce(1000f, evilObject.transform.position, 100f);
        }

        yield return new WaitForSeconds(2f);
    }

    private void MaxwellDance()
    {
        purrAudio.Stop();
        danceAudio.Play();
        danceAnimator.Play("dingusDance");
    }

    private void MaxwellIdle()
    {
        purrAudio.Play();
        danceAudio.Stop();
        danceAnimator.Play("dingusIdle");
    }
}
