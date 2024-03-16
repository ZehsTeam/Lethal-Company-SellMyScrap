using HarmonyLib;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void StartPatch()
    {
        AddNetworkPrefabs();
    }

    private static void AddNetworkPrefabs()
    {
        NetworkManager.Singleton.AddNetworkPrefab(Content.networkHandlerPrefab);
    }
}
