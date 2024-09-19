using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class HelpCommand : Command
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "sell", "")) return true;
        if (MatchesPattern(ref args, "sell", "help")) return true;
        if (MatchesPattern(ref args, "sell-help")) return true;
        if (MatchesPattern(ref args, "sellmyscrap")) return true;
        if (MatchesPattern(ref args, "sell", "my", "scrap")) return true;
        if (MatchesPattern(ref args, "sms")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        // The args here will no longer contain the matched command parts
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}\n\n";
        message += "The following commands are available:\n\n";
        message += "sell <amount>      sell-amount <amount>\n";
        message += "sell quota         sell-quota\n";
        message += "sell all           sell-all\n";
        message += "sell item <name>   sell-item <name>\n";
        message += "sell list          sell-list\n";
        message += "view overtime      view-overtime\n";
        message += "view scrap         view-scrap\n";
        message += "view all scrap     view-all-scrap\n";
        message += "view config        view-config\n";
        message += "edit config        edit-config\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
