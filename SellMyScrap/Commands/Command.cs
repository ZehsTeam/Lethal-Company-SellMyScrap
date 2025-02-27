using com.github.zehsteam.SellMyScrap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace com.github.zehsteam.SellMyScrap.Commands;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public abstract class Command
{
    public TerminalNode PreviousTerminalNode;

    public bool AwaitingConfirmation
    {
        get => CommandManager.AwaitingConfirmationCommand == this;
        set => CommandManager.AwaitingConfirmationCommand = value ? this : null;
    }

    // This will store the parsed flag data after executing the command
    private Dictionary<string, string> _parsedFlags = [];

    public virtual bool IsCommand(ref string[] args)
    {
        return false;
    }

    // Helper method to match command patterns and remove matched args
    protected bool MatchesPattern(ref string[] args, params string[] pattern)
    {
        // Check if the provided pattern matches the start of args
        if (args.Length < pattern.Length) return false;

        for (int i = 0; i < pattern.Length; i++)
        {
            if (!args[i].Equals(pattern[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Remove matched args by slicing the array
        args = ParseFlags(args.Skip(pattern.Length).ToArray());

        return true;
    }

    public virtual TerminalNode Execute(string[] args)
    {
        return TerminalHelper.CreateTerminalNode("Execute override was not found.\n\n");
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
        return TerminalHelper.CreateTerminalNode("OnConfirm override was not found.\n\n");
    }

    protected virtual TerminalNode OnDeny(string[] args)
    {
        return TerminalHelper.CreateTerminalNode("OnDeny override was not found.\n\n");
    }

    protected virtual TerminalNode OnInvalidInput(string[] args)
    {
        return PreviousTerminalNode;
    }

    // Parses flags in the input string array and returns the remaining non-flag args
    protected string[] ParseFlags(string[] args)
    {
        _parsedFlags.Clear();

        List<string> remainingArgs = [];

        foreach (string arg in args)
        {
            // Match flags in the form "-flag:data" or "-flag"
            Match match = Regex.Match(arg, @"-(\w+)(?::(.+))?");

            if (match.Success)
            {
                string flagKey = match.Groups[1].Value.ToLower();
                string flagDataString = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                // Add the flag to the parsedFlags dictionary
                _parsedFlags[flagKey] = flagDataString;
            }
            else
            {
                // If the argument is not a flag, keep it in the remaining args
                remainingArgs.Add(arg);
            }
        }

        return remainingArgs.ToArray(); // Return the updated args without the flags
    }

    // Retrieve a flag with a specific key, allowing for data type parsing
    protected T GetFlagData<T>(string flagKey, T defaultValue = default)
    {
        if (_parsedFlags.TryGetValue(flagKey.ToLower(), out string flagDataString) && !string.IsNullOrEmpty(flagDataString))
        {
            try
            {
                return (T)Convert.ChangeType(flagDataString, typeof(T));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException($"Flag {flagKey} could not be parsed as {typeof(T)}");
            }
        }

        return defaultValue;
    }

    protected bool TryGetFlagData<T>(string flagKey, out T flagData)
    {
        flagData = default;

        if (_parsedFlags.TryGetValue(flagKey.ToLower(), out string flagDataString) && !string.IsNullOrEmpty(flagDataString))
        {
            try
            {
                flagData = (T)Convert.ChangeType(flagDataString, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException($"Flag {flagKey} could not be parsed as {typeof(T)}");
            }
        }

        return false;
    }

    // Check if a flag is present
    protected bool HasFlag(string flagKey)
    {
        return _parsedFlags.ContainsKey(flagKey.ToLower());
    }
}
