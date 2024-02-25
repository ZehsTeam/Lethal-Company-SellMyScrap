using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    private static DepositItemsDesk depositItemsDesk;

    public static DepositItemsDesk DepositItemsDesk
    {
        get
        {
            if (depositItemsDesk == null)
            {
                depositItemsDesk = Object.FindAnyObjectByType<DepositItemsDesk>();
            }

            return depositItemsDesk;
        }
    }

    public static bool enableSpeakInShip = false;

    [HarmonyPatch("MicrophoneSpeak")]
    [HarmonyPrefix]
    static void MicrophoneSpeakPatch(ref DepositItemsDesk __instance, ref System.Random ___CompanyLevelRandom)
    {
        AudioClip clip = ((!(___CompanyLevelRandom.NextDouble() < 0.029999999329447746)) ? __instance.microphoneAudios[___CompanyLevelRandom.Next(0, __instance.microphoneAudios.Length)] : __instance.rareMicrophoneAudios[___CompanyLevelRandom.Next(0, __instance.rareMicrophoneAudios.Length)]);

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(clip, 1f);

        // Play audio clip in the ship
        if (SellMyScrapBase.Instance.ConfigManager.SpeakInShip && enableSpeakInShip)
        {
            enableSpeakInShip = false;

            StartOfRound.Instance.speakerAudioSource.PlayOneShot(clip);
        }
    }

    public static void PlaceItemsOnCounter(List<GrabbableObject> grabbableObjects)
    {
        if (DepositItemsDesk == null) return;

        grabbableObjects.ForEach(PlaceItemOnCounter);
    }

    public static void PlaceItemOnCounter(GrabbableObject grabbableObject)
    {
        if (DepositItemsDesk == null) return;

        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        if (DepositItemsDesk.itemsOnCounter.Contains(grabbableObject)) return;

        if (SellMyScrapBase.IsHostOrServer)
        {
            DepositItemsDesk.itemsOnCounterNetworkObjects.Add(networkObject);
            DepositItemsDesk.itemsOnCounter.Add(grabbableObject);
        }

        Transform parent = DepositItemsDesk.deskObjectsContainer.transform;
        grabbableObject.transform.SetParent(parent, true);
    }
}
