using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace com.github.zehsteam.SellMyScrap;

internal class Octolar
{
    private static Vector3 startPosition = new Vector3(-8.9f, 0f, -3.2f);
    private static Vector3 endPosition = new Vector3(-8.9f, 0f, -6.8f);
    private static Vector3 rotationOffset = new Vector3(0f, 90f, 0f);
    private static float size = 4.5f;

    private static GameObject gameObject;
    private static MeshRenderer meshRenderer;
    public static Transform mouth;
    private static AudioSource audioSource;

    private static System.Random random = new System.Random();
    private static float timer = 0;

    private static List<GrabbableObject> scrapToSuck;

    public static void SetScrapToSuck(List<GrabbableObject> scrap)
    {
        scrapToSuck = scrap;
    }

    public static void Show()
    {
        Spawn();

        StartOfRound.Instance.StopCoroutine("MoveToPosition");

        Material material = random.Next(1, 100) <= 50 ? Assets.octolarNormalMaterial : Assets.octolarSusMaterial;
        meshRenderer.material = material;

        gameObject.transform.localPosition = startPosition;

        gameObject.SetActive(true);

        StartOfRound.Instance.StartCoroutine(PlayAnimation());
    }

    private static void Spawn()
    {
        if (gameObject != null) return;

        gameObject = Object.Instantiate(Assets.octolarPrefab, startPosition, Quaternion.identity);
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        meshRenderer = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();
        mouth = gameObject.transform.GetChild(1);
        audioSource = gameObject.transform.GetChild(3).GetComponent<AudioSource>();

        gameObject.transform.SetParent(ScrapHelper.HangarShip.transform);
        gameObject.transform.localPosition = startPosition;
        gameObject.transform.Rotate(rotationOffset, Space.Self);
        gameObject.transform.localScale = Vector3.one * size;

        gameObject.SetActive(false);
    }

    private static IEnumerator PlayAnimation()
    {
        float slideTime = 3f;

        yield return new WaitForSeconds(2f);

        audioSource.PlayOneShot(Assets.squidwardWalkSound);

        timer = 0f;
        while (timer < slideTime)
        {
            float percent = (1f / slideTime) * timer;
            gameObject.transform.localPosition = startPosition + (endPosition - startPosition) * percent;

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        gameObject.transform.localPosition = endPosition;

        yield return new WaitForSeconds(2f);

        meshRenderer.material = Assets.octolarSuckMaterial;
        SuckScrapToSell();

        yield return new WaitForSeconds(3.5f);

        meshRenderer.material = Assets.octolarNormalMaterial;
        audioSource.PlayOneShot(Assets.minecraftEatSound);

        yield return new WaitForSeconds(2f);

        audioSource.PlayOneShot(Assets.squidwardWalkSound);

        timer = 0f;
        while (timer < slideTime)
        {
            float percent = (1f / slideTime) * timer;
            gameObject.transform.localPosition = endPosition + (startPosition - endPosition) * percent;

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        gameObject.transform.localPosition = startPosition;

        if (SellMyScrapBase.IsHostOrServer)
        {
            PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();
        }

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);

        if (SellMyScrapBase.IsHostOrServer)
        {
            DepositItemsDeskPatch.DepositItemsDesk.SellItemsOnServer();
        }
    }

    private static void SuckScrapToSell()
    {
        if (scrapToSuck == null)
        {
            SellMyScrapBase.mls.LogWarning("Warning: no scrap found to suck :c");
            return;
        }

        scrapToSuck.ForEach(item =>
        {
            item.gameObject.AddComponent<SuckBehaviour>();
        });
    }
}
