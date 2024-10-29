using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class MaxwellScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Maxwell")]
    [Space(5f)]
    public GameObject bodyObject;
    public GameObject evilObject;
    public Animator danceAnimator;
    public AudioSource purrAudio;
    public AudioSource danceAudio;
    public AudioClip[] meowSFX = [];
    public AudioClip evilNoise;

    private bool _isEvil;
    private int _meowIndex;

    private bool _isTarget;

    protected override void Start()
    {
        if (NetworkUtils.IsServer)
        {
            if (PlayerUtils.HasPlayer(PlayerName.Magoroku, PlayerName.PsychoHypnotic))
            {
                _isEvil = Utils.RandomPercent(80);
            }
            else
            {
                _isEvil = Utils.RandomPercent(50);
            }
            
            _meowIndex = Random.Range(0, meowSFX.Length);

            SetDataClientRpc(_isEvil, _meowIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(bool isEvil, int meowIndex)
    {
        _isEvil = isEvil;
        _meowIndex = meowIndex;

        if (PlayerUtils.IsLocalPlayer(PlayerName.Magoroku) && (Utils.RandomPercent(40) || (isEvil && Utils.RandomPercent(80))))
        {
            _isTarget = true;
        }
    }

    protected override IEnumerator StartAnimation()
    {
        SetAnimationIdle();

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(meowSFX, _meowIndex);
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
        if (_isTarget) StartCoroutine(MoveLocalPlayerToMaxwell(suckDuration - 0.1f));
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration / 3f * 2f);

        SetAnimationIdle();
        yield return new WaitForSeconds(pauseDuration / 3f);

        if (_isTarget) PlayerUtils.SetLocalPlayerAllowDeathEnabled(true);

        if (_isEvil)
        {
            yield return StartCoroutine(StartEvilMaxwell());
            yield return new WaitForSeconds(3f);
            yield break;
        }

        if (_isTarget) PlayerUtils.SetLocalPlayerMovementEnabled(true);

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

        _isTarget = true;
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
        if (_isTarget) PlayerUtils.SetLocalPlayerMovementEnabled(true);
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
