using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellCommand : Command
{
    public SellCommand()
    {
        Flags.Add(new CommandFlag("-se", canHaveData: true));
    }

    protected static int CompanyBuyingRate => (int)(StartOfRound.Instance.companyBuyingRate * 100);

    protected override TerminalNode OnConfirm(string[] args)
    {
        string message = $"Sell confirmed. Processing {Plugin.Instance.SellRequest.RealValue}...\n\n";

        Plugin.Instance.ConfirmSellRequest();
        AwaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode(message);
    }

    protected override TerminalNode OnDeny(string[] args)
    {
        Plugin.Instance.CancelSellRequest();

        AwaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode($"Sell aborted.\n\n");
    }

    protected static bool CanUseCommand(out TerminalNode terminalNode)
    {
        terminalNode = TerminalPatch.CreateTerminalNode($"You must be landed at The Company building to sell your scrap.\n\n");

        if (StartOfRound.Instance.currentLevelID != 3) return false; // Return false if not at the Company moon.
        if (StartOfRound.Instance.inShipPhase) return false; // Return false if the ship is in orbit.
        
        bool isShipLanded = StartOfRound.Instance.shipHasLanded;
        bool isShipLeaving = StartOfRound.Instance.shipIsLeaving;

        if (!isShipLanded && !isShipLeaving)
        {
            return false;
        }

        return true;
    }

    protected static string GetQuotaFulfilledString()
    {
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int valueNeeded = Mathf.Max(profitQuota - quotaFulfilled, 0);

        if (valueNeeded == 0)
        {
            return $"Quota fulfilled: ${quotaFulfilled} / ${profitQuota}\n";
        }

        return $"Quota fulfilled: ${quotaFulfilled} / ${profitQuota} (Need: ${valueNeeded})\n";
    }

    protected static string GetOvertimeBonusString(int value)
    {
        int overtimeBonus = Utils.GetOvertimeBonus(value);

        if (overtimeBonus == 0)
        {
            return string.Empty;
        }

        return $"Overtime bonus: ${overtimeBonus} (With value: ${value + overtimeBonus})\n";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="foundFlags"></param>
    /// <returns>-1 = no scrap eater, 0 = random scrap eater, 1+ = scrap eater index.</returns>
    protected static int GetScrapEaterIndex(List<CommandFlag> foundFlags)
    {
        CommandFlag flag = foundFlags.Find(_ => _.Key.ToLower() == "-se");
        if (flag == null || !flag.CanUse) return -1;

        if (int.TryParse(flag.Data, out int index))
        {
            return index;
        }

        return 0;
    }
}
