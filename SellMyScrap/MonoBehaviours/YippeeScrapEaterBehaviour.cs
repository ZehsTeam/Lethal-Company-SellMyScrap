using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class YippeeScrapEaterBehaviour : ScrapEaterBehaviour
{
    [Header("Yippee")]
    [Space(3f)]
    public Animator animator;
    public AudioSource flyAudio;
    public AudioClip yippeeSFX;
    public float startFlySpeed = 1f;
    public float maxFlySpeed = 100f;
    public float flySpeedMultiplier = 10f;

    private float flySpeed = 0f;

    public override IEnumerator StartAnimation()
    {
        Vector3 skyStartPosition = startPosition;
        skyStartPosition.y += 150f;
        transform.localPosition = skyStartPosition;

        SetAnimationFlying(true);
        yield return StartCoroutine(MoveToPosition(skyStartPosition, startPosition, 3f));
        SetAnimationFlying(false);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveToPositionWithEffects(startPosition, endPosition, slideDuration));
        yield return new WaitForSeconds(pauseDuration);

        SuckScrapToSell();
        yield return new WaitForSeconds(suckDuration);

        PlaySFX(eatSFX);
        float eatSFXLength = eatSFX == null ? 0 : eatSFX.length;
        yield return new WaitForSeconds(eatSFXLength);

        PlaySFX(yippeeSFX);

        yield return new WaitForSeconds(pauseDuration);

        yield return StartCoroutine(MoveToPositionWithEffects(endPosition, startPosition, slideDuration));
        yield return new WaitForSeconds(1f);

        SetAnimationFlying(true);
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FlyAway(6f));

        DisableModelObject();
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
        StartCoroutine(SetAnimationVelocityX(true));
        //StartCoroutine(PlayWalkClips(duration));
        yield return StartCoroutine(MoveToPosition(from, to, duration));
        StartCoroutine(SetAnimationVelocityX(false));
    }

    private IEnumerator SetAnimationVelocityX(bool isStarting, float duration = 0.15f)
    {
        float from = isStarting ? 0f : 1f;
        float to = isStarting ? 1f : 0f;

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
            Vector3 position = transform.localPosition;

            flySpeed += flySpeedMultiplier * Time.deltaTime;
            if (flySpeed > maxFlySpeed) flySpeed = maxFlySpeed;

            position.y += flySpeed * Time.deltaTime;

            transform.localPosition = position;

            yield return null;
            timer += Time.deltaTime;
        }
    }
}
