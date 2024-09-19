﻿using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewOvertimeCommand : Command
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "overtime")) return true;
        if (MatchesPattern(ref args, "overtime", "bonus")) return true;
        if (MatchesPattern(ref args, "overtime-bonus")) return true;
        if (MatchesPattern(ref args, "view", "overtime")) return true;
        if (MatchesPattern(ref args, "view-overtime")) return true;
        if (MatchesPattern(ref args, "view", "overtime", "bonus")) return true;
        if (MatchesPattern(ref args, "view-overtime-bonus")) return true;
        if (MatchesPattern(ref args, "view", "overtimebonus")) return true;
        if (MatchesPattern(ref args, "view-overtimebonus")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        int overtimeBonus = Utils.GetOvertimeBonus(0);
        int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
        int profitQuota = TimeOfDay.Instance.profitQuota;
        int newTotalCredits = TerminalPatch.Instance.groupCredits + overtimeBonus;

        string message = $"Your current overtime bonus is ${overtimeBonus}\n";
        message += $"Quota fulfilled: ${quotaFulfilled} / ${profitQuota}\n";
        message += $"Your new total credits will be ${newTotalCredits}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
