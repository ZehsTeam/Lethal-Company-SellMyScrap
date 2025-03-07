﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class CookieFumoScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Cookie Fumo")]
    [Space(5f)]
    public AudioClip fallSFX;
    public AudioClip beforeEatSFX;
    public AudioClip[] voiceLineSFX = [];

    private int _voiceLineIndex;

    protected override void Start()
    {
        if (NetworkUtils.IsServer)
        {
            _voiceLineIndex = Random.Range(0, voiceLineSFX.Length);

            SetDataClientRpc(_voiceLineIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int voiceLineIndex)
    {
        _voiceLineIndex = voiceLineIndex;
    }

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        PlayOneShotSFX(fallSFX);
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        StopAudioSource(soundEffectsAudio);
        PlayOneShotSFX(landSFX, landIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration / 2f);
        yield return new WaitForSeconds(PlayOneShotSFX(beforeEatSFX));
        yield return new WaitForSeconds(pauseDuration / 2f);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(voiceLineSFX, _voiceLineIndex));
        yield return new WaitForSeconds(pauseDuration / 2f);

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
