using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Objects;
using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!NetworkUtils.IsServer) return;

        var networkHandlerHost = Object.Instantiate(Assets.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
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
        bool isAprilFirst = DateTime.Today.Month == 4 && DateTime.Today.Day == 1;
        
        if (isAprilFirst)
        {
            StartOfRound.Instance.shipIntroSpeechSFX = Assets.BrainRotIntroSpeechSFX;
        }
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
        Plugin.HandleLocalDisconnect();
    }
}
