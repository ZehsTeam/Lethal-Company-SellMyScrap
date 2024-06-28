using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class Command
{
    public TerminalNode PreviousTerminalNode;

    public bool AwaitingConfirmation
    {
        get
        {
            return CommandManager.AwaitingConfirmationCommand == this;
        }
        set
        {
            CommandManager.AwaitingConfirmationCommand = value ? this : null;
        }
    }

    protected List<CommandFlag> Flags = [];

    public virtual bool IsCommand(string[] args)
    {
        return false;
    }

    public virtual TerminalNode Execute(string[] args)
    {
        return TerminalPatch.CreateTerminalNode("Execute override was not found.\n\n");
    }
    
    public virtual TerminalNode ExecuteConfirmation(string[] args)
    {
        string arg = args[0].ToLower();

        if ("confirm".Contains(arg) && arg.Length > 0)
        {
            return OnConfirm(args);
        }

        if ("deny".Contains(arg) && arg.Length > 0)
        {
            return OnDeny(args);
        }

        return OnInvalidInput(args);
    }

    protected virtual TerminalNode OnConfirm(string[] args)
    {
        return TerminalPatch.CreateTerminalNode("OnConfirm override was not found.\n\n");
    }

    protected virtual TerminalNode OnDeny(string[] args)
    {
        return TerminalPatch.CreateTerminalNode("OnDeny override was not found.\n\n");
    }

    protected virtual TerminalNode OnInvalidInput(string[] args)
    {
        return PreviousTerminalNode;
    }

    protected List<CommandFlag> GetFlagsFromString(string extra)
    {
        int startIndex = GetFlagsStartIndexInString(extra);
        if (startIndex == -1) return [];

        List<CommandFlag> foundFlags = new List<CommandFlag>();

        string[] items = extra.Substring(startIndex).Trim().Split(' ');

        foreach (var item in items)
        {
            CommandFlag foundFlag = GetFlagFromString(item);
            if (foundFlag == null) continue;

            foundFlags.Add(foundFlag);
        }

        return foundFlags;
    }

    private CommandFlag GetFlagFromString(string text)
    {
        CommandFlag foundFlag = null;

        foreach (var flag in Flags)
        {
            if (text.StartsWith(flag.Key, System.StringComparison.OrdinalIgnoreCase))
            {
                foundFlag = flag;
                break;
            }
        }

        if (foundFlag == null) return null;

        bool validLength = text.Length == foundFlag.Key.Length;
        bool hasData = foundFlag.CanHaveData && text.Contains(":");
        if (hasData) validLength = true;

        if (!validLength) return null;

        string flagData = string.Empty;

        if (hasData)
        {
            flagData = text.Split(":")[1];
        }

        return new CommandFlag(foundFlag.Key, foundFlag.IsHostOnly, foundFlag.CanHaveData, flagData);
    }

    protected int GetFlagsStartIndexInString(string extra)
    {
        int startIndex = -1;

        Flags.ForEach(flag =>
        {
            int index = extra.IndexOf(flag.Key, System.StringComparison.OrdinalIgnoreCase);
            if (index == -1) return;

            if (startIndex == -1)
            {
                startIndex = index;
                return;
            }

            if (index < startIndex)
            {
                startIndex = index;
            }
        });

        return startIndex;
    }
}

public class CommandFlag
{
    public string Key;
    public bool IsHostOnly;
    public bool CanHaveData;
    public string Data;

    public bool CanUse => IsHostOnly ? Plugin.IsHostOrServer : true;

    public CommandFlag(string key, bool isHostOnly = false, bool canHaveData = false, string data = "")
    {
        this.Key = key;
        this.IsHostOnly = isHostOnly;
        this.CanHaveData = canHaveData;
        this.Data = data;
    }
}
