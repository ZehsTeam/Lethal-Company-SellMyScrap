﻿using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal static class DepositItemsDeskPatch
{
    public static DepositItemsDesk Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<DepositItemsDesk>();
            }

            return _instance;
        }
    }

    private static DepositItemsDesk _instance;

    private static int _clipIndex = -1;
    private static bool _speakInShip = false;

    [HarmonyPatch(nameof(DepositItemsDesk.SellItemsOnServer))]
    [HarmonyPrefix]
    private static bool SellItemsOnServerPatch(ref DepositItemsDesk __instance)
    {
        if (__instance.itemsOnCounter.Count == 0)
        {
            return false;
        }

        if (NetworkUtils.IsServer)
        {
            SetMicrophoneSpeakDataOnServer(_speakInShip);
        }

        return true;
    }

    [HarmonyPatch(nameof(DepositItemsDesk.MicrophoneSpeak))]
    [HarmonyPrefix]
    private static bool MicrophoneSpeakPatch(ref DepositItemsDesk __instance)
    {
        List<AudioClip> audioClips = [
            .. __instance.microphoneAudios,
            .. __instance.rareMicrophoneAudios
        ];

        if (_clipIndex == -1)
        {
            _clipIndex = GetRandomAudioClipIndex();
        }

        AudioClip audioClip = audioClips[_clipIndex];

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(audioClip, 1f);

        // Play audio clip in the ship
        if (Plugin.ConfigManager.SpeakInShip.Value && _speakInShip)
        {
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(audioClip, 1f);
        }

        _speakInShip = false;
        _clipIndex = -1;

        return false;
    }

    private static int GetRandomAudioClipIndex()
    {
        if (Utils.RandomPercent(Plugin.ConfigManager.RareVoiceLineChance.Value))
        {
            return Random.Range(0, Instance.rareMicrophoneAudios.Length) + Instance.microphoneAudios.Length;
        }

        return Random.Range(0, Instance.microphoneAudios.Length);
    }

    public static void SetMicrophoneSpeakDataOnClient(bool speakInShip, int clipIndex)
    {
        _speakInShip = speakInShip;
        _clipIndex = clipIndex;
    }

    private static void SetMicrophoneSpeakDataOnServer(bool speakInShip)
    {
        _speakInShip = speakInShip;
        _clipIndex = GetRandomAudioClipIndex();

        PluginNetworkBehaviour.Instance.SetMicrophoneSpeakDataClientRpc(speakInShip, _clipIndex);
    }

    public static void SellItemsOnServer()
    {
        _speakInShip = true;
        Instance.SellItemsOnServer();
    }

    public static void PlaceItemsOnCounter(List<GrabbableObject> grabbableObjects)
    {
        if (Instance == null) return;

        grabbableObjects.ForEach(PlaceItemOnCounter);
    }

    public static void PlaceItemOnCounter(GrabbableObject grabbableObject)
    {
        if (grabbableObject == null || Instance == null) return;
        if (Instance.itemsOnCounter.Contains(grabbableObject)) return;

        Instance.itemsOnCounter.Add(grabbableObject);

        NetworkObject networkObject = grabbableObject.gameObject.GetComponent<NetworkObject>();
        Instance.itemsOnCounterNetworkObjects.Add(networkObject);

        grabbableObject.EnablePhysics(false);
        grabbableObject.EnableItemMeshes(false);

        grabbableObject.transform.SetParent(Instance.deskObjectsContainer.transform);
        grabbableObject.transform.localPosition = Vector3.zero;
    }

    public static void PlaceRagdollOnCounter(RagdollGrabbableObject ragdollGrabbableObject)
    {
        if (ragdollGrabbableObject == null || Instance == null) return;
        if (Instance.itemsOnCounter.Contains(ragdollGrabbableObject)) return;

        Instance.itemsOnCounter.Add(ragdollGrabbableObject);

        NetworkObject networkObject = ragdollGrabbableObject.gameObject.GetComponent<NetworkObject>();
        Instance.itemsOnCounterNetworkObjects.Add(networkObject);

        ragdollGrabbableObject.EnablePhysics(false);
        ragdollGrabbableObject.EnableItemMeshes(false);

        Transform ragdollTransform = ragdollGrabbableObject.ragdoll.transform;

        foreach (var renderer in ragdollTransform.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }

        foreach (var renderer in ragdollTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.enabled = false;
        }

        ragdollTransform.SetParent(Instance.deskObjectsContainer.transform);
        ragdollTransform.localPosition = Vector3.zero;
    }
}
