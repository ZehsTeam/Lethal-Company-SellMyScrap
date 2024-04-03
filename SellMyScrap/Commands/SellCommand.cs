using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellCommand : Command
{
    public SellCommand()
    {
        flags.Add(new CommandFlag("-se", canHaveData: true));
    }

    protected static int CompanyBuyingRate => (int)(StartOfRound.Instance.companyBuyingRate * 100);

    protected override TerminalNode OnConfirm(string[] args)
    {
        string message = $"Sell confirmed. Processing {SellMyScrapBase.Instance.sellRequest.realValue}...\n\n";

        SellMyScrapBase.Instance.ConfirmSellRequest();
        awaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode(message);
    }

    protected override TerminalNode OnDeny(string[] args)
    {
        SellMyScrapBase.Instance.CancelSellRequest();

        awaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode($"Sell aborted.\n\n");
    }

    protected static bool CanUseCommand(out TerminalNode terminalNode)
    {
        terminalNode = null;

        StartOfRound startOfRound = StartOfRound.Instance;
        bool isAtCompany = startOfRound.currentLevelID == 3;
        bool isLanded = !startOfRound.inShipPhase && !startOfRound.travellingToNewLevel;

        if (!isAtCompany || !isLanded)
        {
            terminalNode = TerminalPatch.CreateTerminalNode($"You must be landed at The Company building to sell your scrap.\n\n");
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
        CommandFlag flag = foundFlags.Find(_ => _.key.ToLower() == "-se");
        if (flag == null || !flag.canUse) return -1;

        if (int.TryParse(flag.data, out int index))
        {
            return index;
        }

        return 0;
    }
}
