using com.github.zehsteam.SellMyScrap.Dependencies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

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
    Cake,
    Dracula,
    Pumpkin,
    Pilgrim,
    Santa,
    Nutcracker
}

public class TakeyScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Space(20f)]
    [Header("Takey")]
    [Space(5f)]
    public AudioSource soundEffectsAudioFar;
    public AudioClip takeOffSFXFar;
    public AudioClip takeySitSFX;
    public AudioClip[] beforeEatSFX = [];
    public AudioClip[] voiceLineSFX = [];
    public TakeyVariant[] Variants = [];

    [Space(10f)]
    [Header("Jetpack")]
    [Space(5f)]
    public GameObject jetpackObject;
    public GameObject flameEffectsObject;
    public ParticleSystem smokeTrailParticleSystem;
    public AudioSource jetpackAudio;
    public AudioSource jetpackThrustAudio;
    public AudioSource jetpackBeepAudio;
    public AudioClip jetpackThrustStartSFX;
    public float startFlySpeed = 0.5f;
    public float maxFlySpeed = 100f;
    public float flySpeedMultiplier = 5f;

    [Space(10f)]
    [Header("Gamble Variant")]
    [Space(5f)]
    public MeshFilter CardMeshFilter;
    public Mesh[] CardMeshes = [];

    [Space(10f)]
    [Header("Peepo Chicken Variant")]
    [Space(5f)]
    public Animator ChickenAnimator;
    public AudioSource ChickenAudio;

    [Space(10f)]
    [Header("Donk Dink Variant")]
    [Space(5f)]
    public Animator DinkDonkAnimator;
    public AudioSource DinkDonkAudio;
    public AudioClip DinkDonkDropSFX;
    public AudioClip DinkDonkSpecialLine1SFX;
    public AudioClip DinkDonkSpecialLine2SFX;
    public AudioClip DinkDonkSpecialLine3SFX;
    public AudioClip DinkDonkSpecialLine4SFX;

    [Space(10f)]
    [Header("Feels Variant")]
    [Space(5f)]
    public Animator FeelsAnimator;

    [Space(10f)]
    [Header("LUBBERS Variant")]
    [Space(5f)]
    public Animator LUBBERSAnimator;

    [Space(10f)]
    [Header("Party Hat")]
    [Space(5f)]
    public GameObject[] PartyHatObjects = [];
    public Material[] PartyHatMaterials = [];

    [Space(10f)]
    [Header("Cake Variant")]
    [Space(5f)]
    public MeshRenderer CakeMeshRenderer;
    public Material[] CakeMaterials = [];

    private float _flySpeed;

    private int _variantIndex;
    private int _cardMeshIndex;
    private bool _explode;
    private int _beforeEatIndex;
    private int _voiceLineIndex;
    private int _partyHatMaterialIndex;
    private int _cakeMaterialIndex;

    protected override void Start()
    {
        flameEffectsObject.SetActive(false);
        jetpackObject.SetActive(false);

        if (NetworkUtils.IsServer)
        {
            _variantIndex = GetRandomVariantIndex();
            _cardMeshIndex = Random.Range(0, CardMeshes.Length);
            _partyHatMaterialIndex = Random.Range(0, PartyHatMaterials.Length);
            _cakeMaterialIndex = Random.Range(0, CakeMaterials.Length);

            UpdateVariantOnLocalClient();

            _explode = Utils.RandomPercent(50f);
            _voiceLineIndex = Random.Range(0, voiceLineSFX.Length);
            _beforeEatIndex = Random.Range(0, beforeEatSFX.Length);

            SetDataClientRpc(_variantIndex, _cardMeshIndex, _explode, _voiceLineIndex, _beforeEatIndex, _partyHatMaterialIndex, _cakeMaterialIndex);
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(int variantIndex, int cardMeshIndex, bool explode, int voiceLineIndex, int beforeEatIndex, int partyHatMaterialIndex, int cakeMaterialIndex)
    {
        if (NetworkUtils.IsServer) return;

        _variantIndex = variantIndex;
        _cardMeshIndex = cardMeshIndex;
        _explode = explode;
        _voiceLineIndex = voiceLineIndex;
        _beforeEatIndex = beforeEatIndex;
        _partyHatMaterialIndex = partyHatMaterialIndex;
        _cakeMaterialIndex = cakeMaterialIndex;

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

        if (IsVariantType(TakeyVariantType.Gift, TakeyVariantType.Cake))
        {
            SetPartyHatMaterials();
        }

        if (IsVariantType(TakeyVariantType.Cake))
        {
            SetCakeMaterial();
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

    public bool IsVariantType(params TakeyVariantType[] types)
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

    private void SetPartyHatMaterials()
    {
        if (_partyHatMaterialIndex < 0 || _partyHatMaterialIndex > PartyHatMaterials.Length - 1)
        {
            return;
        }

        Material material = PartyHatMaterials[_partyHatMaterialIndex];
        if (material == null) return;

        foreach (var partyHatObject in PartyHatObjects)
        {
            foreach (var meshRenderer in partyHatObject.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.sharedMaterial = material;
            }
        }
    }

    private void SetCakeMaterial()
    {
        if (_cakeMaterialIndex < 0 || _cakeMaterialIndex > CakeMaterials.Length - 1)
        {
            return;
        }

        Material material = CakeMaterials[_cakeMaterialIndex];
        if (material == null) return;

        CakeMeshRenderer.sharedMaterial = material;
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

            if (_voiceLineIndex == 2 && PlayerUtils.IsLocalPlayer(PlayerName.PsychoHypnotic))
            {
                HUDManager.Instance.DisplayTip("SellMyScrap", "This is NOT a reference to The Wolf of Wall Street");
            }

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
            Utils.CreateExplosion(transform.position, spawnExplosionEffect: true, damage: 100);
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

[Serializable]
public class TakeyVariant
{
    public TakeyVariantType Type;
    [Range(0, 500)]
    public int Weight = 25;
    public GameObject ModelObject;
    public Transform MouthTransform;
    public AudioClip[] BeforeEatSFX = [];
}
