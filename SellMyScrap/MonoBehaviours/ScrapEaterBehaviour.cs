using com.github.zehsteam.SellMyScrap.Patches;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class ScrapEaterBehaviour : MonoBehaviour
{
    public Vector3 startPosition = new Vector3(-8.9f, 0f, -3.2f);
    public Vector3 endPosition = new Vector3(-8.9f, 0f, -6.8f);
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    public Transform mouthTransform;
    public MeshRenderer meshRenderer;
    public AudioSource audioSource;

    [Header("Durations")]
    [Space(3f)]
    public float slideDuration = 4f;
    public float suckDuration = 3f;
    public float pauseDuration = 3f;

    [Header("Materials")]
    [Space(3f)]
    public Material normalMaterial;
    public Material suckMaterial;
    public Material[] slideMaterialVariants;

    [Header("SFX")]
    [Space(3f)]
    public AudioClip slideSFX;
    public AudioClip eatSFX;

    private List<GrabbableObject> scrapToSuck;

    protected virtual void Start()
    {
        transform.localPosition = startPosition;
        transform.localRotation = Quaternion.identity;
        transform.Rotate(rotationOffset, Space.Self);

        scrapToSuck = ScrapEaterManager.scrapToSuck;
    }

    public virtual IEnumerator StartAnimation(int slideMaterialVariant)
    {
        SetSlideMaterialVariant(slideMaterialVariant);

        Vector3 skyStartPosition = startPosition;
        skyStartPosition.y += 150f;
        transform.localPosition = skyStartPosition;

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

        yield return StartCoroutine(MoveToPosition(startPosition, skyStartPosition, 2f));
        SellItemsOnServer();

        Destroy(gameObject);
    }

    protected virtual void SuckScrapToSell()
    {
        if (scrapToSuck == null)
        {
            SellMyScrapBase.mls.LogWarning("Warning: no scrap found to suck :c");
            return;
        }

        scrapToSuck.ForEach(item =>
        {
            if (item == null) return;

            SuckBehaviour suckBehaviour = item.gameObject.AddComponent<SuckBehaviour>();
            suckBehaviour.StartCoroutine(suckBehaviour.StartAnimation(mouthTransform, suckDuration - 0.2f));
        });
    }

    protected virtual void EnableSpeakInShipOnServer()
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();
    }

    protected virtual void SellItemsOnServer()
    {
        if (!SellMyScrapBase.IsHostOrServer) return;

        DepositItemsDeskPatch.DepositItemsDesk.SellItemsOnServer();
    }

    protected IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;

        while (timer <= duration)
        {
            float percent = (1f / duration) * timer;

            transform.localPosition = from + (to - from) * percent;

            yield return null;
            timer += Time.deltaTime;
        }

        transform.localPosition = to;
    }

    protected void PlaySFX(AudioClip audioClip, float volumeScale = 1f)
    {
        if (audioSource == null || audioClip == null) return;

        audioSource.PlayOneShot(audioClip, volumeScale);
    }

    protected void StopSFX()
    {
        if (audioSource == null) return;

        audioSource.Stop();
    }

    protected void SetMaterial(Material material)
    {
        if (meshRenderer == null || material == null) return;

        meshRenderer.material = material;
    }

    protected void SetSlideMaterialVariant(int index)
    {
        if (index > slideMaterialVariants.Length - 1) return;

        SetMaterial(slideMaterialVariants[index]);
    }
}
