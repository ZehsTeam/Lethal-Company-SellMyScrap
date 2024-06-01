using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class ZombiesScrapEater : ScrapEaterExtraBehaviour
{
    private enum ZombieState
    {
        Happy,
        Scared,
        Angry,
        Drunk,
        Heart
    }

    [Space(20f)]
    [Header("Zombies")]
    [Space(5f)]
    public Animator animator = null;
    public GameObject happyObject = null;
    public GameObject scaredObject = null;
    public GameObject angryObject = null;
    public GameObject drunkObject = null;
    public GameObject heartObject = null;
    public AudioClip[] happySFX = new AudioClip[0];
    public AudioClip[] scaredSFX = new AudioClip[0];
    public AudioClip[] angrySFX = new AudioClip[0];
    public AudioClip[] drunkSFX = new AudioClip[0];
    public AudioClip[] heartSFX = new AudioClip[0];
    public AudioClip fallDamageSFX = null;
    public AudioClip dieSFX = null;
    public AudioClip[] hurtSFX = new AudioClip[0];
    public AudioClip[] idleSFX = new AudioClip[0];
    public AudioClip[] stepSFX = new AudioClip[0];
    public float inbetweenStepDuration = 0.1f;

    private ZombieState _zombieState = ZombieState.Happy;
    private int _stateSFXIndex = 0;
    private int _hurtSFXIndex = 0;
    private int _idleSFXIndex = 0;
    private bool _playDieAnimation = false;

    private Coroutine _walkAudioCO = null;

    protected override void Start()
    {
        if (IsHostOrServer)
        {
            _zombieState = (ZombieState)Random.Range(0, 5);
            _stateSFXIndex = GetStateSFXIndex();
            _hurtSFXIndex = Random.Range(0, hurtSFX.Length);
            _idleSFXIndex = Random.Range(0, idleSFX.Length);
            _playDieAnimation = Utils.RandomPercent(80);
            SetDataClientRpc(_zombieState, _stateSFXIndex, _hurtSFXIndex, _idleSFXIndex, _playDieAnimation);
            UpdateModel();
        }

        base.Start();
    }

    [ClientRpc]
    private void SetDataClientRpc(ZombieState state, int stateSFXIndex, int hurtSFXIndex, int idleSFXIndex, bool playDieAnimation)
    {
        if (IsHostOrServer) return;

        _zombieState = state;
        _stateSFXIndex = stateSFXIndex;
        _hurtSFXIndex = hurtSFXIndex;
        _idleSFXIndex = idleSFXIndex;
        _playDieAnimation = playDieAnimation;
        UpdateModel();
    }

    protected override IEnumerator StartAnimation()
    {
        if (_playDieAnimation)
        {
            yield return StartCoroutine(PlayZombieDeathAnimation());
        }

        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(fallDamageSFX);
        PlayOneShotSFX(hurtSFX, _hurtSFXIndex);
        ShakeCamera();

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        StartWalkAudio();
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopWalkAudio();

        yield return new WaitForSeconds(pauseDuration / 2f);
        yield return new WaitForSeconds(PlayOneShotSFX(idleSFX, _idleSFXIndex));
        yield return new WaitForSeconds(pauseDuration / 2f);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        yield return new WaitForSeconds(pauseDuration / 2f);
        yield return new WaitForSeconds(PlayStateSFX(_stateSFXIndex));
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

    private IEnumerator PlayZombieDeathAnimation()
    {
        Vector3 skyPos = new Vector3(endPosition.x, spawnPosition.y, endPosition.z);
        yield return StartCoroutine(MoveToPosition(skyPos, endPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        PlayOneShotSFX(fallDamageSFX);
        ShakeCamera();
        yield return new WaitForSeconds(0.2f);
        PlayOneShotSFX(dieSFX);
        PlayDeathAnimation();
        yield return new WaitForSeconds(2f);
        transform.position = spawnPosition;
        ShowModel();
        PlayIdleAnimation();
    }

    private IEnumerator WalkAudio()
    {
        while (true)
        {
            PlayOneShotSFX(movementAudio, stepSFX[Random.Range(0, stepSFX.Length)]);

            yield return new WaitForSeconds(inbetweenStepDuration);
        }
    }

    private void StartWalkAudio()
    {
        StopWalkAudio();
        _walkAudioCO = StartCoroutine(WalkAudio());
    }

    private void StopWalkAudio()
    {
        if (_walkAudioCO != null)
        {
            StopCoroutine(_walkAudioCO);
        }
    }

    private void PlayDeathAnimation()
    {
        animator.Play("Die");
    }

    private void PlayIdleAnimation()
    {
        animator.Play("Idle");
    }

    private void ShowModel()
    {
        modelObject.SetActive(true);
    }

    private void UpdateModel()
    {
        happyObject.SetActive(_zombieState == ZombieState.Happy);
        scaredObject.SetActive(_zombieState == ZombieState.Scared);
        angryObject.SetActive(_zombieState == ZombieState.Angry);
        drunkObject.SetActive(_zombieState == ZombieState.Drunk);
        heartObject.SetActive(_zombieState == ZombieState.Heart);

        UpdateMouthTransform();
    }

    private void UpdateMouthTransform()
    {
        GameObject activeObject = null;

        for (int i = 0; i < modelObject.transform.childCount; i++)
        {
            GameObject obj = modelObject.transform.GetChild(i).gameObject;

            if (obj.activeSelf)
            {
                activeObject = obj;
                break;
            }
        }

        mouthTransform = activeObject.transform.Find("Mouth");
    }

    private int GetStateSFXIndex()
    {
        switch (_zombieState)
        {
            case ZombieState.Happy:
                return Random.Range(0, happySFX.Length);
            case ZombieState.Scared:
                return Random.Range(0, scaredSFX.Length);
            case ZombieState.Angry:
                return Random.Range(0, angrySFX.Length);
            case ZombieState.Drunk:
                return Random.Range(0, drunkSFX.Length);
            case ZombieState.Heart:
                return Random.Range(0, heartSFX.Length);
        }

        return 0;
    }

    private float PlayStateSFX(int index)
    {
        switch (_zombieState)
        {
            case ZombieState.Happy:
                return PlayOneShotSFX(happySFX, index);
            case ZombieState.Scared:
                return PlayOneShotSFX(scaredSFX, index);
            case ZombieState.Angry:
                return PlayOneShotSFX(angrySFX, index);
            case ZombieState.Drunk:
                return PlayOneShotSFX(drunkSFX, index);
            case ZombieState.Heart:
                return PlayOneShotSFX(heartSFX, index);
        }

        return 0f;
    }
}
