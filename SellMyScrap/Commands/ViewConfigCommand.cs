using com.github.zehsteam.SellMyScrap.Patches;
using Newtonsoft.Json;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewConfigCommand : Command
{
    public override bool IsCommand(string[] args)
    {
        if (args[0] == "view" && args[1] == "config") return true;
        if (args[0] == "view-config") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        SyncedConfig configManager = SellMyScrapBase.Instance.ConfigManager;

        bool isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        string syncedMessage = isHostOrServer ? string.Empty : " (Synced with host)";

        string message = $"SellMyScrap v{MyPluginInfo.PLUGIN_VERSION} config\n\n";
        message += $"[Sell Settings]{syncedMessage}\n";
        message += $"sellGifts:    {configManager.SellGifts}\n";
        message += $"sellShotguns: {configManager.SellShotguns}\n";
        message += $"sellAmmo:     {configManager.SellAmmo}\n";
        message += $"sellPickles:  {configManager.SellPickles}\n\n";
        message += $"[Advanced Sell Settings]{syncedMessage}\n";
        message += $"dontSellListJson: {JsonConvert.SerializeObject(configManager.DontSellListJson)}\n\n";
        message += "[Terminal Settings]\n";
        message += $"overrideWelcomeMessage: {configManager.OverrideWelcomeMessage}\n";
        message += $"overrideHelpMessage:    {configManager.OverrideHelpMessage}\n";
        message += $"showFoundItems:         {configManager.ShowFoundItems}\n";
        message += $"sortFoundItems:         {configManager.SortFoundItems}\n";
        message += $"alignFoundItemsPrice:   {configManager.AlignFoundItemsPrice}\n\n";
        message += "[Misc Settings]\n";
        message += $"speakInShip: {configManager.SpeakInShip}\n\n";

        return TerminalPatch.CreateTerminalNode(message);
    }
}
