using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(InteractTrigger))]
internal class InteractTriggerPatch
{
    [HarmonyPatch(nameof(InteractTrigger.StopInteraction))]
    [HarmonyPostfix]
    static void StopInteractionPatch(ref InteractTrigger __instance)
    {
        if (StartMatchLeverPatch.InteractTrigger == __instance)
        {
            StartMatchLeverPatch.DisplayedSellWarning = false;
        }
    }
}
