﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class PsychoScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Psycho")]
    [Space(5f)]
    public MeshRenderer meshRenderer = null;
    public Material normalMaterial = null;
    public Material suckMaterial = null;
    public AudioClip hohSFX = null;
    public AudioClip suckSFX = null;
    public AudioClip raidSFX = null;
    public ParticleSystem potatoesParticleSystem = null;

    private bool _raid = false;

    protected override void Start()
    {
        SetMaterial(normalMaterial);

        if (suckSFX != null)
        {
            suckDuration = suckSFX.length;
        }

        if (NetworkUtils.IsServer)
        {
            _raid = Utils.RandomPercent(100);
            SetDataClientRpc(_raid);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(bool raid)
    {
        _raid = raid;
    }

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(hohSFX);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        SetMaterial(suckMaterial);
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration);
        PlayOneShotSFX(suckSFX);
        yield return new WaitForSeconds(suckDuration);

        SetMaterial(normalMaterial);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(hohSFX));
        yield return new WaitForSeconds(pauseDuration / 2f);

        if (_raid)
        {
            yield return StartCoroutine(RaidAnimation());
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

    private IEnumerator RaidAnimation()
    {
        float raidSFXLength = raidSFX.length;
        float particleSystemStart = 3.36f;
        float particleSystemLength = raidSFXLength - particleSystemStart - 0.5f;

        PlayOneShotSFX(raidSFX);
        yield return new WaitForSeconds(particleSystemStart);
        SetMaterial(suckMaterial);
        potatoesParticleSystem.Play();
        yield return new WaitForSeconds(particleSystemLength);
        potatoesParticleSystem.Stop();
        potatoesParticleSystem.transform.SetParent(null);
        potatoesParticleSystem.gameObject.AddComponent<DestroyAfterTimeBehaviour>().duration = 15f;
        SetMaterial(normalMaterial);
        yield return new WaitForSeconds(pauseDuration);
    }

    private void SetMaterial(Material material)
    {
        if (meshRenderer == null || material == null) return;

        meshRenderer.material = material;
    }
}
