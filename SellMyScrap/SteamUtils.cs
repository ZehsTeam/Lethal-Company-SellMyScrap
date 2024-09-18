using Steamworks;
using System.Collections.Generic;

namespace com.github.zehsteam.SellMyScrap;

internal enum PlayerName
{
    Takerst,
    ZombiesAteMyChannel,
    Insym,
    Thorlar,
    PsychoHypnotic,
    Magoroku,
    IElucian
}

internal static class SteamUtils
{
    public static List<PlayerData> PlayersData { get; private set; } = [];

    public static void Initialize()
    {
        PlayersData = [
            new PlayerData(PlayerName.Takerst,             username: "Takerst",            steamId: 76561197980238122),
            new PlayerData(PlayerName.ZombiesAteMyChannel, username: "ZombieAteMyChannel", steamId: 76561197990822861),
            new PlayerData(PlayerName.Insym,               username: "Insym",              steamId: 76561198008109303),
            new PlayerData(PlayerName.Thorlar,             username: "Thorlar",            steamId: 76561197964616102),
            new PlayerData(PlayerName.PsychoHypnotic,      username: "PsychoHypnotic",     steamId: 76561197970440803),
            new PlayerData(PlayerName.Magoroku,            username: "Magoroku",           steamId: 76561197982837475),
            new PlayerData(PlayerName.IElucian,            username: "iElucian",           steamId: 76561197962979730),
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
