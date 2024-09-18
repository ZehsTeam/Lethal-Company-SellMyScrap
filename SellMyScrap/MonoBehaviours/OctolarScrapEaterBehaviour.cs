using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class OctolarScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Octolar")]
    [Space(5f)]
    public MeshRenderer meshRenderer = null;
    public Material[] materialVariants = [];
    public Material suckMaterial = null;
    public AudioClip fallSFX = null;
    public AudioClip suckSFX = null;
    public AudioClip afterEatSFX = null;

    private int _materialVariantIndex = 0;

    protected override void Start()
    {
        if (NetworkUtils.IsServer && materialVariants.Length > 0)
        {
            _materialVariantIndex = Random.Range(0, materialVariants.Length);

            SetDataClientRpc(_materialVariantIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int materialVariantIndex)
    {
        _materialVariantIndex = materialVariantIndex;
        SetMaterialVariant(materialVariantIndex);
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
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        SetMaterial(suckMaterial);
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration);
        PlayOneShotSFX(suckSFX);
        yield return new WaitForSeconds(suckDuration);

        SetMaterialVariant(_materialVariantIndex);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(afterEatSFX));
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

    private void SetMaterial(Material material)
    {
        if (meshRenderer == null || material == null) return;

        meshRenderer.material = material;
    }

    private void SetMaterialVariant(int index)
    {
        if (index < 0 || index > materialVariants.Length - 1) return;

        SetMaterial(materialVariants[index]);
    }
}
