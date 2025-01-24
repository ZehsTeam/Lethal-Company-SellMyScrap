using Steamworks;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

internal enum PlayerName
{
    CritHaxXoG,
    Takerst,
    Insym,
    Thorlar,
    PsychoHypnotic,
    Magoroku,
    ZombiesAteMyChannel,
    WolfsMyChocolate,
    Hiccubz,
    Lunxara,
    PANDAM0NIEM,
    Jxshkins,
    SpookyTug,
    Raqaizal,
    MaestroSpooks,
    VanoraDarkWolf,
    TetraFlash,
    XSuperBeefyx,
    Ariesgoddess168,
    LustStings,
    TIMETELL,
    IElucian,
    Irishdev13,
    Oopsmyleg,
    Reapermitsuki,
    Yinisin,
    AGlitchedNpc
}

internal static class SteamUtils
{
    public static List<PlayerData> PlayersData { get; private set; } = [];

    static SteamUtils()
    {
        PlayersData = [
            new PlayerData(PlayerName.CritHaxXoG,          username: "CritHaxXoG",         steamId: 76561198993712632),
            new PlayerData(PlayerName.Takerst,             username: "Takerst",            steamId: 76561197980238122),
            new PlayerData(PlayerName.Insym,               username: "Insym",              steamId: 76561198008109303),
            new PlayerData(PlayerName.Thorlar,             username: "Thorlar",            steamId: 76561197964616102),
            new PlayerData(PlayerName.PsychoHypnotic,      username: "PsychoHypnotic",     steamId: 76561197970440803),
            new PlayerData(PlayerName.Magoroku,            username: "Magoroku",           steamId: 76561197982837475),
            new PlayerData(PlayerName.ZombiesAteMyChannel, username: "ZombieAteMyChannel", steamId: 76561197990822861),
            new PlayerData(PlayerName.WolfsMyChocolate,    username: ["WolfsMyChocolate", "wolfyfook"], steamId: [76561199112488706, 76561199589529626]),
            new PlayerData(PlayerName.Hiccubz,             username: "Hiccubz",            steamId: 76561199090697084),
            new PlayerData(PlayerName.Lunxara,             username: "Lunxara",            steamId: 76561198086325047),
            new PlayerData(PlayerName.PANDAM0NIEM,         username: "PANDAM0NIEM",        steamId: 76561199169070660),
            new PlayerData(PlayerName.Jxshkins,            username: "jxshkins",           steamId: 76561199004549621),
            new PlayerData(PlayerName.SpookyTug,           username: "SpookyTug",          steamId: 76561198053549767),
            new PlayerData(PlayerName.Raqaizal,            username: "Raqaizal",           steamId: 76561198047992629),
            new PlayerData(PlayerName.MaestroSpooks,       username: "MaestroSpooks",      steamId: 76561198150757587),
            new PlayerData(PlayerName.VanoraDarkWolf,      username: "Vanora",             steamId: 76561198064529684),
            new PlayerData(PlayerName.TetraFlash,          username: "TetraFlash",         steamId: 76561197993458924),
            new PlayerData(PlayerName.XSuperBeefyx,        username: "xSuperBeefyx",       steamId: 76561197964982016),
            new PlayerData(PlayerName.Ariesgoddess168,     username: "NewAgeAries",        steamId: 76561198174632877),
            new PlayerData(PlayerName.LustStings,          username: "lustings",           steamId: 76561198053068793),
            new PlayerData(PlayerName.TIMETELL,            username: "TIMETELL",           steamId: 76561197967817500),
            new PlayerData(PlayerName.IElucian,            username: "iElucian",           steamId: 76561197962979730),
            new PlayerData(PlayerName.Irishdev13,          username: "TTV_Irishdev13",     steamId: 76561198123000350),
            new PlayerData(PlayerName.Oopsmyleg,           username: "Oopsmyleg",          steamId: 76561198113371889),
            new PlayerData(PlayerName.Reapermitsuki,       username: "reaper-mitsuki",     steamId: 76561198281494776),
            new PlayerData(PlayerName.Yinisin,             username: "Yinisin",            steamId: 76561199582073183),
            new PlayerData(PlayerName.AGlitchedNpc,        username: "a glitched npc",     steamId: 76561198984467725)
        ];
    }

    public static bool IsPlayer(PlayerName playerName, string username, ulong steamId)
    {
        PlayerData playerData = GetPlayerData(playerName);
        if (playerData == null) return false;

        return playerData.IsPlayer(username, steamId);
    }

    public static PlayerData GetLocalClientPlayerData()
    {
        foreach (var playerData in PlayersData)
        {
            if (playerData.IsLocalClient())
            {
                return playerData;
            }
        }

        return null;
    }

    public static PlayerData GetPlayerData(PlayerName playerName)
    {
        foreach (var playerData in PlayersData)
        {
            if (playerData.PlayerName == playerName)
            {
                return playerData;
            }
        }

        return null;
    }

    public static bool IsLocalClient(PlayerName playerName)
    {
        PlayerData playerData = GetPlayerData(playerName);
        if (playerData == null) return false;

        return playerData.IsLocalClient();
    }

    public static bool IsLocalClient(string username, ulong steamId)
    {
        if (!SteamClient.IsValid) return false;
        if (!SteamClient.IsLoggedOn) return false;

        if (SteamClient.Name.Equals(username, System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (SteamClient.SteamId == steamId) return true;

        return false;
    }
}

internal class PlayerData
{
    public PlayerName PlayerName { get; private set; }
    public string[] Username { get; private set; }
    public ulong[] SteamId { get; private set; }

    public PlayerData(PlayerName playerName, string username, ulong steamId)
    {
        PlayerName = playerName;
        Username = [username];
        SteamId = [steamId];
    }

    public PlayerData(PlayerName playerName, string[] username, ulong[] steamId)
    {
        PlayerName = playerName;
        Username = username;
        SteamId = steamId;
    }

    public bool IsLocalClient()
    {
        for (int i = 0; i < Username.Length; i++)
        {
            if (SteamUtils.IsLocalClient(Username[i], SteamId[i]))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPlayer(string username, ulong steamId)
    {
        for (int i = 0; i < Username.Length; i++)
        {
            if (Username[i].Equals(username, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (SteamId[i] == steamId)
            {
                return true;
            }
        }

        return false;
    }
}
