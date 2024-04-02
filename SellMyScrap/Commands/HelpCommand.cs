using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class HelpCommand : Command
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "sell" && args[1] == string.Empty) return true;
        if (args[0] == "sell" && args[1] == "help") return true;
        if (args[0] == "sell-help") return true;
        if (args[0] == "sellmyscrap") return true;
        if (args[0] == "sell" && args[1] == "my" && args[2] == "scrap") return true;
        if (args[0] == "sms") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}\n\n";
        message += "The following commands are available:\n\n";
        message += "sell <amount>      sell-amount <amount>\n";
        message += "sell quota         sell-quota\n";
        message += "sell all           sell-all\n";
        message += "sell item <name>   sell-item <name>\n";
        message += "view scrap         view-scrap\n";
        message += "view all scrap     view-all-scrap\n";
        message += "view overtime      view-overtime\n";
        message += "view config        view-config\n";
        message += "edit config        edit-config\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
