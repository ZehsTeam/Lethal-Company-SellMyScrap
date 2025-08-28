using com.github.zehsteam.SellMyScrap.Helpers;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class WolfyScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Wolfy")]
    [Space(5f)]
    public AudioClip[] BeforeEatSFX = [];
    public AudioClip[] AfterEatSFX = [];

    private int _beforeEatIndex;
    private int _afterEatIndex;

    protected override void Start()
    {
        if (NetworkUtils.IsServer)
        {
            _beforeEatIndex = Random.Range(0, BeforeEatSFX.Length);
            _afterEatIndex = Random.Range(0, AfterEatSFX.Length);

            SetDataClientRpc(_beforeEatIndex, _afterEatIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int beforeEatIndex, int afterEatIndex)
    {
        _beforeEatIndex = beforeEatIndex;
        _afterEatIndex = afterEatIndex;
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
        yield return new WaitForSeconds(PlayOneShotSFX(BeforeEatSFX, _beforeEatIndex));
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(AfterEatSFX, _afterEatIndex));
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
