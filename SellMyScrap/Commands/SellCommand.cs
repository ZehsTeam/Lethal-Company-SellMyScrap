﻿using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellCommand : Command
{
    public SellCommand()
    {
        flags.Add(new CommandFlag("-se", canHaveExtraData: true));
    }

    protected static int CompanyBuyingRate => (int)(StartOfRound.Instance.companyBuyingRate * 100);

    protected override TerminalNode OnConfirm(string[] args)
    {
        string message = $"Sell confirmed. Processing {GetValueString(SellMyScrapBase.Instance.sellRequest)}...\n\n";

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

    protected static string GetValueString(ScrapToSell scrapToSell)
    {
        return GetValueString(scrapToSell.realValue, scrapToSell.value);
    }

    protected static string GetValueString(SellRequest sellRequest)
    {
        return GetValueString(sellRequest.realValue, sellRequest.value);
    }

    protected static string GetValueString(int realValue, int value)
    {
        return CompanyBuyingRate == 100 ? $"${value}" : $"${realValue} (${value})";
    }

    protected static string GetOvertimeBonusString(int value)
    {
        int overtimeBonus = Utils.GetOvertimeBonus(value);
        return overtimeBonus == 0 ? "\n" : $"Overtime bonus: ${overtimeBonus} (${overtimeBonus + value})\n\n";
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
