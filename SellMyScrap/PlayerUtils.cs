using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
internal static class PlayerUtils
{
    private static float _previousPlayerMovementSpeed;
    private static float _previousPlayerJumpForce;

    public static PlayerControllerB GetLocalPlayerScript()
    {
        if (GameNetworkManager.Instance == null) return null;
        return GameNetworkManager.Instance.localPlayerController;
    }

    public static bool IsLocalPlayer(PlayerControllerB playerScript)
    {
        return playerScript == GetLocalPlayerScript();
    }

    public static bool IsLocalPlayer(PlayerName playerName)
    {
        return SteamUtils.IsLocalClient(playerName);
    }

    public static bool IsLocalPlayer(PlayerName[] playerNames)
    {
        foreach (var playerName in playerNames)
        {
            if (SteamUtils.IsLocalClient(playerName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPlayer(PlayerControllerB playerScript, PlayerName playerName)
    {
        if (playerScript == null) return false;

        return SteamUtils.IsPlayer(playerName, playerScript.playerUsername, playerScript.playerSteamId);
    }

    public static bool HasPlayer(PlayerName playerName)
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (IsPlayer(playerScript, playerName))
            {
                return true;
            }
        }

        return false;
    }

    public static PlayerControllerB GetPlayerScript(PlayerName playerName)
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (IsPlayer(playerScript, playerName))
            {
                return playerScript;
            }
        }

        return null;
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
        StartOfRound.Instance.allowLocalPlayerDeath = enabled;
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
}