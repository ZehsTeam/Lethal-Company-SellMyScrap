using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class OctolarScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Header("Octolar")]
    [Space(3f)]
    public MeshRenderer meshRenderer = null;
    public Material[] materialVariants = new Material[0];
    public Material suckMaterial = null;
    public AudioClip afterEatSFX = null;

    private int materialVariantIndex = 0;

    protected override void Start()
    {
        if (IsHostOrServer && materialVariants.Length > 0)
        {
            materialVariantIndex = Random.Range(0, materialVariants.Length);

            SetDataClientRpc(materialVariantIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int materialVariantIndex)
    {
        this.materialVariantIndex = materialVariantIndex;
        SetMaterialVariant(materialVariantIndex);
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
        SetMaterial(suckMaterial);
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        SetMaterialVariant(materialVariantIndex);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        PlayOneShotSFX(afterEatSFX);
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
