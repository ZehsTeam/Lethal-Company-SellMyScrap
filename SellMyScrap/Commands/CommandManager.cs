using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal static class CommandManager
{
    private static List<Command> _commands = [];

    public static Command AwaitingConfirmationCommand;

    public static void Initialize()
    {
        _commands =
        [
            new HelpCommand(),
            new SellQuotaCommand(),
            new SellAllCommand(),
            new SellItemCommand(),
            new SellListCommand(),
            new SellAmountCommand(),
            new ViewScrapCommand(),
            new ViewAllScrapCommand(),
            new ViewOvertimeCommand(),
            new ViewConfigCommand(),
            new EditConfigCommand(),
        ];

        AwaitingConfirmationCommand = null;
    }

    public static bool TryExecuteCommand(string[] array, out TerminalNode terminalNode)
    {
        terminalNode = null;

        string[] args = GetArgs(array, 3);

        if (AwaitingConfirmationCommand != null)
        {
            Command _command = AwaitingConfirmationCommand;
            terminalNode = _command.ExecuteConfirmation(args);
            _command.PreviousTerminalNode = terminalNode;
            return true;
        }

        Command command = GetCommand(args);
        if (command == null) return false;
        
        terminalNode = command.Execute(args);
        command.PreviousTerminalNode = terminalNode;
        return true;
    }
    
    public static void OnLocalDisconnect()
    {
        AwaitingConfirmationCommand = null;
    }

    public static void OnTerminalQuit()
    {
        AwaitingConfirmationCommand = null;
    }

    private static string[] GetArgs(string[] array, int length)
    {
        List<string> args = new List<string>();

        foreach (string arg in array)
        {
            if (arg.Trim() == string.Empty) continue;

            args.Add(arg.Trim());
        }

        if (args.Count > length) return args.ToArray();

        for (int i = 0; i < length - args.Count; i++)
        {
            args.Add(string.Empty);
        }

        return args.ToArray();
    }

    private static Command GetCommand(string[] args)
    {
        string[] _args = args;

        foreach (var command in _commands)
        {
            if (command.IsCommand(_args))
            {
                return command;
            }
        }

        return null;
    }
}
