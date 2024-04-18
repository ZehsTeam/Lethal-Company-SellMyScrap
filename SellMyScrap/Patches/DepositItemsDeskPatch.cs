using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    private static DepositItemsDesk instance;
    public static DepositItemsDesk Instance
    {
        get
        {
            if (instance is null)
            {
                instance = Object.FindFirstObjectByType<DepositItemsDesk>();
            }

            return instance;
        }
    }

    private static System.Random companyLevelRandom = new System.Random();
    private static int clipIndex = -1;
    private static bool speakInShip = false;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void StartPatch(ref System.Random ___CompanyLevelRandom)
    {
        companyLevelRandom = ___CompanyLevelRandom;
    }

    [HarmonyPatch("SellItemsOnServer")]
    [HarmonyPrefix]
    static bool SellItemsOnServerPatch(ref DepositItemsDesk __instance)
    {
        if (__instance.itemsOnCounter.Count == 0)
        {
            return false;
        }

        if (Plugin.IsHostOrServer)
        {
            SetMicrophoneSpeakDataOnServer(speakInShip);
        }

        return true;
    }

    [HarmonyPatch("MicrophoneSpeak")]
    [HarmonyPrefix]
    static bool MicrophoneSpeakPatch(ref DepositItemsDesk __instance)
    {
        List<AudioClip> audioClips = [
            .. __instance.microphoneAudios,
            .. __instance.rareMicrophoneAudios
        ];

        if (clipIndex == -1)
        {
            clipIndex = GetRandomAudioClipIndex();
        }

        AudioClip audioClip = audioClips[clipIndex];

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(audioClip, 1f);

        // Play audio clip in the ship
        if (Plugin.Instance.ConfigManager.SpeakInShip && speakInShip)
        {
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(audioClip, 1f);
        }

        speakInShip = false;
        clipIndex = -1;

        return false;
    }

    private static int GetRandomAudioClipIndex()
    {
        if (companyLevelRandom.NextDouble() < 0.029999999329447746)
        {
            return companyLevelRandom.Next(0, Instance.microphoneAudios.Length);
        }

        return companyLevelRandom.Next(0, Instance.rareMicrophoneAudios.Length);
    }

    public static void SetMicrophoneSpeakDataOnClient(bool _speakInShip, int _clipIndex)
    {
        speakInShip = _speakInShip;
        clipIndex = _clipIndex;
    }

    private static void SetMicrophoneSpeakDataOnServer(bool _speakInShip)
    {
        speakInShip = _speakInShip;
        clipIndex = GetRandomAudioClipIndex();

        PluginNetworkBehaviour.Instance.SetMicrophoneSpeakDataClientRpc(speakInShip, clipIndex);
    }

    public static void SellItemsOnServer()
    {
        speakInShip = true;
        Instance.SellItemsOnServer();
    }

    public static void PlaceItemsOnCounter(List<GrabbableObject> grabbableObjects)
    {
        if (Instance is null) return;

        grabbableObjects.ForEach(PlaceItemOnCounter);
    }

    public static void PlaceItemOnCounter(GrabbableObject grabbableObject)
    {
        if (grabbableObject is null || Instance is null) return;
        if (Instance.itemsOnCounter.Contains(grabbableObject)) return;

        Instance.itemsOnCounter.Add(grabbableObject);

        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        Instance.itemsOnCounterNetworkObjects.Add(networkObject);

        grabbableObject.EnablePhysics(false);
        grabbableObject.EnableItemMeshes(false);

        grabbableObject.transform.SetParent(Instance.deskObjectsContainer.transform);
        grabbableObject.transform.localPosition = Vector3.zero;
    }
}
