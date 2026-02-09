using com.github.zehsteam.SellMyScrap.Commands;
using com.github.zehsteam.SellMyScrap.Helpers;
using HarmonyLib;
using System;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalPatch
{
    public static Terminal Instance { get; private set; }

    private static bool _hasOverrideTerminalNodes = false;

    [HarmonyPatch(nameof(Terminal.Awake))]
    [HarmonyPrefix]
    private static void AwakePatch(Terminal __instance)
    {
        Instance = __instance;
    }

    [HarmonyPatch(nameof(Terminal.Start))]
    [HarmonyPrefix]
    private static void StartPatchPrefix(ref Terminal __instance)
    {
        TerminalHelper.SetInstance(__instance);
    }

    [HarmonyPatch(nameof(Terminal.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void StartPatchPostfix(ref TerminalNodesList ___terminalNodes)
    {
        OverrideTerminalNodes(___terminalNodes);
    }

    #region TerminalNode Overrides
    private static void OverrideTerminalNodes(TerminalNodesList terminalNodes)
    {
        if (_hasOverrideTerminalNodes) return;
        _hasOverrideTerminalNodes = true;

        if (ConfigManager.OverrideWelcomeMessage.Value)
        {
            OverrideWelcomeTerminalNode(terminalNodes);
        }

        if (ConfigManager.OverrideHelpMessage.Value)
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
        Plugin.HandleTerminalQuit();
    }

    [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ParsePlayerSentencePatch(ref Terminal __instance, ref TerminalNode __result)
    {
        string[] array = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (CommandManager.TryExecuteCommand(array, out TerminalNode terminalNode))
        {
            if (terminalNode == null)
            {
                __result = TerminalHelper.CreateTerminalNode("TerminalNode is null!\n\n");
                return false;
            }

            __result = terminalNode;
            return false;
        }

        return true;
    }
}
