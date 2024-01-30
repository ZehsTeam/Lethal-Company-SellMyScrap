using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    [HarmonyPatch("MicrophoneSpeak")]
    [HarmonyPrefix]
    static void MicrophoneSpeakPatch(ref DepositItemsDesk __instance, ref System.Random ___CompanyLevelRandom)
    {
        AudioClip clip = ((!(___CompanyLevelRandom.NextDouble() < 0.029999999329447746)) ? __instance.microphoneAudios[___CompanyLevelRandom.Next(0, __instance.microphoneAudios.Length)] : __instance.rareMicrophoneAudios[___CompanyLevelRandom.Next(0, __instance.rareMicrophoneAudios.Length)]);

        // Play audio clip at the desk
        __instance.speakerAudio.PlayOneShot(clip, 1f);

        // Play audio clip in the ship
        StartOfRound.Instance.speakerAudioSource.PlayOneShot(clip);
    }
}
