using com.github.zehsteam.SellMyScrap.Commands;
using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Patches;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    private static bool hasOverrideTerminalNodes = false;

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    [HarmonyPriority(int.MaxValue)]
    static void StartPatch(ref TerminalNodesList ___terminalNodes)
    {
        OverrideTerminalNodes(___terminalNodes);
    }

    #region TerminalNode Overrides
    private static void OverrideTerminalNodes(TerminalNodesList terminalNodes)
    {
        if (hasOverrideTerminalNodes) return;
        hasOverrideTerminalNodes = true;

        if (SellMyScrapBase.Instance.ConfigManager.OverrideWelcomeMessage) OverrideWelcomeTerminalNode(terminalNodes);
        if (SellMyScrapBase.Instance.ConfigManager.OverrideHelpMessage) OverrideHelpTerminalNode(terminalNodes);
    }

    private static void OverrideWelcomeTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 1;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;
        
        string message = defaultMessage.Trim();
        message += $"\n\n[SellMyScrap]\nType \"Sell\" for a list of commands.\n\n\n\n";

        terminalNodes.specialNodes[index].displayText = message;
    }

    private static void OverrideHelpTerminalNode(TerminalNodesList terminalNodes)
    {
        int index = 13;
        string defaultMessage = terminalNodes.specialNodes[index].displayText;
        
        string message = defaultMessage.Replace("[numberOfItemsOnRoute]", "").Trim();
        message += "\n\n>SELL\nTo see the list of SellMyScrap commands.\n\n\n[numberOfItemsOnRoute]";

        terminalNodes.specialNodes[index].displayText = message;
    }
    #endregion

    [HarmonyPatch("QuitTerminal")]
    [HarmonyPostfix]
    static void QuitTerminalPatch()
    {
        SellMyScrapBase.Instance.OnTerminalQuit();
    }

    [HarmonyPatch("ParsePlayerSentence")]
    [HarmonyPrefix]
    [HarmonyPriority(int.MaxValue)]
    static bool ParsePlayerSentencePatch(ref Terminal __instance, ref TerminalNode __result)
    {
        string[] array = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).Split(' ');

        if (CommandManager.TryExecuteCommand(array, out TerminalNode terminalNode))
        {
            if (terminalNode == null)
            {
                __result = CreateTerminalNode("Error: terminalNode is null!\n\n");
                return false;
            }

            __result = terminalNode;
            return false;
        }

        return true;
    }

    public static TerminalNode CreateTerminalNode(string message)
    {
        return CreateTerminalNode(message, true);
    }

    public static TerminalNode CreateTerminalNode(string message, bool clearPreviousText)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();

        terminalNode.displayText = message;
        terminalNode.clearPreviousText = clearPreviousText;
        terminalNode.maxCharactersToType = 50;

        return terminalNode;
    }
}
