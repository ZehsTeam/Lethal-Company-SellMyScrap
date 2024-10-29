using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public enum ZombiesVariantType
{
    Happy,
    Scared,
    Angry,
    Drunk,
    Heart,
    Dead,
    Panda,
    Clown,
    Sunburnt,
    Party
}

public class ZombiesScrapEater : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Zombies")]
    [Space(5f)]
    public AudioClip[] BeforeEatSFX = [];
    public AudioClip[] VoiceLineSFX = [];
    public ZombiesVariant[] Variants = [];
    public Animator Animator;
    public AudioClip FallDamageSFX;
    public AudioClip DieSFX;
    public AudioClip[] HurtSFX = [];
    public AudioClip[] StepSFX = [];
    public float InbetweenStepDuration = 0.5f;

    private int _variantIndex;
    private int _beforeEatIndex;
    private int _voiceLineIndex;
    private int _hurtSFXIndex;
    private bool _playDieAnimation;

    private Coroutine _walkAudioCoroutine;

    protected override void Start()
    {
        if (NetworkUtils.IsServer)
        {
            _variantIndex = GetRandomVariantIndex();

            UpdateVariantOnLocalClient();

            _beforeEatIndex = Random.Range(0, BeforeEatSFX.Length);
            _voiceLineIndex = Random.Range(0, VoiceLineSFX.Length);
            _hurtSFXIndex = Random.Range(0, HurtSFX.Length);
            _playDieAnimation = Utils.RandomPercent(90f);

            SetDataClientRpc(_variantIndex, _beforeEatIndex, _voiceLineIndex, _hurtSFXIndex, _playDieAnimation);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int variantIndex, int beforeEatIndex, int voiceLineIndex, int hurtSFXIndex, bool playDieAnimation)
    {
        if (NetworkUtils.IsServer) return;

        _variantIndex = variantIndex;
        _beforeEatIndex = beforeEatIndex;
        _voiceLineIndex = voiceLineIndex;
        _hurtSFXIndex = hurtSFXIndex;
        _playDieAnimation = playDieAnimation;

        UpdateVariantOnLocalClient();
    }

    #region Variant Stuff
    private int GetRandomVariantIndex()
    {
        if (TargetVariantIndex > -1)
        {
            return Mathf.Clamp(TargetVariantIndex, 0, Variants.Length - 1);
        }

        return Utils.GetRandomIndexFromWeightList(Variants.Select(x => x.Weight).ToList());
    }

    private void UpdateVariantOnLocalClient()
    {
        for (int i = 0; i < Variants.Length; i++)
        {
            Variants[i].ModelObject.SetActive(i == _variantIndex);
        }

        ZombiesVariant variant = GetVariant();

        if (variant.MouthTransform != null)
        {
            mouthTransform = variant.MouthTransform;
        }

        if (variant.VoiceLineSFX.Length > 0)
        {
            VoiceLineSFX = variant.VoiceLineSFX;
        }
    }

    private ZombiesVariant GetVariant()
    {
        return Variants[_variantIndex];
    }

    private ZombiesVariantType GetVariantType()
    {
        return GetVariant().Type;
    }

    public bool IsVariantType(ZombiesVariantType type)
    {
        return GetVariantType() == type;
    }

    public bool IsVariantType(params ZombiesVariantType[] types)
    {
        foreach (var type in types)
        {
            if (IsVariantType(type))
            {
                return true;
            }
        }

        return false;
    }

    public int GetVariantIndex(ZombiesVariantType type)
    {
        for (int i = 0; i < Variants.Length; i++)
        {
            if (Variants[i].Type == type)
            {
                return i;
            }
        }

        return Random.Range(0, Variants.Length);
    }
    #endregion

    protected override IEnumerator StartAnimation()
    {
        if (_playDieAnimation)
        {
            yield return StartCoroutine(PlayZombieDeathAnimationCoroutine());
        }

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(FallDamageSFX);
        PlayOneShotSFX(HurtSFX, _hurtSFXIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        StartWalkAudio();
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopWalkAudio();

        yield return new WaitForSeconds(pauseDuration / 2f);
        yield return new WaitForSeconds(PlayOneShotSFX(BeforeEatSFX, _beforeEatIndex));
        yield return new WaitForSeconds(pauseDuration / 2f);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration / 2f);
        yield return new WaitForSeconds(PlayOneShotSFX(VoiceLineSFX, _voiceLineIndex));
        yield return new WaitForSeconds(pauseDuration / 2f);

        // Move ScrapEater to startPosition
        StartWalkAudio();
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopWalkAudio();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, 2f));
    }

    private IEnumerator PlayZombieDeathAnimationCoroutine()
    {
        Vector3 skyPos = new Vector3(endPosition.x, spawnPosition.y, endPosition.z);
        yield return StartCoroutine(MoveToPosition(skyPos, endPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(FallDamageSFX);
        ShakeCamera();
        yield return new WaitForSeconds(0.2f);
        PlayOneShotSFX(DieSFX);
        PlayDeathAnimation();
        yield return new WaitForSeconds(2f);
        transform.position = spawnPosition;
        ShowModel();
        PlayIdleAnimation();
    }

    private IEnumerator WalkAudioCoroutine()
    {
        while (true)
        {
            PlayOneShotSFX(movementAudio, StepSFX[Random.Range(0, StepSFX.Length)]);

            yield return new WaitForSeconds(InbetweenStepDuration);
        }
    }

    private void StartWalkAudio()
    {
        StopWalkAudio();
        _walkAudioCoroutine = StartCoroutine(WalkAudioCoroutine());
    }

    private void StopWalkAudio()
    {
        if (_walkAudioCoroutine != null)
        {
            StopCoroutine(_walkAudioCoroutine);
        }
    }

    private void PlayDeathAnimation()
    {
        Animator.Play("Die");
    }

    private void PlayIdleAnimation()
    {
        Animator.Play("Idle");
    }

    private void ShowModel()
    {
        modelObject.SetActive(true);
    }
}

[System.Serializable]
public class ZombiesVariant
{
    public ZombiesVariantType Type;
    [Range(0, 500)]
    public int Weight = 25;
    public GameObject ModelObject;
    public Transform MouthTransform;
    public AudioClip[] VoiceLineSFX = [];
}
