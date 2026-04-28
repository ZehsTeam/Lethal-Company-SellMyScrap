using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class PlayerIdentityUtils
{
    public static PlayerIdentity MagorokuIdentity { get; private set; } = new PlayerIdentity("Magoroku", 76561197982837475);
    public static PlayerIdentity PsychoHypnoticIdentity { get; private set; } = new PlayerIdentity("PsychoHypnotic", 76561197970440803);
    public static PlayerIdentity LunxaraIdentity { get; private set; } = new PlayerIdentity("Lunxara", 76561198086325047);

    public static bool IsPlayer(PlayerControllerB playerScript, PlayerIdentity identity)
    {
        if (playerScript == null)
            return false;

        if (playerScript.playerUsername.Equals(identity.Username, StringComparison.OrdinalIgnoreCase))
            return true;

        if (playerScript.playerSteamId == identity.SteamId)
            return true;

        return false;
    }

    public static bool IsLocalPlayer(PlayerIdentity identity)
    {
        if (SteamClient.IsValid)
        {
            if (SteamClient.Name.Equals(identity.Username, StringComparison.OrdinalIgnoreCase))
                return true;

            if (SteamClient.SteamId == identity.SteamId)
                return true;
        }

        return IsPlayer(PlayerUtils.LocalPlayerScript, identity);
    }

    public static bool TryGetPlayer(PlayerIdentity identity, out PlayerControllerB playerScript)
    {
        if (PlayerUtils.ConnectedPlayerScripts.Length == 0)
        {
            Logger.LogWarning($"[{nameof(PlayerIdentityUtils)}] {nameof(TryGetPlayer)}: PlayerScripts list is empty!");

            playerScript = null;
            return false;
        }

        playerScript = PlayerUtils.ConnectedPlayerScripts.FirstOrDefault(x => IsPlayer(x, identity));
        return playerScript != null;
    }

    public static bool HasPlayer(PlayerIdentity identity)
    {
        return TryGetPlayer(identity, out _);
    }



    public static bool IsPlayerAny(PlayerControllerB playerScript, IEnumerable<PlayerIdentity> identities)
    {
        return identities.Any(x => IsPlayer(playerScript, x));
    }

    public static bool IsLocalPlayerAny(IEnumerable<PlayerIdentity> identities)
    {
        return identities.Any(IsLocalPlayer);
    }

    public static bool TryGetPlayerAny(IEnumerable<PlayerIdentity> identities, out PlayerControllerB playerScript)
    {
        foreach (var identity in identities)
        {
            if (TryGetPlayer(identity, out playerScript))
            {
                return true;
            }
        }

        playerScript = null;
        return false;
    }

    public static bool HasPlayerAny(IEnumerable<PlayerIdentity> identities)
    {
        return identities.Any(HasPlayer);
    }
}

internal struct PlayerIdentity
{
    public string Username { get; set; }
    public ulong SteamId { get; set; }

    public PlayerIdentity(string username, ulong steamId)
    {
        Username = username;
        SteamId = steamId;
    }
}
