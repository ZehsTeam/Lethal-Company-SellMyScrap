using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class YippeeScrapEaterBehaviour : ScrapEaterExtraBehaviour
{
    [Header("Yippee")]
    [Space(3f)]
    public Animator animator = null;
    public AudioSource flyAudio = null;
    public AudioClip afterEatSFX = null;
    public float startFlySpeed = 1f;
    public float maxFlySpeed = 100f;
    public float flySpeedMultiplier = 10f;

    private float flySpeed = 0f;

    protected override IEnumerator StartAnimation()
    {
        // Move ScrapEater to startPosition
        SetAnimationFlying(true);
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        SetAnimationFlying(false);
        PlayOneShotSFX(landSFX, landIndex);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPositionWithEffects(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        MoveTargetScrapToTargetTransform(mouthTransform, suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);

        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));
        PlayOneShotSFX(afterEatSFX);
        yield return new WaitForSeconds(pauseDuration);

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPositionWithEffects(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        SetAnimationFlying(true);
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FlyAway(6f));
    }

    private void SetAnimationFlying(bool enabled)
    {
        animator.SetBool("Chase", enabled);

        if (enabled)
        {
            flyAudio.Play();
        }
        else
        {
            flyAudio.Stop();
        }
    }

    private IEnumerator MoveToPositionWithEffects(Vector3 from, Vector3 to, float duration)
    {
        StartCoroutine(SetAnimationVelocityX(true, speedMultiplier: 0.8f));
        yield return StartCoroutine(MoveToPosition(from, to, duration));
        StartCoroutine(SetAnimationVelocityX(false, speedMultiplier: 0.8f));
    }

    private IEnumerator SetAnimationVelocityX(bool isStarting, float duration = 0.15f, float speedMultiplier = 1f)
    {
        float from = isStarting ? 0f : 1f;
        float to = isStarting ? 1f : 0f;

        animator.SetFloat("WalkingSpeedMultiplier", speedMultiplier);
        animator.SetFloat("VelocityX", from);

        float timer = 0f;
        while (timer < duration)
        {
            float percent = (1f / duration) * timer;
            float value = from + (to - from) * percent;

            animator.SetFloat("VelocityX", value);

            yield return null;
            timer += Time.deltaTime;
        }

        animator.SetFloat("VelocityX", to);
    }

    private IEnumerator FlyAway(float duration)
    {
        flySpeed = startFlySpeed;
        float timer = 0f;

        while (timer < duration)
        {
            flySpeed += flySpeedMultiplier * Time.deltaTime;
            if (flySpeed > maxFlySpeed) flySpeed = maxFlySpeed;

            Vector3 position = transform.localPosition;
            position.y += flySpeed * Time.deltaTime;

            transform.localPosition = position;

            yield return null;
            timer += Time.deltaTime;
        }
    }
}
