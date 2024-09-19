using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewConfigCommand : Command
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "view", "config")) return true;
        if (MatchesPattern(ref args, "view-config")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config\n\n";
        message += $"{ConfigHelper.GetConfigSettingsMessage()}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
