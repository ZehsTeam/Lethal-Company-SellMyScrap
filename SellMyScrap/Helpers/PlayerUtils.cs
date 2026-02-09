using GameNetcodeStuff;
using Steamworks;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Helpers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal static class PlayerUtils
{
    private static float _previousPlayerMovementSpeed;
    private static float _previousPlayerJumpForce;

    public static PlayerControllerB GetLocalPlayerScript()
    {
        if (GameNetworkManager.Instance == null)
        {
            return null;
        }

        return GameNetworkManager.Instance.localPlayerController;
    }

    public static bool IsLocalPlayer(PlayerControllerB playerScript)
    {
        return playerScript == GetLocalPlayerScript();
    }

    public static PlayerControllerB GetPlayerScriptByClientId(ulong clientId)
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (playerScript.actualClientId == clientId)
            {
                return playerScript;
            }
        }

        return null;
    }

    public static bool AreAllPlayersDead()
    {
        bool result = true;

        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (!playerScript.isPlayerDead)
            {
                result = false;
            }
        }

        return result;
    }

    public static void SetLocalPlayerMovementEnabled(bool enabled)
    {
        PlayerControllerB playerScript = GetLocalPlayerScript();

        // Enabled
        if (enabled)
        {
            if (_previousPlayerMovementSpeed == 0f)
            {
                _previousPlayerMovementSpeed = 4.6f;
            }

            if (_previousPlayerJumpForce == 0f)
            {
                _previousPlayerJumpForce = 13f;
            }

            playerScript.movementSpeed = _previousPlayerMovementSpeed;
            playerScript.jumpForce = _previousPlayerJumpForce;

            return;
        }

        // Disabled

        if (playerScript.movementSpeed > 0f)
        {
            _previousPlayerMovementSpeed = playerScript.movementSpeed;
        }

        if (playerScript.jumpForce > 0f)
        {
            _previousPlayerJumpForce = playerScript.jumpForce;
        }

        playerScript.movementSpeed = 0f;
        playerScript.jumpForce = 0f;
    }

    public static void SetLocalPlayerAllowDeathEnabled(bool enabled)
    {
        if (StartOfRound.Instance == null) return;

        FieldInfo fieldInfo = typeof(StartOfRound).GetField("allowLocalPlayerDeath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo == null) return;

        fieldInfo.SetValue(StartOfRound.Instance, enabled);
    }

    public static void ReviveDeadPlayersAfterTime(float time)
    {
        StartOfRound.Instance.StartCoroutine(ReviveDeadPlayersAfterTimeCO(time));
    }

    private static IEnumerator ReviveDeadPlayersAfterTimeCO(float time)
    {
        yield return new WaitForSeconds(time);

        if (AreAllPlayersDead())
        {
            StartOfRound.Instance.ReviveDeadPlayers();
        }
    }

    #region Other
    public static bool IsLocalPlayer(string username, ulong steamId)
    {
        return SteamClient.Name == username || SteamClient.SteamId == steamId;
    }

    public static bool HasPlayer(string username, ulong steamId)
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (playerScript.playerUsername == username)
            {
                return true;
            }

            if (playerScript.playerSteamId == steamId)
            {
                return true;
            }
        }

        return false;
    }

    // Magoroku
    public static bool IsLocalPlayerMagoroku()
    {
        return IsLocalPlayer(GetMagorokuInfo().Item1, GetMagorokuInfo().Item2);
    }

    public static bool HasPlayerMagoroku()
    {
        return HasPlayer(GetMagorokuInfo().Item1, GetMagorokuInfo().Item2);
    }

    public static bool HasPlayerLunxara()
    {
        return HasPlayer(GetLunxaraInfo().Item1, GetMagorokuInfo().Item2);
    }

    // PsychoHypnotic
    public static bool IsLocalPlayerPsychoHypnotic()
    {
        return IsLocalPlayer(GetPsychoHypnoticInfo().Item1, GetPsychoHypnoticInfo().Item2);
    }

    public static bool HasPlayerPsychoHypnotic()
    {
        return HasPlayer(GetPsychoHypnoticInfo().Item1, GetPsychoHypnoticInfo().Item2);
    }

    private static (string, ulong) GetMagorokuInfo()
    {
        return ("Magoroku", 76561197982837475);
    }

    private static (string, ulong) GetPsychoHypnoticInfo()
    {
        return ("PsychoHypnotic", 76561197970440803);
    }

    private static (string, ulong) GetLunxaraInfo()
    {
        return ("Lunxara", 76561198086325047);
    }
    #endregion
}