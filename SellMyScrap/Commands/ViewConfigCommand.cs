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
        return TerminalPatch.CreateTerminalNode("The SellMyScrap config viewer has been removed. Please use the LethalConfig mod to view the config settings in-game.\n\n");
    }
}
