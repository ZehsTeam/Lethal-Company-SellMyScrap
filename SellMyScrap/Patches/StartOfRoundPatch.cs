using com.github.zehsteam.SellMyScrap.Data;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    //private static AudioClip _cachedShipIntroSpeechSFX;

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
