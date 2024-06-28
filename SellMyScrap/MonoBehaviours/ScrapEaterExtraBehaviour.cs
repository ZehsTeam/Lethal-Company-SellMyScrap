using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ScrapEaterExtraBehaviour : ScrapEaterBehaviour
{
    [Space(20f)]
    [Header("Extra")]
    [Space(5f)]
    public Vector3 spawnPosition = new Vector3(-8.9f, 150f, -3.2f);
    public Vector3 spawnRotationOffset = new Vector3(0f, 90f, 0f);
    public Vector3 startPosition = new Vector3(-8.9f, 0f, -3.2f);
    public Vector3 endPosition = new Vector3(-8.9f, 0f, -6.72f);

    [Space(5f)]
    public Transform mouthTransform = null;
    public AudioSource soundEffectsAudio = null;
    public AudioSource movementAudio = null;

    [Space(5f)]
    public float movementDuration = 4f;
    public float suckDuration = 3.5f;
    public float pauseDuration = 2f;

    [Space(5f)]
    public AudioClip[] landSFX = new AudioClip[0];
    public AudioClip eatSFX = null;
    public AudioClip takeOffSFX = null;

    protected int landIndex = 0;

    protected override void Start()
    {
        transform.SetParent(GetHangarShipTransform());
        transform.localPosition = spawnPosition;
        transform.localRotation = Quaternion.identity;
        transform.Rotate(spawnRotationOffset, Space.Self);

        if (IsHostOrServer)
        {
            landIndex = Random.Range(0, landSFX.Length);

            SetExtraDataClientRpc(landIndex);
        }

        base.Start();
    }

    [ClientRpc]
    protected void SetExtraDataClientRpc(int landIndex)
    {
        this.landIndex = landIndex;
    }

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration);

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, 2f));
    }

    protected virtual void MoveTargetScrapToTargetTransform(Transform targetTransform, float duration)
    {
        targetScrap.ForEach(item =>
        {
            if (item == null) return;

            SuckBehaviour suckBehaviour = item.gameObject.AddComponent<SuckBehaviour>();
            suckBehaviour.StartEvent(targetTransform, duration);
        });
    }

    protected IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration)
    {
        transform.localPosition = from;

        float timer = 0f;
        while (timer < duration)
        {
            float percent = (1f / duration) * timer;
            Vector3 position = from + (to - from) * percent;

            transform.localPosition = position;

            yield return null;
            timer += Time.deltaTime;
        }

        transform.localPosition = to;
    }

    protected float PlayOneShotSFX(AudioClip audioClip, float volumeScale = 1f)
    {
        return PlayOneShotSFX(soundEffectsAudio, audioClip, volumeScale);
    }

    protected float PlayOneShotSFX(AudioClip[] audioClips, int index, float volumeScale = 1f)
    {
        if (index < 0 || index > audioClips.Length - 1) return 0f;

        PlayOneShotSFX(audioClips[index], volumeScale);
        return audioClips[index].length;
    }

    protected float PlayOneShotSFX(AudioSource audioSource, AudioClip audioClip, float volumeScale = 1f)
    {
        if (audioSource == null || audioClip == null) return 0f;

        audioSource.PlayOneShot(audioClip, volumeScale);
        return audioClip.length;
    }

    protected void PlayAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.Play();
    }

    protected void StopAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.Stop();
    }

    // MAKE THIS BETTER - ScreenShakeType has more options
    protected void ShakeCamera(float bigShakeDistance = 8f, float smallShakeDistance = 18f)
    {
        float distance = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position);

        if (distance <= bigShakeDistance)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
        else if (distance <= smallShakeDistance)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
        }
    }
}
