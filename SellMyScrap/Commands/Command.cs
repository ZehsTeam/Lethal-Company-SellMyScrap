using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

public class Command
{
    public TerminalNode previousTerminalNode;
    public bool awaitingConfirmation
    {
        get
        {
            return CommandManager.awaitingConfirmationCommand == this;
        }
        set
        {
            CommandManager.awaitingConfirmationCommand = value ? this : null;
        }
    }

    protected List<CommandFlag> flags = new List<CommandFlag>();

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
        return previousTerminalNode;
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

        foreach (var flag in flags)
        {
            if (text.StartsWith(flag.key, System.StringComparison.OrdinalIgnoreCase))
            {
                foundFlag = flag;
                break;
            }
        }

        if (foundFlag == null) return null;

        bool validLength = text.Length == foundFlag.key.Length;
        bool hasExtraData = foundFlag.canHaveExtraData && text.Contains(":");
        if (hasExtraData) validLength = true;

        if (!validLength) return null;

        string flagData = string.Empty;

        if (hasExtraData)
        {
            flagData = text.Split(":")[1];
        }

        return new CommandFlag(foundFlag.key, foundFlag.isHostOnly, foundFlag.canHaveExtraData, flagData);
    }

    protected int GetFlagsStartIndexInString(string extra)
    {
        int startIndex = -1;

        flags.ForEach(flag =>
        {
            int index = extra.IndexOf(flag.key, System.StringComparison.OrdinalIgnoreCase);
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
    public string key;
    public bool isHostOnly;
    public bool canHaveExtraData;
    public string data;

    public bool canUse => isHostOnly ? SellMyScrapBase.IsHostOrServer : true;

    public CommandFlag(string key, bool isHostOnly = false, bool canHaveExtraData = false, string data = "")
    {
        this.key = key;
        this.isHostOnly = isHostOnly;
        this.canHaveExtraData = canHaveExtraData;
        this.data = data;
    }
}
