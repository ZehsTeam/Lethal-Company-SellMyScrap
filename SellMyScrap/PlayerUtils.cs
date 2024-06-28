using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class PlayerUtils
{
    private static float _previousPlayerMovementSpeed;
    private static float _previousPlayerJumpForce;

    #region Magoroku
    public static bool HasPlayerMagoroku()
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (IsPlayerMagoroku(playerScript)) return true;
        }

        return false;
    }

    public static bool IsLocalPlayerMagoroku()
    {
        return IsPlayerMagoroku(GetLocalPlayerScript());
    }

    public static bool IsPlayerMagoroku(PlayerControllerB playerScript)
    {
        if (playerScript.playerSteamId == 76561197982837475) return true;
        if (playerScript.playerUsername == "Magoroku") return true;

        return false;
    }
    #endregion

    public static PlayerControllerB GetLocalPlayerScript()
    {
        return GameNetworkManager.Instance.localPlayerController;
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