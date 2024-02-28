using com.github.zehsteam.SellMyScrap.Patches;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewConfigCommand : Command
{
    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "view" && args[1] == "config") return true;
        if (args[0] == "view-config") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        string message = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} config\n\n";
        message += $"{ConfigHelper.GetConfigSettingsMessage()}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
