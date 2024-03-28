using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.MonoBehaviours;

public class ScrapEaterBehaviour : NetworkBehaviour
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
    public float suckDuration = 3.5f;
    public float pauseDuration = 2f;

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

    public virtual void Start()
    {
        transform.SetParent(ScrapHelper.HangarShip.transform);

        transform.localPosition = startPosition;
        transform.localRotation = Quaternion.identity;
        transform.Rotate(rotationOffset, Space.Self);

        if (SellMyScrapBase.IsHostOrServer && slideMaterialVariants.Length > 0)
        {
            SetSlideMaterialVariantClientRpc(Random.Range(0, slideMaterialVariants.Length));
        }
    }

    public override void OnNetworkSpawn()
    {
        if (SellMyScrapBase.IsHostOrServer)
        {
            StartEventClientRpc();
        }
    }

    [ClientRpc]
    public void SetScrapToSuckClientRpc(string networkObjectIdsString)
    {
        scrapToSuck = NetworkUtils.GetGrabbableObjects(networkObjectIdsString);

        scrapToSuck.ForEach(item =>
        {
            item.grabbable = false;
        });
    }

    [ClientRpc]
    public void SetSlideMaterialVariantClientRpc(int index)
    {
        SetSlideMaterialVariant(index);
    }

    [ClientRpc]
    public void StartEventClientRpc()
    {
        StartCoroutine(StartEvent());
    }

    public virtual IEnumerator StartEvent()
    {
        yield return StartCoroutine(StartAnimation());

        EnableSpeakInShipOnServer();
        yield return new WaitForSeconds(0.5f);

        SellItemsOnServer();

        if (SellMyScrapBase.IsHostOrServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    public virtual IEnumerator StartAnimation()
    {
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
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveToPosition(startPosition, skyStartPosition, 2f));

        meshRenderer.gameObject.SetActive(false);
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
            Vector3 position = from + (to - from) * percent;

            transform.localPosition = position;

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
        if (index < 0 || index > slideMaterialVariants.Length - 1) return;

        SetMaterial(slideMaterialVariants[index]);
    }
}
