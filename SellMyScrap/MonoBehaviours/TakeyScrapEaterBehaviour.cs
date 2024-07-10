using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class TakeyScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Takey")]
    [Space(5f)]
    public AudioSource soundEffectsAudioFar = null;
    public AudioClip takeOffSFXFar = null;
    public AudioClip takeySitSFX = null;
    public AudioClip[] beforeEatSFX = [];
    public AudioClip[] voiceLineSFX = [];
    public GameObject jetpackObject = null;
    public GameObject flameEffectsObject = null;
    public ParticleSystem smokeTrailParticleSystem = null;
    public AudioSource jetpackAudio = null;
    public AudioSource jetpackThrustAudio = null;
    public AudioSource jetpackBeepAudio = null;
    public AudioClip jetpackThrustStartSFX = null;
    public float startFlySpeed = 0.5f;
    public float maxFlySpeed = 100f;
    public float flySpeedMultiplier = 5f;

    private float flySpeed = 0f;

    private bool explode = false;
    private int beforeEatIndex = 0;
    private int voiceLineIndex = 0;
    
    protected override void Start()
    {
        flameEffectsObject.SetActive(false);
        jetpackObject.SetActive(false);

        if (IsHostOrServer)
        {
            explode = Random.Range(1f, 100f) <= 50f;
            voiceLineIndex = Random.Range(0, voiceLineSFX.Length);
            beforeEatIndex = Random.Range(0, beforeEatSFX.Length);

            SetDataClientRpc(explode, voiceLineIndex, beforeEatIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(bool explode, int voiceLineIndex, int beforeEatIndex)
    {
        this.explode = explode;
        this.voiceLineIndex = voiceLineIndex;
        this.beforeEatIndex = beforeEatIndex;
    }

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(takeySitSFX);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);
        yield return new WaitForSeconds(PlayOneShotSFX(beforeEatSFX, beforeEatIndex));
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(voiceLineSFX, voiceLineIndex));
        yield return new WaitForSeconds(pauseDuration);

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Takey FLY!!!
        yield return StartCoroutine(JetpackFly(6f));

        EndSmokeTrail();

        if (explode)
        {
            Utils.CreateExplosion(transform.position);
        }

        flameEffectsObject.SetActive(false);
        jetpackObject.SetActive(false);
    }

    private IEnumerator JetpackFly(float duration)
    {
        jetpackObject.SetActive(true);

        PlayOneShotSFX(takeOffSFX);
        PlayOneShotSFX(soundEffectsAudioFar, takeOffSFXFar);
        PlayOneShotSFX(jetpackAudio, jetpackThrustStartSFX);
        jetpackBeepAudio.Play();

        yield return new WaitForSeconds(0.3f);

        flameEffectsObject.SetActive(true);
        jetpackThrustAudio.Play();

        flySpeed = startFlySpeed;
        float timer = 0f;

        while (timer < duration)
        {
            if (timer >= 0.3f && !smokeTrailParticleSystem.isPlaying)
            {
                smokeTrailParticleSystem.Play();
            }

            flySpeed += flySpeedMultiplier * Time.deltaTime;
            if (flySpeed > maxFlySpeed) flySpeed = maxFlySpeed;

            Vector3 position = transform.localPosition;
            position.y += flySpeed * Time.deltaTime;

            transform.localPosition = position;

            yield return null;
            timer += Time.deltaTime;
        }
    }

    private void EndSmokeTrail()
    {
        smokeTrailParticleSystem.Stop();
        smokeTrailParticleSystem.transform.SetParent(null);
        smokeTrailParticleSystem.gameObject.AddComponent<DestroyAfterTimeBehaviour>().duration = 10f;
    }
}
