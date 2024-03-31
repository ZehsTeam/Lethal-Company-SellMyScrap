using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class MaxwellScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Header("Maxwell")]
    [Space(3f)]
    public GameObject bodyObject = null;
    public GameObject evilObject = null;
    public Animator danceAnimator = null;
    public AudioSource purrAudio = null;
    public AudioSource danceAudio = null;
    public AudioClip[] meowSFX = new AudioClip[0];
    public AudioClip evilNoise = null;

    private bool isEvil = false;
    private int meowIndex = 0;

    protected override void Start()
    {
        if (IsHostOrServer)
        {
            isEvil = Random.Range(1f, 100f) <= 50f;
            meowIndex = Random.Range(0, meowSFX.Length);

            SetDataClientRpc(isEvil, meowIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(bool isEvil, int meowIndex)
    {
        this.isEvil = isEvil;
        this.meowIndex = meowIndex;
    }

    protected override IEnumerator StartAnimation()
    {
        SetAnimationIdle();

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX);
        PlayOneShotSFX(meowSFX, meowIndex);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration / 3f);

        SetAnimationDance();
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        SetAnimationIdle();
        yield return new WaitForSeconds(pauseDuration / 3f);

        if (isEvil)
        {
            yield return StartCoroutine(StartEvilMaxwell());
            yield return new WaitForSeconds(5f);
            yield break;
        }

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, 2f));
    }

    private IEnumerator StartEvilMaxwell()
    {
        bodyObject.SetActive(false);
        evilObject.SetActive(true);

        purrAudio.Stop();
        PlayOneShotSFX(evilNoise);
        yield return new WaitForSeconds(1.5f);

        Vector3 position = transform.position;
        position.y += 0.31f;
        Utils.CreateExplosion(position, damage: 150);

        foreach (var rb in evilObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;

            // apply force outwards from center
            rb.AddExplosionForce(1000f, evilObject.transform.position, 100f);
        }
    }

    private void SetAnimationDance()
    {
        danceAnimator.Play("dingusDance");

        purrAudio.Stop();
        danceAudio.Play();
    }

    private void SetAnimationIdle()
    {
        danceAnimator.Play("dingusIdle");

        purrAudio.Play();
        danceAudio.Stop();
    }
}
