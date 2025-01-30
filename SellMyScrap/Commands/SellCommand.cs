using com.github.zehsteam.SellMyScrap.Dependencies.ShipInventoryProxy;
using com.github.zehsteam.SellMyScrap.Helpers;
using com.github.zehsteam.SellMyScrap.Helpers.ScrapMatchAlgorithms;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellCommand : Command
{
    protected static int CompanyBuyingRate => (int)(StartOfRound.Instance.companyBuyingRate * 100);

    protected override TerminalNode OnConfirm(string[] args)
    {
        string message = $"Sell confirmed. Processing {Plugin.Instance.SellRequest.RealScrapValue}...\n\n";

        Plugin.Instance.ConfirmSellRequest();
        AwaitingConfirmation = false;

        return TerminalHelper.CreateTerminalNode(message);
    }

    protected override TerminalNode OnDeny(string[] args)
    {
        Plugin.Instance.CancelSellRequest();

        AwaitingConfirmation = false;

        return TerminalHelper.CreateTerminalNode($"Sell aborted.\n\n");
    }

    protected int GetScrapEaterIndex()
    {
        if (TryGetFlagData("se", out int data))
        {
            return Mathf.Clamp(data - 1, 0, ScrapEaterManager.ScrapEaters.Count - 1);
        }

        return HasFlag("se") ? -1 : -2;
    }

    protected int GetScrapEaterVariantIndex()
    {
        if (TryGetFlagData("variant", out int data))
        {
            return Mathf.Max(data - 1, 0);
        }

        return -1;
    }

    protected bool WithOvertimeBonus()
    {
        return HasFlag("o");
    }

    protected bool OnlyUseShipInventory()
    {
        if (!ShipInventoryProxy.Enabled) return false;

        if (HasFlag("inv")) return true;
        if (HasFlag("inventory")) return true;
        if (HasFlag("shipinv")) return true;
        if (HasFlag("shipinventory")) return true;

        return false;
    }

    protected BaseScrapMatchAlgorithm GetScrapMatchAlgorithm()
    {
        if (TryGetFlagData("a", out int flagIndex))
        {
            return BaseScrapMatchAlgorithm.GetAlgorithmByFlag(flagIndex);
        }

        return BaseScrapMatchAlgorithm.Default;
    }

    protected static bool CanUseCommand(out TerminalNode failReason)
    {
        failReason = null;

        if (StartOfRound.Instance.inShipPhase)
        {
            failReason = TerminalHelper.CreateTerminalNode($"You must be landed on a moon that allows selling.\n\n");
            return false; // If the ship is in orbit.
        }

        if (DepositItemsDeskHelper.Instance == null)
        {
            failReason = TerminalHelper.CreateTerminalNode($"You cannot sell on this moon.\n\n");
            return false; // If not on a moon that has a sell desk.
        }

        return true;
    }

    protected static string GetQuotaFulfilledString(int valueFound)
    {
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int valueNeeded = Mathf.Max(profitQuota - quotaFulfilled, 0);

        if (valueNeeded == 0)
        {
            return $"Quota fulfilled: ${quotaFulfilled} / ${profitQuota}";
        }

        string needColor = valueFound >= valueNeeded ? TerminalHelper.GreenColor2 : "red";
        return $"Quota fulfilled: ${quotaFulfilled} / ${profitQuota} <color={needColor}>(Need: ${valueNeeded})</color>";
    }

    protected static string GetOvertimeBonusString(int value)
    {
        int overtimeBonus = Utils.GetOvertimeBonus(value);

        if (overtimeBonus == 0)
        {
            return string.Empty;
        }

        return $"Overtime bonus: ${overtimeBonus} <color={TerminalHelper.GreenColor2}>(With value: ${value + overtimeBonus})</color>\n";
    }

    protected static string GetOvertimeBonusWithValueString(int value, int targetValue, out bool hasEnoughWithOvertimeBonus)
    {
        int overtimeBonus = Utils.GetOvertimeBonus(value);

        hasEnoughWithOvertimeBonus = value + overtimeBonus >= targetValue;

        if (overtimeBonus <= 0)
        {
            return string.Empty;
        }

        string withValueColor = hasEnoughWithOvertimeBonus ? TerminalHelper.GreenColor2 : "red";
        return $"Overtime bonus: ${overtimeBonus} <color={withValueColor}>(With value: ${value + overtimeBonus})</color>\n";
    }
}
