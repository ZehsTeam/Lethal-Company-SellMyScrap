using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

    [Header("Feels Variant")]
    public Animator FeelsAnimator = null;

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

        if (IsHostOrServer)
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
        if (Plugin.IsHostOrServer) return;

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
        if (PlayerUtils.IsLocalPlayerTakerst() && Utils.RandomPercent(75) && !(bool)ModpackSaveSystem.ReadValue("ForcedShowTakeyScrapEaterPeepoChickenVariant", false))
        {
            ModpackSaveSystem.WriteValue("ForcedShowTakeyScrapEaterPeepoChickenVariant", true);
            return GetVariantIndex(TakeyVariantType.PeepoChicken);
        }

        return Utils.GetRandomIndexFromWeightList(Variants.Select(_ => _.Weight).ToList());
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

        if (IsVariantType(TakeyVariantType.PeepoChicken))
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
        if (IsVariantType(TakeyVariantType.PeepoChicken))
        {
            ChickenAnimator.SetBool("Animate", false);
        }

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", true);
            DinkDonkAudio.Play();
        }

        if (IsVariantType(TakeyVariantType.Feels))
        {
            FeelsAnimator.SetBool("Animate", true);
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

        if (IsVariantType(TakeyVariantType.PeepoChicken))
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

        // Move targetScrap to mouthTransform over time.
        if (IsVariantType(TakeyVariantType.PeepoChicken))
        {
            yield return StartCoroutine(MoveTargetScrapToTargetTransformDelayed(mouthTransform, suckDuration - 0.1f, duration: 15f));
        }
        else
        {
            MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        }
        
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));

        if (IsVariantType(TakeyVariantType.DinkDonk))
        {
            DinkDonkAnimator.SetBool("Animate", false);
            DinkDonkAudio.Stop();
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(PlayOneShotSFX(voiceLineSFX, _voiceLineIndex));
        yield return new WaitForSeconds(1f);

        if (IsVariantType(TakeyVariantType.PeepoChicken))
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

public enum TakeyVariantType
{
    Default,
    Horny,
    Shady,
    Captain,
    Gamble,
    PeepoChicken,
    DinkDonk,
    FightClub,
    Cute,
    Feels,
    Stabby
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
