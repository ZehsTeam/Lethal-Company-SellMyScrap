using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class GlitchScrapEater : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Glitch")]
    [Space(5f)]

    [SerializeField]
    private AudioClip[] _beforeEatSFX = [];

    [SerializeField]
    private AudioClip[] _beforeEatLunxaraSFX = [];

    [SerializeField]
    private AudioClip _afterEatSFX;

    private int _beforeEatSFXIndex;
    private bool _playLunxaraSFX;

    private AudioClip _flashbangExplodeSFX;
    private GameObject _flashbangParticlePrefab;

    protected override void Start()
    {
        if (NetworkUtils.IsServer)
        {
            if (PlayerUtils.HasPlayerLunxara())
            {
                _playLunxaraSFX = Utils.RollPercentChance(50);
            }

            if (_playLunxaraSFX)
            {
                _beforeEatSFXIndex = Random.Range(0, _beforeEatLunxaraSFX.Length);
            }
            else
            {
                _beforeEatSFXIndex = Random.Range(0, _beforeEatSFX.Length);
            } 

            SetDataClientRpc(_beforeEatSFXIndex, _playLunxaraSFX);
        }

        FindFlashbangAssets();

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int beforeEatSFXIndex, bool playLunxaraSFX)
    {
        _beforeEatSFXIndex = beforeEatSFXIndex;
        _playLunxaraSFX = playLunxaraSFX;
    }

    private void FindFlashbangAssets()
    {
        Item item = TerminalPatch.Instance.buyableItemsList.FirstOrDefault(x => x.itemName.Equals("Stun grenade", System.StringComparison.OrdinalIgnoreCase));
        
        if (item == null)
        {
            Logger.LogError($"[GlitchScrapEater] Failed to find Stun Grenade item.");
            return;
        }

        if (!item.spawnPrefab.TryGetComponent(out StunGrenadeItem stunGrenadeItem))
        {
            Logger.LogError($"[GlitchScrapEater] Failed to find {nameof(StunGrenadeItem)} component on Stun Grenade item prefab.");
            return;
        }

        _flashbangExplodeSFX = stunGrenadeItem.explodeSFX;
        _flashbangParticlePrefab = stunGrenadeItem.stunGrenadeExplosion;
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
        yield return new WaitForSeconds(pauseDuration / 2f);

        AudioClip[] beforeEatSFX = _playLunxaraSFX ? _beforeEatLunxaraSFX : _beforeEatSFX;
        yield return new WaitForSeconds(PlayOneShotSFX(beforeEatSFX, _beforeEatSFXIndex));
        yield return new WaitForSeconds(pauseDuration / 2f);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(PlayOneShotSFX(_afterEatSFX));

        // Flashbang
        float flashbangSFXLength = _flashbangExplodeSFX.length;

        Vector3 position = mouthTransform.position;

        Instantiate(_flashbangParticlePrefab, position, Quaternion.identity);
        StunGrenadeItem.StunExplosion(position, affectAudio: true, flashSeverityMultiplier: 1f, enemyStunTime: 7.5f, 1f);
        PlayOneShotSFX(_flashbangExplodeSFX);

        yield return new WaitForSeconds(0.25f);

        modelObject.SetActive(false);

        yield return new WaitForSeconds(flashbangSFXLength);
    }
}
