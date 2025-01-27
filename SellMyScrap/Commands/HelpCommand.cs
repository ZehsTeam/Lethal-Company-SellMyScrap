using com.github.zehsteam.SellMyScrap.Helpers;
using System.Text;

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
        StringBuilder builder = new StringBuilder();
        
        builder.AppendLine($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}\n");
        builder.AppendLine("The following commands are available:\n");
        builder.AppendLine("sell <amount>      sell-amount <amount>");
        builder.AppendLine("sell quota         sell-quota");
        builder.AppendLine("sell all           sell-all");
        builder.AppendLine("sell item <name>   sell-item <name>");
        builder.AppendLine("sell list          sell-list");
        builder.AppendLine("view overtime      view-overtime");
        builder.AppendLine("view scrap         view-scrap");
        builder.AppendLine("view all scrap     view-all-scrap\n\n");

        return TerminalHelper.CreateTerminalNode(builder.ToString());
    }
}
