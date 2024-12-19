using com.github.zehsteam.SellMyScrap.Data;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    private static AudioClip _cachedShipIntroSpeechSFX;

    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!NetworkUtils.IsServer) return;

        var networkHandlerHost = Object.Instantiate(Content.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Plugin.ConfigManager.TrySetCustomValues();

        RemoveMapPropsContainerForTesting();
    }

    private static void RemoveMapPropsContainerForTesting()
    {
        GameObject gameObject = GameObject.Find("Environment/MapPropsContainerForTesting");
        if (gameObject == null) return;

        gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(StartOfRound.firstDayAnimation))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void FirstDayAnimationPatchPrefix()
    {
        if (_cachedShipIntroSpeechSFX != null)
        {
            StartOfRound.Instance.shipIntroSpeechSFX = _cachedShipIntroSpeechSFX;
        }

        if (!PlayerUtils.IsLocalPlayer([PlayerName.CritHaxXoG, PlayerName.Takerst, PlayerName.PsychoHypnotic, PlayerName.IElucian, PlayerName.AGlitchedNpc, PlayerName.Lunxara, PlayerName.LustStings, PlayerName.Ariesgoddess168, PlayerName.ZombiesAteMyChannel, PlayerName.WolfsMyChocolate, PlayerName.Hiccubz]))
        {
            return;
        }

        if (ModpackSaveSystem.ReadValue("PlayedCustomIntroSpeech_2", false))
        {
            return;
        }

        ModpackSaveSystem.WriteValue("PlayedCustomIntroSpeech_2", true);

        _cachedShipIntroSpeechSFX = StartOfRound.Instance.shipIntroSpeechSFX;
        StartOfRound.Instance.shipIntroSpeechSFX = Content.BrainRotIntroSpeechSFX;
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    private static void OnClientConnectPatch(ref ulong clientId)
    {
        if (NetworkUtils.IsServer)
        {
            SyncedConfigEntryBase.SendConfigsToClient(clientId);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPrefix]
    private static void OnLocalDisconnectPatch()
    {
        Plugin.Instance.OnLocalDisconnect();
    }
}
