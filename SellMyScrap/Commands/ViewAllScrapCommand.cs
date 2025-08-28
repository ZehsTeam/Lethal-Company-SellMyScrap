using com.github.zehsteam.SellMyScrap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewAllScrapCommand : Command
{
    private List<Item> _scrapItems = [];
    private int _itemsPerPage = 50;
    private int _pages;
    private int _pageIndex;

    public override bool IsCommand(ref string[] args)
    {
        if (MatchesPattern(ref args, "view", "all", "scrap")) return true;
        if (MatchesPattern(ref args, "sell-all-scrap")) return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        _scrapItems = ScrapHelper.GetAllScrapItems();
        _pages = Mathf.CeilToInt((float)_scrapItems.Count / (float)_itemsPerPage);
        _pageIndex = 0;

        string message = GetMessage();

        Logger.LogInfo($"\n\n{ScrapHelper.GetScrapItemMessage(_scrapItems)}\n");

        AwaitingConfirmation = true;

        return TerminalHelper.CreateTerminalNode(message);
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];

        if (exitStrings.Contains(args[0].ToLower()))
        {
            AwaitingConfirmation = false;
            return TerminalHelper.CreateTerminalNode("Closed view all scrap.\n\n");
        }

        if ("next".Contains(args[0], StringComparison.OrdinalIgnoreCase))
        {
            _pageIndex++;
            _pageIndex = Mathf.Clamp(_pageIndex, 0, _pages - 1);

            return TerminalHelper.CreateTerminalNode(GetMessage());
        }

        if ("previous".Contains(args[0], StringComparison.OrdinalIgnoreCase))
        {
            _pageIndex--;
            _pageIndex = Mathf.Clamp(_pageIndex, 0, _pages - 1);

            return TerminalHelper.CreateTerminalNode(GetMessage());
        }

        if (!"page".Contains(args[0], StringComparison.OrdinalIgnoreCase))
        {
            return TerminalHelper.CreateTerminalNode(GetMessage("Error: invalid command.\n\n"));
        }

        if (!int.TryParse(args[1], out int requestedPage))
        {
            return TerminalHelper.CreateTerminalNode(GetMessage("Error: invalid page number.\n\n"));
        }

        requestedPage = Mathf.Clamp(requestedPage, 1, _pages);
        _pageIndex = requestedPage - 1;

        return TerminalHelper.CreateTerminalNode(GetMessage());
    }

    private string GetMessage(string additionMessage = "")
    {
        int index = _pageIndex * _itemsPerPage;
        int columns = 2;
        int amount = Mathf.Min(_scrapItems.Count - index, _itemsPerPage);
        List<Item> scrapItemsForPage = _scrapItems.GetRange(index, amount);

        var builder = new StringBuilder();

        builder.AppendLine($"Found {_scrapItems.Count} total items from all moons. (Page {_pageIndex + 1} / {_pages})\n");
        builder.AppendLine($"{ScrapHelper.GetScrapItemMessage(scrapItemsForPage, columns, 26)}\n");
        builder.AppendLine("The following commands are available:\n");
        builder.AppendLine("page <number>");
        builder.AppendLine("next");
        builder.AppendLine("prev");
        builder.AppendLine("exit\n");
        builder.Append(additionMessage);

        return builder.ToString();
    }
}
