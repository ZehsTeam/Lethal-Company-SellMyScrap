using com.github.zehsteam.SellMyScrap.Dependencies;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public enum TakeyVariantType
{
    Default,
    Gazmi,
    Shady,
    Captain,
    Gamble,
    ChickenDance,
    DinkDonk,
    FightClub,
    Cute,
    Feels,
    Stabby,
    LUBBERS,
    ALOO,
    Gift,
    Cake
}

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
    public TakeyVariant[] Variants = [];

    [Header("Jetpack")]
    [Space(5f)]
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

    [Header("Gamble Variant")]
    public MeshFilter CardMeshFilter = null;
    public Mesh[] CardMeshes = [];

    [Header("Peepo Chicken Variant")]
    public Animator ChickenAnimator = null;
    public AudioSource ChickenAudio = null;

    [Header("Donk Dink Variant")]
    public Animator DinkDonkAnimator = null;
    public AudioSource DinkDonkAudio = null;
    public AudioClip DinkDonkDropSFX = null;
    public AudioClip DinkDonkSpecialLine1SFX = null;
    public AudioClip DinkDonkSpecialLine2SFX = null;
    public AudioClip DinkDonkSpecialLine3SFX = null;
    public AudioClip DinkDonkSpecialLine4SFX = null;

    [Header("Feels Variant")]
    public Animator FeelsAnimator = null;

    [Header("LUBBERS Variant")]
    public Animator LUBBERSAnimator = null;

    private float _flySpeed = 0f;

    private int _variantIndex = 0;
    private int _cardMeshIndex = 0;
    private bool _explode = false;
    private int _beforeEatIndex = 0;
    private int _voiceLineIndex = 0;
    
    protected override void Start()
    {
        flameEffectsObject.SetActive(false);
        jetpackObject.SetActive(false);

        if (NetworkUtils.IsServer)
        {
            _variantIndex = GetRandomVariantIndex();
            _cardMeshIndex = Random.Range(0, CardMeshes.Length);

            UpdateVariantOnLocalClient();

            _explode = Utils.RandomPercent(50f);
            _voiceLineIndex = Random.Range(0, voiceLineSFX.Length);
            _beforeEatIndex = Random.Range(0, beforeEatSFX.Length);

            SetDataClientRpc(_variantIndex, _cardMeshIndex, _explode, _voiceLineIndex, _beforeEatIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int variantIndex, int cardMeshIndex, bool explode, int voiceLineIndex, int beforeEatIndex)
    {
        if (NetworkUtils.IsServer) return;

        _variantIndex = variantIndex;
        _cardMeshIndex = cardMeshIndex;
        _explode = explode;
        _voiceLineIndex = voiceLineIndex;
        _beforeEatIndex = beforeEatIndex;

        UpdateVariantOnLocalClient();
    }

    #region Variant Stuff
    private int GetRandomVariantIndex()
    {
        if (TargetVariantIndex > -1)
        {
            return Mathf.Clamp(TargetVariantIndex, 0, Variants.Length - 1);
        }

        if (PlayerUtils.IsLocalPlayer(PlayerName.PsychoHypnotic) && !ModpackSaveSystem.ReadValue("ForcedShowTakeyScrapEaterPeepoChickenVariant3", false))
        {
            ModpackSaveSystem.WriteValue("ForcedShowTakeyScrapEaterPeepoChickenVariant3", true);
            return GetVariantIndex(TakeyVariantType.ChickenDance);
        }

        if (PlayerUtils.IsLocalPlayer(PlayerName.Takerst) && !ModpackSaveSystem.ReadValue("ForcedShowTakeyScrapEaterDinkDonkVariant", false) && targetScrap.Sum(x => x.scrapValue) >= 1000 && Utils.RandomPercent(60))
        {
            ModpackSaveSystem.WriteValue("ForcedShowTakeyScrapEaterDinkDonkVariant", true);
            return GetVariantIndex(TakeyVariantType.DinkDonk);
        }

        return Utils.GetRandomIndexFromWeightList(Variants.Select(x => x.Weight).ToList());
    }

    private void UpdateVariantOnLocalClient()
    {
        for (int i = 0; i < Variants.Length; i++)
        {
            Variants[i].ModelObject.SetActive(i == _variantIndex);
        }

        TakeyVariant variant = GetVariant();

        if (variant.MouthTransform != null)
        {
            mouthTransform = variant.MouthTransform;
        }
        
        if (variant.BeforeEatSFX.Length > 0)
        {
            beforeEatSFX = variant.BeforeEatSFX;
        }

        if (IsVariantType(TakeyVariantType.Gamble))
        {
            CardMeshFilter.mesh = CardMeshes[_cardMeshIndex];
        }

        if (IsVariantType(TakeyVariantType.ChickenDance))
        {
            ChickenAnimator.SetBool("Animate", false);
        }
    }

    private TakeyVariant GetVariant()
    {
        return Variants[_variantIndex];
    }

    private TakeyVariantType GetVariantType()
    {
        return GetVariant().Type;
    }

    public bool IsVariantType(TakeyVariantType type)
    {
        return GetVariantType() == type;
    }

    public int GetVariantIndex(TakeyVariantType type)
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
        HangarShipDoor hangarShipDoor = FindFirstObjectByType<HangarShipDoor>();

        if (IsVariantType(TakeyVariantType.Gazmi) && hangarShipDoor != null)
        {
            hangarShipDoor.PlayDoorAnimation(closed: false);
            hangarShipDoor.SetDoorButtonsEnabled(false);
        }

        if (IsVariantType(TakeyVariantType.ChickenDance))
        {
            ChickenAnimator.SetBool("Animate", false);
        }

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", true);
            DinkDonkAudio.Play();
            TriggerTakeyPlushDinkDonkScrapEaterSpawnedEvent();
        }

        if (IsVariantType(TakeyVariantType.Feels))
        {
            FeelsAnimator.SetBool("Animate", true);
        }

        if (IsVariantType(TakeyVariantType.LUBBERS))
        {
            LUBBERSAnimator.SetBool("Animate", true);
        }

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(takeySitSFX);

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", false);
            DinkDonkAudio.Stop();
            PlayOneShotSFX(DinkDonkDropSFX);
        }

        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(1f);

        if (IsVariantType(TakeyVariantType.ChickenDance))
        {
            ChickenAnimator.SetBool("Animate", true);
            ChickenAudio.Play();
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(PlayOneShotSFX(beforeEatSFX, _beforeEatIndex));

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            yield return new WaitForSeconds(0.5f);
            DinkDonkAnimator.SetBool("Animate", true);
            DinkDonkAudio.Play();
        }

        yield return new WaitForSeconds(1f);

        bool playedCustomSuckAnimation = false;

        if (IsVariantType(TakeyVariantType.ChickenDance))
        {
            playedCustomSuckAnimation = true;
            yield return StartCoroutine(MoveTargetScrapToTargetTransformDelayed(mouthTransform, suckDuration - 0.1f, duration: 15f));
            yield return new WaitForSeconds(suckDuration);
            yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        }

        if (IsVariantType(TakeyVariantType.DinkDonk) && targetScrap.Sum(x => x.scrapValue) >= 1000)
        {
            playedCustomSuckAnimation = true;
            yield return StartCoroutine(DinkDonkSpecialSuckAnimation());
        }

        if (!playedCustomSuckAnimation)
        {
            MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
            yield return new WaitForSeconds(suckDuration);
            yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        }

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", false);
            DinkDonkAudio.Stop();
            yield return new WaitForSeconds(0.5f);
        }

        if (!(IsVariantType(TakeyVariantType.DinkDonk) && playedCustomSuckAnimation))
        {
            yield return new WaitForSeconds(PlayOneShotSFX(voiceLineSFX, _voiceLineIndex));
            yield return new WaitForSeconds(1f);
        }

        if (IsVariantType(TakeyVariantType.ChickenDance))
        {
            ChickenAnimator.SetBool("Animate", false);
            ChickenAudio.Stop();
            yield return new WaitForSeconds(1f);
        }

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", true);
            DinkDonkAudio.Play();
            yield return new WaitForSeconds(0.5f);
        }

        if (IsVariantType(TakeyVariantType.Gazmi) && hangarShipDoor != null)
        {
            hangarShipDoor.SetDoorButtonsEnabled(true);
        }

        // Takey FLY!!!
        yield return StartCoroutine(JetpackFly(6f));

        EndSmokeTrail();

        if (_explode)
        {
            Utils.CreateExplosion(transform.position);
        }

        flameEffectsObject.SetActive(false);
        jetpackObject.SetActive(false);
    }

    private void TriggerTakeyPlushDinkDonkScrapEaterSpawnedEvent()
    {
        if (!IsVariantType(TakeyVariantType.DinkDonk)) return;

        if (TakeyPlushProxy.Enabled)
        {
            TakeyPlushProxy.TriggerDinkDonkScrapEaterSpawnedEvent();
        }
    }

    private IEnumerator DinkDonkSpecialSuckAnimation()
    {
        List<List<GrabbableObject>> targetScrapLists = Utils.SplitList(targetScrap.OrderBy(x => x.scrapValue).ToList(), numberOfLists: 3);

        yield return new WaitForSeconds(PlayOneShotSFX(DinkDonkSpecialLine1SFX));
        yield return new WaitForSeconds(0.5f);

        PlayOneShotSFX(DinkDonkSpecialLine2SFX);
        yield return new WaitForSeconds(1f);
        MoveTargetScrapToTargetTransform(targetScrapLists[0], mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(0.5f);

        PlayOneShotSFX(DinkDonkSpecialLine3SFX);
        yield return new WaitForSeconds(1.2f);
        MoveTargetScrapToTargetTransform(targetScrapLists[1], mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(0.5f);

        PlayOneShotSFX(DinkDonkSpecialLine4SFX);
        yield return new WaitForSeconds(2.1f);
        MoveTargetScrapToTargetTransform(targetScrapLists[2], mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
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

        _flySpeed = startFlySpeed;
        float timer = 0f;

        while (timer < duration)
        {
            if (timer >= 0.3f && !smokeTrailParticleSystem.isPlaying)
            {
                smokeTrailParticleSystem.Play();
            }

            _flySpeed += flySpeedMultiplier * Time.deltaTime;
            if (_flySpeed > maxFlySpeed) _flySpeed = maxFlySpeed;

            Vector3 position = transform.localPosition;
            position.y += _flySpeed * Time.deltaTime;

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

[System.Serializable]
public class TakeyVariant
{
    public TakeyVariantType Type;
    [Range(0, 100)]
    public int Weight;
    public GameObject ModelObject;
    public Transform MouthTransform;
    public AudioClip[] BeforeEatSFX;
}
