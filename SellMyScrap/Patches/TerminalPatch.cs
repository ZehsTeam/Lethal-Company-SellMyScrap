using com.github.zehsteam.SellMyScrap.Commands;
using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalPatch
{
    public static Terminal Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<Terminal>();
            }

            return _instance;
        }
    }

    private static Terminal _instance;

    public const string GreenColor2 = "#007f00";
    public const string GrayColor = "#7f7f7f";
    public const string RedColor = "#ff0000";

    private static bool hasOverrideTerminalNodes = false;

    [HarmonyPatch(nameof(Terminal.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void StartPatch(ref TerminalNodesList ___terminalNodes)
    {
        OverrideTerminalNodes(___terminalNodes);
    }

    #region TerminalNode Overrides
    private static void OverrideTerminalNodes(TerminalNodesList terminalNodes)
    {
        if (hasOverrideTerminalNodes) return;
        hasOverrideTerminalNodes = true;

        if (Plugin.ConfigManager.OverrideWelcomeMessage.Value)
        {
            OverrideWelcomeTerminalNode(terminalNodes);
        }

        if (Plugin.ConfigManager.OverrideHelpMessage.Value)
        {
            OverrideHelpTerminalNode(terminalNodes);
        }
    }

    private static void OverrideWelcomeTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 1;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;

        string messageToReplace = "Type \"Help\" for a list of commands.";
        string message = defaultMessage.Replace(messageToReplace, $"{messageToReplace}\n\n[{MyPluginInfo.PLUGIN_NAME}]\nType \"Sell\" for a list of commands.");

        terminalNodes.specialNodes[index].displayText = message;
    }

    private static void OverrideHelpTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 13;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;

        string messageToReplace = ">OTHER\nTo see the list of other commands";
        string message = defaultMessage.Replace(messageToReplace, $"{messageToReplace}.\n\n>SELL\nTo see the list of {MyPluginInfo.PLUGIN_NAME} commands.");

        terminalNodes.specialNodes[index].displayText = message;
    }
    #endregion

    [HarmonyPatch(nameof(Terminal.QuitTerminal))]
    [HarmonyPostfix]
    private static void QuitTerminalPatch()
    {
        Plugin.Instance.OnTerminalQuit();
    }

    [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ParsePlayerSentencePatch(ref Terminal __instance, ref TerminalNode __result)
    {
        string[] array = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        if (CommandManager.TryExecuteCommand(array, out TerminalNode terminalNode))
        {
            if (terminalNode == null)
            {
                __result = CreateTerminalNode("TerminalNode is null!\n\n");
                return false;
            }

            __result = terminalNode;
            return false;
        }

        return true;
    }

    public static TerminalNode CreateTerminalNode(string message, bool clearPreviousText = true)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = message;
        terminalNode.clearPreviousText = clearPreviousText;
        terminalNode.maxCharactersToType = 50;

        return terminalNode;
    }
}
