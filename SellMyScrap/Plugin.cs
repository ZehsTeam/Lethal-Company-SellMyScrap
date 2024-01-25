using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SellMyScrap.Patches;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SellMyScrap
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SellMyScrapBase : BaseUnityPlugin
    {
        private const string modGUID = "Zehs.SellMyScrap";
        private const string modName = "Sell My Scrap";
        private const string modVersion = "1.1.4";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static SellMyScrapBase Instance;
        internal static ManualLogSource mls;
        internal ConfigurationController ConfigManager;

        public SellRequest sellRequest;
        public ScrapToSell scrapToSell;
        public string[] dontSellList;

        void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modName} has awoken!");

            harmony.PatchAll(typeof(TerminalPatch));

            ConfigManager = new ConfigurationController(Config);
            dontSellList = Instance.ConfigManager.DontSellListJson;
        }

        public ScrapToSell GetScrapToSell(int amount, GameObject ship)
        {
            // Get all scrap
            if (amount == -1)
            {
                scrapToSell = new ScrapToSell(GetScrapFromShip(ship));
                return scrapToSell;
            }

            // Get scrap based on amount
            scrapToSell = ScrapCalculator.GetScrapToSell(GetScrapFromShip(ship), amount, StartOfRound.Instance.companyBuyingRate);
            return scrapToSell;
        }

        public void RequestSell(int amount, GameObject ship, DepositItemsDesk depositItemsDesk)
        {
            if (scrapToSell == null|| scrapToSell.value != amount)
            {
                GetScrapToSell(amount, ship);
            }

            PerformSell(scrapToSell.scrap, depositItemsDesk);

            scrapToSell = null;
        }

        public void RequestSellAll(GameObject ship, DepositItemsDesk depositItemsDesk)
        {
            PerformSell(GetScrapFromShip(ship), depositItemsDesk);
        }

        public static List<GrabbableObject> GetScrapFromShip(GameObject ship)
        {
            GrabbableObject[] itemsInShip = ship.GetComponentsInChildren<GrabbableObject>();
            List<GrabbableObject> scrap = new List<GrabbableObject>();

            for (int i = 0; i < itemsInShip.Length; i++)
            {
                GrabbableObject itemInShip = itemsInShip[i];
                if (!IsItemAllowedScrap(itemInShip)) continue;

                scrap.Add(itemInShip);
            }

            return scrap;
        }

        public static bool IsItemAllowedScrap(GrabbableObject item)
        {
            if (!item.itemProperties.isScrap) return false;
            if (item.isPocketed) return false;
            if (item.isHeld) return false;

            string itemName = item.itemProperties.itemName;
            if (itemName == "Gift" && !Instance.ConfigManager.SellGifts) return false;
            if (itemName == "Shotgun" && !Instance.ConfigManager.SellShotguns) return false;
            if (itemName == "Ammo" && !Instance.ConfigManager.SellAmmo) return false;
            if (itemName == "Homemade flashbang" && !Instance.ConfigManager.SellHomemadeFlashbang) return false;
            if (itemName == "Jar of pickles" && !Instance.ConfigManager.SellPickles) return false;
            
            // Dont sell list
            for (int i = 0; i < Instance.dontSellList.Length; i++)
            {
                if (itemName.ToLower().Contains(Instance.dontSellList[i].ToLower())) return false;
            }

            return true;
        }

        public static void PerformSell(List<GrabbableObject> scrap, DepositItemsDesk depositItemsDesk)
        {
            Instance.scrapToSell = null;

            scrap.ForEach(item =>
            {
                item.transform.parent = depositItemsDesk.deskObjectsContainer.transform;
                
                depositItemsDesk.AddObjectToDeskServerRpc(item.gameObject.GetComponent<NetworkObject>());
            });

            depositItemsDesk.SellItemsOnServer();
        }
    }
}
