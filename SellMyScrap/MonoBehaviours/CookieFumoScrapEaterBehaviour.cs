using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class CookieFumoScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Header("Cookie Fumo")]
    [Space(3f)]
    public AudioClip fallSFX = null;
    public AudioClip beforeEatSFX = null;
    public AudioClip[] voiceLineSFX = new AudioClip[0];

    private int voiceLineIndex = 0;

    protected override void Start()
    {
        if (IsHostOrServer)
        {
            voiceLineIndex = Random.Range(0, voiceLineSFX.Length);

            SetDataClientRpc(voiceLineIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int voiceLineIndex)
    {
        this.voiceLineIndex = voiceLineIndex;
    }

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        PlayOneShotSFX(fallSFX);
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);
        yield return new WaitForSeconds(PlayOneShotSFX(beforeEatSFX));
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

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, 2f));
    }
}
