using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal static class DepositItemsDeskPatch
{
    public static int ClipIndex = -1;
    public static bool SpeakInShip = false;

    [HarmonyPatch(nameof(DepositItemsDesk.Start))]
    [HarmonyPrefix]
    private static void StartPatch(ref DepositItemsDesk __instance)
    {
        DepositItemsDeskHelper.SetInstance(__instance);
    }

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
            SetMicrophoneSpeakData_Server(SpeakInShip);
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

        if (ClipIndex == -1)
        {
            ClipIndex = GetRandomAudioClipIndex();
        }

        AudioClip audioClip = audioClips[ClipIndex];

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(audioClip, 1f);

        // Play audio clip in the ship
        if (SpeakInShip && ConfigManager.SpeakInShip.Value)
        {
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(audioClip, 1f);
        }

        SpeakInShip = false;
        ClipIndex = -1;

        return false;
    }

    private static int GetRandomAudioClipIndex()
    {
        if (Utils.RollPercentChance(ConfigManager.RareVoiceLineChance.Value))
        {
            return Random.Range(0, DepositItemsDeskHelper.Instance.rareMicrophoneAudios.Length) + DepositItemsDeskHelper.Instance.microphoneAudios.Length;
        }

        return Random.Range(0, DepositItemsDeskHelper.Instance.microphoneAudios.Length);
    }

    public static void SetMicrophoneSpeakData_LocalClient(bool speakInShip, int clipIndex)
    {
        SpeakInShip = speakInShip;
        ClipIndex = clipIndex;
    }

    private static void SetMicrophoneSpeakData_Server(bool speakInShip)
    {
        SpeakInShip = speakInShip;
        ClipIndex = GetRandomAudioClipIndex();

        PluginNetworkBehaviour.Instance.SetMicrophoneSpeakDataClientRpc(speakInShip, ClipIndex);
    }
}
