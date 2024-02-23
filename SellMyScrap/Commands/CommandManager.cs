using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class CommandManager
{
    private static List<Command> commands = new List<Command>();
    public static Command awaitingConfirmationCommand;

    public static void Initialize()
    {
        commands =
        [
            new HelpCommand(),
            new SellQuotaCommand(),
            new SellAllCommand(),
            new SellAmountCommand(),
            new ViewScrapCommand(),
            new ViewConfigCommand(),
        ];

        awaitingConfirmationCommand = null;
    }

    public static bool TryExecuteCommand(string[] array, out TerminalNode terminalNode)
    {
        terminalNode = null;

        string[] args = GetArgs(array, 5);

        if (awaitingConfirmationCommand != null)
        {
            Command _command = awaitingConfirmationCommand;
            terminalNode = _command.ExecuteConfirmation(args);
            _command.previousTerminalNode = terminalNode;
            return true;
        }

        Command command = GetCommand(args);
        if (command == null) return false;
        
        terminalNode = command.Execute(args);
        command.previousTerminalNode = terminalNode;
        return true;
    }
    
    public static void OnLocalDisconnect()
    {
        awaitingConfirmationCommand = null;
    }

    public static void OnTerminalQuit()
    {
        awaitingConfirmationCommand = null;
    }

    private static string[] GetArgs(string[] array, int length)
    {
        if (array.Length >= length) return array;

        List<string> args = array.ToList();

        for (int i = 0; i < length - array.Length; i++)
        {
            args.Add(string.Empty);
        }

        return args.ToArray();
    }

    private static Command GetCommand(string[] args)
    {
        Command result = null;

        commands.ForEach(command =>
        {
            if (result != null) return;

            if (command.IsCommand(args))
            {
                result = command;
            }
        });

        return result;
    }
}
