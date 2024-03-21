using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

internal class TakeyScrapEaterBehaviour : ScrapEaterBehaviour
{
    [Header("Takey")]
    [Space(3f)]
    public GameObject jetpackObject;
    public GameObject flameEffectsObject;
    public AudioSource jetpackAudio;
    public AudioSource jetpackThrustAudio;
    public AudioSource jetpackBeepAudio;
    public AudioClip jetpackThrustStartSFX;
    public AudioClip liftOffSFX;
    public float startFlySpeed = 0.5f;
    public float maxFlySpeed = 100f;
    public float flySpeedMultiplier = 5f;

    private float flySpeed;

    protected override void Start()
    {
        base.Start();

        jetpackObject.SetActive(false);
        flameEffectsObject.SetActive(false);
    }

    public override IEnumerator StartAnimation(int slideMaterialVariant)
    {
        SetSlideMaterialVariant(slideMaterialVariant);

        Vector3 skyStartPosition = startPosition;
        skyStartPosition.y += 150f;

        yield return StartCoroutine(MoveToPosition(skyStartPosition, startPosition, 2f));

        yield return new WaitForSeconds(1f);

        PlaySFX(slideSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, slideDuration));
        StopSFX();
        yield return new WaitForSeconds(pauseDuration);

        SetMaterial(suckMaterial);
        SuckScrapToSell();
        yield return new WaitForSeconds(suckDuration);

        SetMaterial(normalMaterial);
        PlaySFX(eatSFX);
        yield return new WaitForSeconds(pauseDuration);

        PlaySFX(slideSFX);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, slideDuration));
        StopSFX();

        EnableSpeakInShipOnServer();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(JetpackFly(7.5f));
        SellItemsOnServer();

        Destroy(gameObject);
    }

    private IEnumerator JetpackFly(float duration)
    {
        jetpackObject.SetActive(true);

        PlaySFX(liftOffSFX);
        jetpackAudio.PlayOneShot(jetpackThrustStartSFX);
        jetpackBeepAudio.Play();

        yield return new WaitForSeconds(0.3f);

        flameEffectsObject.SetActive(true);
        jetpackThrustAudio.Play();

        flySpeed = startFlySpeed;
        float timer = 0f;

        while (timer <= duration)
        {
            Vector3 position = transform.localPosition;

            flySpeed += flySpeedMultiplier * Time.deltaTime;
            if (flySpeed > maxFlySpeed) flySpeed = maxFlySpeed;

            position.y += flySpeed * Time.deltaTime;

            transform.localPosition = position;

            yield return null;
            timer += Time.deltaTime;
        }

        StopSFX();
        jetpackBeepAudio.Stop();
        jetpackThrustAudio.Stop();
    }
}
