using HarmonyLib;
using Newtonsoft.Json;
using ShipInventory.Helpers;
using ShipInventory.Objects;
using ShipInventory.Patches;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;

[HarmonyPatch(typeof(GameNetworkManager_Patches))]
internal static class GameNetworkManager_PatchesPatch
{
    [HarmonyPatch(nameof(GameNetworkManager_Patches.SaveChuteItems))]
    [HarmonyPrefix]
    private static bool SaveChuteItemsPatch()
    {
        string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;

        try
        {
            ES3.DeleteKey("shipGrabbableItemIDs", currentSaveFileName);
            ES3.DeleteKey("shipGrabbableItemPos", currentSaveFileName);
            ES3.DeleteKey("shipScrapValues", currentSaveFileName);
            ES3.DeleteKey("shipItemSaveData", currentSaveFileName);
        }
        catch { }

        Plugin.Logger.LogInfo("[ShipInventory] Saving chute items...");

        try
        {
            IEnumerable<ItemData> items = ItemManager.GetItems();

            if (items.Any())
            {
                string data = JsonConvert.SerializeObject(items);
                ES3.Save("shipInventoryItemsJson", data, currentSaveFileName);
            }
            else
            {
                ES3.DeleteKey("shipInventoryItemsJson", currentSaveFileName);
            }

            Plugin.Logger.LogInfo("[ShipInventory] Chute items saved!");
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"[ShipInventory] Failed to save chute items. {ex}");
        }

        return false;
    }
}
