using GameNetcodeStuff;
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

    private bool isTarget = false;

    protected override void Start()
    {
        if (IsHostOrServer)
        {
            if (PlayerUtils.HasPlayerMagoroku())
            {
                isEvil = Utils.RandomPercent(80);
            }
            else
            {
                isEvil = Utils.RandomPercent(50);
            }
            
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

        if (PlayerUtils.IsLocalPlayerMagoroku() && (Utils.RandomPercent(40) || (isEvil && Utils.RandomPercent(80))))
        {
            isTarget = true;
        }
    }

    protected override IEnumerator StartAnimation()
    {
        SetAnimationIdle();

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(meowSFX, meowIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration / 3f);

        SetAnimationDance();
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        // Move targetScrap to mouthTransform over time.
        if (isTarget) StartCoroutine(MoveLocalPlayerToMaxwell(suckDuration - 0.1f));
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        SetAnimationIdle();
        yield return new WaitForSeconds(pauseDuration / 3f);

        if (isTarget) PlayerUtils.SetLocalPlayerAllowDeathEnabled(true);

        if (isEvil)
        {
            yield return StartCoroutine(StartEvilMaxwell());
            yield return new WaitForSeconds(3f);
            yield break;
        }

        if (isTarget) PlayerUtils.SetLocalPlayerMovementEnabled(true);

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, 2f));
    }

    private IEnumerator MoveLocalPlayerToMaxwell(float duration)
    {
        PlayerControllerB localPlayerScript = PlayerUtils.GetLocalPlayerScript();

        isTarget = true;
        PlayerUtils.SetLocalPlayerMovementEnabled(false);
        PlayerUtils.SetLocalPlayerAllowDeathEnabled(false);

        Vector3 startPosition = localPlayerScript.transform.position;

        Vector3 endPosition = mouthTransform.position;
        endPosition.x += 1f;
        endPosition.y = transform.position.y;

        float timer = 0f;
        while (timer < duration)
        {
            float percent = (1f / duration) * timer;
            Vector3 newPosition = startPosition + (endPosition - startPosition) * percent;
            localPlayerScript.transform.position = newPosition;

            yield return null;
            timer += Time.deltaTime;
        }
    }

    private IEnumerator StartEvilMaxwell()
    {
        bodyObject.SetActive(false);
        evilObject.SetActive(true);

        purrAudio.Stop();
        PlayOneShotSFX(evilNoise);

        yield return new WaitForSeconds(1.25f);
        if (isTarget) PlayerUtils.SetLocalPlayerMovementEnabled(true);
        yield return new WaitForSeconds(0.25f);

        Vector3 position = transform.position;
        position.y += 0.31f;
        Utils.CreateExplosion(position, damage: 150);

        evilObject.transform.SetParent(null);
        evilObject.AddComponent<DestroyAfterTimeBehaviour>().duration = 15f;

        foreach (var rb in evilObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;

            // apply force outwards from center
            rb.AddExplosionForce(1000f, evilObject.transform.position, 100f);
        }

        PlayerUtils.ReviveDeadPlayersAfterTime(5f);
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
