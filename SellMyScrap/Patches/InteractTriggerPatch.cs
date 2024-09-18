using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(InteractTrigger))]
internal static class InteractTriggerPatch
{
    [HarmonyPatch(nameof(InteractTrigger.StopInteraction))]
    [HarmonyPostfix]
    private static void StopInteractionPatch(ref InteractTrigger __instance)
    {
        if (StartMatchLeverPatch.InteractTrigger == __instance)
        {
            StartMatchLeverPatch.DisplayedSellWarning = false;
        }
    }
}
