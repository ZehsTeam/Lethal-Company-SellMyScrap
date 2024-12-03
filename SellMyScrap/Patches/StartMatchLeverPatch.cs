using HarmonyLib;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(StartMatchLever))]
internal static class StartMatchLeverPatch
{
    public static InteractTrigger InteractTrigger;
    public static bool DisplayedSellWarning;

    [HarmonyPatch(nameof(StartMatchLever.Start))]
    [HarmonyPostfix]
    private static void StartPatch(ref StartMatchLever __instance)
    {
        InteractTrigger = __instance.GetComponent<InteractTrigger>();
        DisplayedSellWarning = false;
    }

    [HarmonyPatch(nameof(StartMatchLever.BeginHoldingInteractOnLever))]
    [HarmonyPostfix]
    private static void BeginHoldingInteractOnLeverPatch(ref StartMatchLever __instance)
    {
        // Return if there are still more days until the deadline
        if (TimeOfDay.Instance.daysUntilDeadline > 0 || !Plugin.ConfigManager.ShowQuotaWarning.Value)
        {
            // Reset the timeToHold on the InteractTrigger.
            if (__instance.triggerScript.timeToHold == 4.01f)
            {
                __instance.triggerScript.timeToHold = 0.7f;
            }
            
            return;
        }

        if (StartOfRound.Instance.currentLevelID != 3) return; // Return if not at the Company moon.
        if (!StartOfRound.Instance.shipHasLanded) return; // Return if the ship is not landed.

        // If the profit quota was fulfilled, reset the timeToHold on the InteractTrigger and return.
        if (TimeOfDay.Instance.quotaFulfilled >= TimeOfDay.Instance.profitQuota)
        {
            __instance.triggerScript.timeToHold = 0.7f;
            return;
        }

        if (!DisplayedSellWarning)
        {
            DisplayedSellWarning = true;

            __instance.triggerScript.timeToHold = 4.01f;
            HUDManager.Instance.DisplayTip("HALT!", "You did not sell enough scrap to fulfill the profit quota.", isWarning: true);
        }
    }
}
