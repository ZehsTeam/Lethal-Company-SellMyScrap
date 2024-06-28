using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewOvertimeCommand : Command
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "view" && args[1] == "overtime") return true;
        if (args[0] == "view-overtime") return true;
        if (args[0] == "view" && args[1] == "overtime" && args[2] == "bonus") return true;
        if (args[0] == "view-overtime-bonus") return true;
        if (args[0] == "view" && args[1] == "overtimebonus") return true;
        if (args[0] == "view-overtimebonus") return true;

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
