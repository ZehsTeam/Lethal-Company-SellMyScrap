using com.github.zehsteam.SellMyScrap.Patches;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Commands;

internal class ViewAllScrapCommand : Command
{
    private List<Item> scrapItems = new List<Item>();
    private int itemsPerPage = 50;
    private int pages;
    private int pageIndex;

    public override bool IsCommand(string[] args)
    {
        args = Utils.GetArrayToLower(args);

        if (args[0] == "view" && args[1] == "all" && args[2] == "scrap") return true;
        if (args[0] == "view-all-scrap") return true;

        return false;
    }

    public override TerminalNode Execute(string[] args)
    {
        scrapItems = ScrapHelper.GetAllScrapItems();
        pages = Mathf.CeilToInt((float)scrapItems.Count / (float)itemsPerPage);
        pageIndex = 0;

        string message = GetMessage();

        awaitingConfirmation = true;

        return TerminalPatch.CreateTerminalNode(message);
    }

    public override TerminalNode ExecuteConfirmation(string[] args)
    {
        string[] _args = Utils.GetArrayToLower(args);

        string[] exitStrings = ["exit", "quit", "q", "close", "leave", "back"];

        if (exitStrings.Contains(_args[0]))
        {
            awaitingConfirmation = false;
            return TerminalPatch.CreateTerminalNode("Closed view all scrap.\n\n");
        }

        if (_args[0] == "next")
        {
            pageIndex++;
            pageIndex = Mathf.Clamp(pageIndex, 0, pages - 1);

            return TerminalPatch.CreateTerminalNode(GetMessage());
        }

        if (_args[0] == "prev")
        {
            pageIndex--;
            pageIndex = Mathf.Clamp(pageIndex, 0, pages - 1);

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

        requestedPage = Mathf.Clamp(requestedPage, 1, pages);
        pageIndex = requestedPage - 1;

        return TerminalPatch.CreateTerminalNode(GetMessage());
    }

    private string GetMessage(string additionMessage = "")
    {
        int index = pageIndex * itemsPerPage;
        int columns = 2;
        int amount = Mathf.Min(scrapItems.Count - index, itemsPerPage);
        List<Item> scrapItemsForPage = scrapItems.GetRange(index, amount);

        string message = $"Found {scrapItems.Count} total items from all moons. (Page {pageIndex + 1} / {pages})\n\n";
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
