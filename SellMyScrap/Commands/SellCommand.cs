using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class SellCommand : Command
{
    public static int companyBuyingRate => (int)(StartOfRound.Instance.companyBuyingRate * 100);

    public override TerminalNode OnConfirm(string[] args)
    {
        string message = $"Sell confirmed. Processing {GetValueString(SellMyScrapBase.Instance.sellRequest)}...\n\n";

        SellMyScrapBase.Instance.ConfirmSellRequest();
        awaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode(message);
    }

    public override TerminalNode OnDeny(string[] args)
    {
        SellMyScrapBase.Instance.CancelSellRequest();

        awaitingConfirmation = false;

        return TerminalPatch.CreateTerminalNode($"Sell aborted.\n\n");
    }

    public static bool CanUseCommand(out TerminalNode terminalNode)
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

    public static string GetValueString(ScrapToSell scrapToSell)
    {
        return GetValueString(scrapToSell.realValue, scrapToSell.value);
    }

    public static string GetValueString(SellRequest sellRequest)
    {
        return GetValueString(sellRequest.realValue, sellRequest.value);
    }

    private static string GetValueString(int realValue, int value)
    {
        if (companyBuyingRate == 100)
        {
            return $"${value}";
        }

        return $"${realValue} (${value})";
    }
}
