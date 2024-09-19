using System.Collections.Generic;
using System.Linq;

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

        string[] args = GetArgs(array, expectedLength: 5);

        Command command;

        if (AwaitingConfirmationCommand != null)
        {
            command = AwaitingConfirmationCommand;
            terminalNode = command.ExecuteConfirmation(args);
            command.PreviousTerminalNode = terminalNode;
            return true;
        }

        command = GetCommand(ref args);
        if (command == null) return false;

        terminalNode = command.Execute(GetArgs(args, expectedLength: 5));
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

    private static string[] GetArgs(string[] array, int expectedLength)
    {
        if (array.Length >= expectedLength) return array;

        return array.Concat(Enumerable.Repeat(string.Empty, expectedLength - array.Length)).ToArray();
    }

    private static Command GetCommand(ref string[] args)
    {
        foreach (var command in _commands)
        {
            if (command.IsCommand(ref args))
            {
                return command;
            }
        }

        return null;
    }
}
