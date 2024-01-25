using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace SellMyScrap.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("ParsePlayerSentence")]
        [HarmonyPrefix]
        static bool ParsePlayerSentencePatch(Terminal __instance, ref TerminalNode __result)
        {
            string[] array = __instance.screenText.text.Split(new char[1] { '\n' });
            if (array.Length == 0) return true;

            string[] array2 = array.Last().Trim().ToLower().Split(new char[1] { ' ' });

            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            if (localPlayerController == null) return true;

            StartOfRound startOfRound = localPlayerController.playersManager;
            if (startOfRound == null) return true;

            SellType sellType = SellType.None;
            if (array2[0] == "sell") sellType = SellType.SellAmount;
            if (array2[0] == "sell-quota") sellType = SellType.SellQuota;
            if (array2[0] == "sell-all") sellType = SellType.SellAll;

            // Trying to sell
            if (sellType != SellType.None)
            {
                // Check if host
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    __result = CreateTerminalNode("Only the host can sell!\n\n");
                    return false;
                }

                // Check if at The Company building
                if (!IsAtTheCompany(startOfRound) || startOfRound.inShipPhase || startOfRound.travellingToNewLevel)
                {
                    __result = CreateTerminalNode($"You must be at The Company building to sell your scrap!\n\n");
                    return false;
                }
            }

            // sell <amount>
            if (sellType == SellType.SellAmount)
            {
                // Amount not specified
                if (array2.Length < 2)
                {
                    __result = CreateTerminalNode("Please specify an amount to sell.\n\nUsage: sell <amount>\n\n");
                    return false;
                }

                int amount = 0;
                int.TryParse(array2[1], out amount);

                // Invalid sell amount
                if (amount <= 0)
                {
                    __result = CreateTerminalNode("ERROR! Sell amount is invalid.\n\nUsage: sell <amount>\n\n");
                    return false;
                }

                GameObject ship = GameObject.Find("/Environment/HangarShip");
                ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(amount, ship);
                int rate = (int)(100f * startOfRound.companyBuyingRate);

                // No items to sell
                if (scrapToSell.scrap.Count == 0)
                {
                    __result = CreateTerminalNode("No items found to sell.\n\n");
                    return false;
                }

                string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value} / ${amount} selling at %{rate}\n\n";
                
                // Display found scrap items
                if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
                {
                    if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
                    {
                        message += $"{scrapToSell.GetListAsString()}\n\n";
                    }
                    else
                    {
                        message += $"[Too many items to show]\n\n";
                    }
                }

                message += "Please CONFIRM or DENY.\n\n";
                
                __result = CreateTerminalNode(message);
                SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellAmount, scrapToSell.value, amount, true);
                return false;
            }

            // sell-quota
            if (sellType == SellType.SellQuota)
            {
                int amount = TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;

                if (amount <= 0)
                {
                    __result = CreateTerminalNode("Quota has already been fulfilled.\n\n");
                    return false;
                }

                GameObject ship = GameObject.Find("/Environment/HangarShip");
                ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(amount, ship);
                int rate = (int)(100f * startOfRound.companyBuyingRate);

                // No items to sell
                if (scrapToSell.scrap.Count == 0)
                {
                    __result = CreateTerminalNode("No items found to sell.\n\n");
                    return false;
                }

                string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value} / ${amount} selling at %{rate}\n\n";

                // Display found scrap items
                if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
                {
                    if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
                    {
                        message += $"{scrapToSell.GetListAsString()}\n\n";
                    }
                    else
                    {
                        message += $"[Too many items to show]\n\n";
                    }
                }

                message += "Please CONFIRM or DENY.\n\n";

                __result = CreateTerminalNode(message);
                SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellQuota, scrapToSell.value, amount, true);
                return false;
            }

            // sell-all
            if (sellType == SellType.SellAll)
            {
                GameObject ship = GameObject.Find("/Environment/HangarShip");
                ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(-1, ship);
                int rate = (int)(100f * startOfRound.companyBuyingRate);

                // No items to sell
                if (scrapToSell.scrap.Count == 0)
                {
                    __result = CreateTerminalNode("No items found to sell.\n\n");
                    return false;
                }

                string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value} selling at %{rate}\n\n";

                // Display found scrap items
                if (SellMyScrapBase.Instance.ConfigManager.ShowFoundItems)
                {
                    if (scrapToSell.scrap.Count <= SellMyScrapBase.Instance.ConfigManager.ShowFoundItemsLimit)
                    {
                        message += $"{scrapToSell.GetListAsString()}\n\n";
                    }
                    else
                    {
                        message += $"[Too many items to show]\n\n";
                    }
                }

                message += "Please CONFIRM or DENY.\n\n";

                __result = CreateTerminalNode(message);
                SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellAll, scrapToSell.value, -1, true);
                return false;
            }

            // Sell confirmation
            if (sellType == SellType.None && SellMyScrapBase.Instance.sellRequest != null && SellMyScrapBase.Instance.sellRequest.awaitingConfirmation)
            {
                DepositItemsDesk depositItemsDesk = UnityEngine.Object.FindObjectOfType<DepositItemsDesk>();

                if (depositItemsDesk == null)
                {
                    SellMyScrapBase.Instance.sellRequest = null;
                    SellMyScrapBase.Instance.scrapToSell = null;

                    __result = CreateTerminalNode("ERROR! Cannot find DepositItemsDesk. Sell aborted.\n\n");

                    return false;
                }

                // CONFIRM
                if ("confirm".Contains(array2[0].ToLower()))
                {
                    SellRequest sellRequest = SellMyScrapBase.Instance.sellRequest;

                    GameObject ship = GameObject.Find("/Environment/HangarShip");

                    // sell <amount>
                    if (sellRequest.type == SellType.SellAmount)
                    {
                        __result = CreateTerminalNode($"Sell confirmed. Processing ${sellRequest.amount}...\n\n");

                        SellMyScrapBase.Instance.RequestSell(sellRequest.amount, ship, depositItemsDesk);
                    }

                    // sell-quota
                    if (sellRequest.type == SellType.SellQuota)
                    {
                        __result = CreateTerminalNode($"Sell confirmed. Processing ${sellRequest.amount}...\n\n");

                        SellMyScrapBase.Instance.RequestSell(sellRequest.amount, ship, depositItemsDesk);
                    }

                    // sell-all
                    if (sellRequest.type == SellType.SellAll)
                    {
                        __result = CreateTerminalNode($"Sell confirmed. Processing ${sellRequest.amount}...\n\n");

                        SellMyScrapBase.Instance.RequestSellAll(ship, depositItemsDesk);
                    }

                    return false;
                }

                // DENY
                if ("deny".Contains(array2[0].ToLower()))
                {
                    __result = CreateTerminalNode("Sell aborted.\n\n");

                    return false;
                }

                SellMyScrapBase.Instance.sellRequest = null;
            }

            return true;
        }

        private static bool IsAtTheCompany(StartOfRound playersManager)
        {
            int companyBuildingLevelID = 3;

            return playersManager.currentLevel.levelID == companyBuildingLevelID;
        } 

        private static TerminalNode CreateTerminalNode(string message)
        {
            return new TerminalNode
            {
                displayText = message,
                clearPreviousText = true
            };
        }
    }
}
