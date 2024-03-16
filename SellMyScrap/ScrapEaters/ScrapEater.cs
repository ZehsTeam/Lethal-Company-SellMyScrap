using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.ScrapEaters;

public class ScrapEater
{
    protected GameObject spawnPrefab;

    protected Vector3 startPosition = new Vector3(-8.9f, 0f, -3.2f);
    protected Vector3 endPosition = new Vector3(-8.9f, 0f, -6.8f);
    protected Vector3 rotationOffset = new Vector3(0f, 90f, 0f);
    protected float size = 4.5f;

    protected GameObject spawnedObject;
    public Transform mouthTransform;
    protected AudioSource audioSource;
    protected MeshRenderer meshRenderer;

    protected float slideDuration = 3f;
    public float suckDuration = 3.5f;
    protected float waitDuration = 2f;

    protected AudioClip slideSFX;
    protected AudioClip eatSFX;

    protected Material defaultMaterial;
    protected Material[] slideMaterials;
    protected Material suckMaterial;

    public Coroutine startCoroutine;
    public Coroutine startAnimationCoroutine;

    protected float timer;

    public virtual int GetSpawnWeight()
    {
        return 1;
    }

    public int GetRandomSlideMaterialIndex()
    {
        if (slideMaterials == null) return 0;

        return Random.Range(0, slideMaterials.Length);
    }

    public virtual IEnumerator Start(int slideMaterialIndex)
    {
        if (spawnPrefab == null) yield break;

        if (startCoroutine != null)
        {
            StartOfRound.Instance.StopCoroutine(startCoroutine);
        }

        if (startAnimationCoroutine != null)
        {
            StartOfRound.Instance.StopCoroutine(startAnimationCoroutine);
        }

        if (spawnedObject != null)
        {
            Object.Destroy(spawnedObject);
        }

        spawnedObject = Object.Instantiate(spawnPrefab, startPosition, Quaternion.identity, ScrapHelper.HangarShip.transform);
        spawnedObject.hideFlags = HideFlags.HideAndDontSave;

        spawnedObject.transform.localPosition = startPosition;
        spawnedObject.transform.localRotation = Quaternion.identity;
        spawnedObject.transform.Rotate(rotationOffset, Space.Self);
        spawnedObject.transform.localScale = Vector3.one * size;

        try
        {
            mouthTransform = spawnedObject.transform.Find("Mouth");
            meshRenderer = spawnedObject.transform.Find("Model").GetComponent<MeshRenderer>();
            audioSource = spawnedObject.transform.Find("SoundEffects").GetComponent<AudioSource>();
        }
        catch (System.Exception e)
        {
            SellMyScrapBase.mls.LogError($"Error: failed to find object/component on spawnPrefab \"{spawnPrefab.name}\"\n\n{e}");
        }

        yield return startAnimationCoroutine = StartOfRound.Instance.StartCoroutine(StartAnimation(slideMaterialIndex));

        if (SellMyScrapBase.IsHostOrServer)
        {
            PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();
        }

        yield return new WaitForSeconds(0.5f);

        if (SellMyScrapBase.IsHostOrServer)
        {
            DepositItemsDeskPatch.DepositItemsDesk.SellItemsOnServer();
        }

        if (ScrapEaterManager.activeScrapEater == this)
        {
            ScrapEaterManager.activeScrapEater = null;
        }

        Object.Destroy(spawnedObject);
    }

    protected virtual IEnumerator StartAnimation(int slideMaterialIndex)
    {
        if (meshRenderer != null && slideMaterials != null)
        {
            meshRenderer.material = slideMaterials[slideMaterialIndex];
        }

        yield return new WaitForSeconds(2f);

        if (audioSource != null && slideSFX != null)
        {
            audioSource.PlayOneShot(slideSFX);
        }

        timer = 0f;
        while (timer < slideDuration)
        {
            float percent = (1f / slideDuration) * timer;
            spawnedObject.transform.localPosition = startPosition + (endPosition - startPosition) * percent;

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        spawnedObject.transform.localPosition = endPosition;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        yield return new WaitForSeconds(waitDuration);

        if (meshRenderer != null && suckMaterial != null)
        {
            meshRenderer.material = suckMaterial;
        }
        
        SuckScrapToSell();

        yield return new WaitForSeconds(suckDuration);

        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }

        if (audioSource != null && eatSFX != null)
        {
            audioSource.PlayOneShot(eatSFX);
        }

        yield return new WaitForSeconds(waitDuration);

        if (audioSource != null && slideSFX != null)
        {
            audioSource.PlayOneShot(slideSFX);
        }

        timer = 0f;
        while (timer < slideDuration)
        {
            float percent = (1f / slideDuration) * timer;
            spawnedObject.transform.localPosition = endPosition + (startPosition - endPosition) * percent;

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        spawnedObject.transform.localPosition = startPosition;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    protected virtual void SuckScrapToSell()
    {
        if (ScrapEaterManager.scrapToSuck == null)
        {
            SellMyScrapBase.mls.LogWarning("Warning: no scrap found to suck :c");
            return;
        }

        ScrapEaterManager.scrapToSuck.ForEach(item =>
        {
            item.gameObject.AddComponent<SuckBehaviour>();
        });
    }
}
