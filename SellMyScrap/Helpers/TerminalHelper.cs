using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class TerminalHelper
{
    public static Terminal Instance { get; private set; }

    public const string GreenColor2 = "#007f00";
    public const string GrayColor = "#7f7f7f";
    public const string RedColor = "#ff0000";

    public static void SetInstance(Terminal instance)
    {
        Instance = instance;
    }

    public static TerminalNode CreateTerminalNode(string message, bool clearPreviousText = true, int maxCharactersToType = 50)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = message;
        terminalNode.clearPreviousText = clearPreviousText;
        terminalNode.maxCharactersToType = maxCharactersToType;

        return terminalNode;
    }
}
