using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

public enum CommandType
{
    None,
    Sell,
    ViewScrap,
}

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

        string first = array2[0].ToLower();

        CommandType commandType = CommandType.None;
        SellType sellType = SellType.None;

        // Find command
        switch (first)
        {
            case "sell":
                {
                    commandType = CommandType.Sell;
                    sellType = SellType.SellAmount;
                    break;
                }
            case "sell-quota":
                {
                    commandType = CommandType.Sell;
                    sellType = SellType.SellQuota;
                    break;
                }
            case "sell-all":
                {
                    commandType = CommandType.Sell;
                    sellType = SellType.SellAll;
                    break;
                }
            case "view-scrap":
                {
                    commandType = CommandType.ViewScrap;
                    break;
                }
        }

        // Trying to sell
        if (sellType != SellType.None)
        {
            // Check if host
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                __result = CreateTerminalNode("Only the host can use sell commands!\n\n");
                return false;
            }

            // Check if at The Company building
            if (!IsAtTheCompany(startOfRound) || startOfRound.inShipPhase || startOfRound.travellingToNewLevel)
            {
                __result = CreateTerminalNode($"You must be landed at The Company building to sell your scrap!\n\n");
                return false;
            }
        }

        // Parse sell <amount>
        if (commandType == CommandType.Sell && sellType == SellType.SellAmount)
        {
            __result = CreateTerminalNode(ParseSellAmount(array2));
            return false;
        }

        // Parse sell-quota
        if (commandType == CommandType.Sell && sellType == SellType.SellQuota)
        {
            __result = CreateTerminalNode(ParseSellQuota(array2));
            return false;
        }

        // Parse sell-all
        if (commandType == CommandType.Sell && sellType == SellType.SellAll)
        {
            __result = CreateTerminalNode(ParseSellAll(array2));
            return false;
        }

        // Parse sell confirmation
        bool showSellConfirmation = (commandType == CommandType.None && SellMyScrapBase.Instance.sellRequest != null && SellMyScrapBase.Instance.sellRequest.awaitingConfirmation);

        if (showSellConfirmation)
        {
            __result = CreateTerminalNode(ParseSellConfirmation(array2));
            return false;
        }

        // Parse view
        if (commandType == CommandType.ViewScrap)
        {
            __result = CreateTerminalNode(ParseViewScrap(array2));
            return false;
        }

        return true;
    }

    private static bool IsAtTheCompany(StartOfRound playersManager)
    {
        int companyBuildingLevelID = 3;

        return playersManager.currentLevel.levelID == companyBuildingLevelID;
    } 

    private static GameObject GetShipGameObject()
    {
        return GameObject.Find("/Environment/HangarShip");
    }

    private static int GetCompanyBuyingRate()
    {
        return (int)(100f * StartOfRound.Instance.companyBuyingRate);
    }

    private static string ParseSellAmount(string[] array)
    {
        // Amount not specified
        if (array.Length < 2)
        {
            return "Please specify an amount to sell.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer\nExample: sell 500\n\n";
        }

        int amount = 0;
        int.TryParse(array[1], out amount);

        // Invalid sell amount
        if (amount <= 0)
        {
            return "ERROR! Sell amount is invalid.\n\nUsage: sell <amount>\nWhere <amount> is a positive integer\nExample: sell 500\n\n";
        }

        GameObject ship = GetShipGameObject();
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(amount, ship);
        int rate = GetCompanyBuyingRate();

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nRequested amount: ${amount}\nThe Company is buying at %{rate}\n\n";

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

        SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellAmount, scrapToSell.value, amount, true);

        // Return confirmation message
        return message;
    }

    private static string ParseSellQuota(string[] array)
    {
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int amount = profitQuota - quotaFulfilled;

        // Quota has already been fulfilled
        if (amount <= 0)
        {
            return "Quota has already been fulfilled.\n\n";
        }

        GameObject ship = GetShipGameObject();
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(amount, ship);
        int rate = GetCompanyBuyingRate();

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nProfit quota: ${quotaFulfilled} / ${profitQuota} (${amount})\nThe Company is buying at %{rate}\n\n";

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

        SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellQuota, scrapToSell.value, amount, true);

        // Return confirmation message
        return message;
    }

    private static string ParseSellAll(string[] array)
    {
        GameObject ship = GetShipGameObject();
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(-1, ship);
        int rate = GetCompanyBuyingRate();

        // No items to sell
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found to sell.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\nThe Company is buying at %{rate}\n\n";

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

        SellMyScrapBase.Instance.sellRequest = new SellRequest(SellType.SellAll, scrapToSell.value, -1, true);

        // Return confirmation message
        return message;
    }

    private static string ParseSellConfirmation(string[] array)
    {
        DepositItemsDesk depositItemsDesk = UnityEngine.Object.FindAnyObjectByType<DepositItemsDesk>();

        if (depositItemsDesk == null)
        {
            SellMyScrapBase.Instance.sellRequest = null;
            SellMyScrapBase.Instance.scrapToSell = null;

            return "ERROR! Cannot find DepositItemsDesk. Sell aborted.\n\n";
        }

        // CONFIRM
        if ("confirm".Contains(array[0].ToLower()))
        {
            SellRequest sellRequest = SellMyScrapBase.Instance.sellRequest;

            GameObject ship = GameObject.Find("/Environment/HangarShip");

            // sell <amount> OR sell-quota
            if (sellRequest.type == SellType.SellAmount || sellRequest.type == SellType.SellQuota)
            {
                SellMyScrapBase.Instance.RequestSell(sellRequest.amount, ship, depositItemsDesk);

                return $"Sell confirmed. Processing ${sellRequest.amount}...\n\n";
            }

            // sell-all
            if (sellRequest.type == SellType.SellAll)
            {
                SellMyScrapBase.Instance.RequestSellAll(ship, depositItemsDesk);

                return $"Sell confirmed. Processing ${sellRequest.amount}...\n\n";
            }
        }

        // DENY
        if ("deny".Contains(array[0].ToLower()))
        {
            SellMyScrapBase.Instance.sellRequest = null;

            return "Sell aborted.\n\n";
        }

        SellMyScrapBase.Instance.sellRequest = null;

        return "Invalid input. Sell aborted.\n\n";
    }

    private static string ParseViewScrap(string[] array)
    {
        GameObject ship = GetShipGameObject();
        ScrapToSell scrapToSell = SellMyScrapBase.Instance.GetScrapToSell(-1, ship);

        // No items found
        if (scrapToSell.scrap.Count == 0)
        {
            return "No items found.\n\n";
        }

        string message = $"Found {scrapToSell.scrap.Count} items with a total of ${scrapToSell.value}\n\n";

        message += $"{scrapToSell.GetListAsString()}\n\n";

        return message;
    }

    private static TerminalNode CreateTerminalNode(string message)
    {
        return CreateTerminalNode(message, true);
    }

    private static TerminalNode CreateTerminalNode(string message, bool clearPreviousText)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = message;
        terminalNode.clearPreviousText = clearPreviousText;

        return terminalNode;
    }
}
