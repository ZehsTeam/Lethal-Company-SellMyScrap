using HarmonyLib;
using Newtonsoft.Json;
using ShipInventory.Helpers;
using ShipInventory.Objects;
using ShipInventory.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy.Patches;

[HarmonyPatch(typeof(StartOfRound_Patches))]
internal static class StartOfRound_PatchesPatch
{
    [HarmonyPatch(nameof(StartOfRound_Patches.LoadStoredItems))]
    [HarmonyPrefix]
    private static bool LoadStoredItemsPatch()
    {
        string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;

        try
        {
            if (!ES3.KeyExists("shipInventoryItemsJson", currentSaveFileName))
            {
                ItemManager.SetItems(System.Array.Empty<ItemData>());
                return false;
            }
        }
        catch { }

        Plugin.Logger.LogInfo("[ShipInventory] Loading stored items...");
        
        try
        {
            string data = ES3.Load<string>("shipInventoryItemsJson", currentSaveFileName);
            IEnumerable<ItemData> items = JsonConvert.DeserializeObject<IEnumerable<ItemData>>(data);

            ItemManager.SetItems(items);

            Plugin.Logger.LogInfo("[ShipInventory] Loaded stored items!");
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError($"[ShipInventory] Failed to load stored items. {ex}");
        }

        return false;
    }
}
