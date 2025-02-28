﻿using com.github.zehsteam.SellMyScrap.Helpers;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class EditConfigCommand : Command
{
    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "edit", "config")) return true;
        if (MatchesPattern(ref args, "edit-config")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        return TerminalHelper.CreateTerminalNode("The SellMyScrap config editor has been removed. Please use the LethalConfig mod to edit the config settings in-game.\n\n");
    }
}
