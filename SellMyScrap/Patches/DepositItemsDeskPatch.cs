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
    public static DepositItemsDesk depositItemsDesk => Object.FindAnyObjectByType<DepositItemsDesk>();

    private static int clipIndex = -1;
    private static bool speakInShip = false;

    [HarmonyPatch("SellItemsOnServer")]
    [HarmonyPrefix]
    static bool SellItemsOnServerPatch(ref DepositItemsDesk __instance)
    {
        if (__instance.itemsOnCounter.Count == 0)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch("MicrophoneSpeak")]
    [HarmonyPrefix]
    static bool MicrophoneSpeakPatch(ref DepositItemsDesk __instance, ref System.Random ___CompanyLevelRandom)
    {
        List<AudioClip> audioClips = __instance.microphoneAudios.ToList();
        audioClips.AddRange(__instance.rareMicrophoneAudios);

        if (clipIndex == -1)
        {
            clipIndex = GetRandomAudioClipIndex(__instance, ___CompanyLevelRandom);
        }

        if (SellMyScrapBase.IsHostOrServer)
        {
            PluginNetworkBehaviour.Instance.SetDepositItemsDeskAudioClipClientRpc(clipIndex);
        }

        AudioClip audioClip = audioClips[clipIndex];

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(audioClip, 1f);

        // Play audio clip in the ship
        if (SellMyScrapBase.Instance.ConfigManager.SpeakInShip && speakInShip)
        {
            speakInShip = false;
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(audioClip, 1f);
        }

        clipIndex = -1;

        return false;
    }

    private static int GetRandomAudioClipIndex(DepositItemsDesk __instance, System.Random ___CompanyLevelRandom)
    {
        int index = 0;

        if (___CompanyLevelRandom.NextDouble() < 0.029999999329447746)
        {
            index = ___CompanyLevelRandom.Next(0, __instance.microphoneAudios.Length);
        }
        else
        {
            index = ___CompanyLevelRandom.Next(0, __instance.rareMicrophoneAudios.Length);
        }

        return index;
    }

    public static void SetAudioClip(int index)
    {
        clipIndex = index;
    }

    public static void EnableSpeakInShip()
    {
        speakInShip = true;
    }

    public static void SellItemsOnServer()
    {
        PluginNetworkBehaviour.Instance.EnableSpeakInShipClientRpc();
        depositItemsDesk.SellItemsOnServer();
    }

    public static void PlaceItemsOnCounter(List<GrabbableObject> grabbableObjects)
    {
        if (depositItemsDesk == null) return;

        grabbableObjects.ForEach(PlaceItemOnCounter);
    }

    public static void PlaceItemOnCounter(GrabbableObject grabbableObject)
    {
        if (grabbableObject == null || depositItemsDesk == null) return;
        if (depositItemsDesk.itemsOnCounter.Contains(grabbableObject)) return;

        depositItemsDesk.itemsOnCounter.Add(grabbableObject);

        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        depositItemsDesk.itemsOnCounterNetworkObjects.Add(networkObject);

        grabbableObject.EnablePhysics(false);
        grabbableObject.EnableItemMeshes(false);

        grabbableObject.transform.SetParent(depositItemsDesk.deskObjectsContainer.transform);
        grabbableObject.transform.localPosition = Vector3.zero;
    }
}
