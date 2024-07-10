using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewAllScrapCommand : Command
{
    private List<Item> _scrapItems = [];
    private int _itemsPerPage = 50;
    private int _pages;
    private int _pageIndex;

    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "view" && args[1] == "all" && args[2] == "scrap") return true;
        if (args[0] == "view-all-scrap") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        _scrapItems = ScrapHelper.GetAllScrapItems();
        _pages = Mathf.CeilToInt((float)_scrapItems.Count / (float)_itemsPerPage);
        _pageIndex = 0;

        string message = GetMessage();

        Plugin.logger.LogInfo($"\n\n{ScrapHelper.GetScrapItemMessage(_scrapItems)}\n");

        AwaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(message);
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        string[] _args = Utils.GetArrayToLower(args);

        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];

        if (exitStrings.Contains(_args[0]))
        {
            AwaitingConfirmation = false;
            return TerminalPatch.CreateTerminalNode("Closed view all scrap.\n\n");
        }

        if (_args[0] == "next" || _args[0] == "n")
        {
            _pageIndex++;
            _pageIndex = Mathf.Clamp(_pageIndex, 0, _pages - 1);

            return TerminalPatch.CreateTerminalNode(GetMessage());
        }

        if (_args[0] == "prev" || _args[0] == "p")
        {
            _pageIndex--;
            _pageIndex = Mathf.Clamp(_pageIndex, 0, _pages - 1);

            return TerminalPatch.CreateTerminalNode(GetMessage());
        }

        if (_args[0] != "page")
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid command.\n\n"));
        }

        if (!int.TryParse(args[1], out int requestedPage))
        {
            return TerminalPatch.CreateTerminalNode(GetMessage("Error: invalid page number.\n\n"));
        }

        requestedPage = Mathf.Clamp(requestedPage, 1, _pages);
        _pageIndex = requestedPage - 1;

        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    private string GetMessage(string additionMessage = "")
    {
        int index = _pageIndex * _itemsPerPage;
        int columns = 2;
        int amount = Mathf.Min(_scrapItems.Count - index, _itemsPerPage);
        List<Item> scrapItemsForPage = _scrapItems.GetRange(index, amount);

        string message = $"Found {_scrapItems.Count} total items from all moons. (Page {_pageIndex + 1} / {_pages})\n\n";
        message += $"{ScrapHelper.GetScrapItemMessage(scrapItemsForPage, columns, 26)}\n\n";
        message += "The following commands are available:\n\n";
        message += "page <number>\n";
        message += "next\n";
        message += "prev\n";
        message += "exit\n\n";
        message += additionMessage;

        return message;
    }
}
