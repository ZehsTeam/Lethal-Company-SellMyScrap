using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap;

public class PlayerUtils
{
    private static float previousPlayerMovementSpeed;
    private static float previousPlayerJumpForce;

    private static ulong magorokuSteamId = 76561197982837475;
    private static string magorokuUsername = "Magoroku";

    public static bool HasPlayerMagoroku()
    {
        return HasPlayer(magorokuSteamId, magorokuUsername);
    }

    public static bool IsLocalPlayerMagoroku()
    {
        return IsLocalPlayer(magorokuSteamId, magorokuUsername);
    }

    public static bool HasPlayer(ulong steamId, string username)
    {
        foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
        {
            if (IsPlayer(playerScript, steamId, username))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsLocalPlayer(ulong steamId, string username)
    {
        return IsPlayer(GetLocalPlayerScript(), steamId, username);
    }

    private static bool IsPlayer(PlayerControllerB playerScript, ulong steamId, string username)
    {
        if (playerScript.playerSteamId == steamId) return true;
        if (playerScript.playerUsername == username) return true;

        return false;
    }

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
            if (previousPlayerMovementSpeed == 0f)
            {
                previousPlayerMovementSpeed = 4.6f;
            }

            if (previousPlayerJumpForce == 0f)
            {
                previousPlayerJumpForce = 13f;
            }

            playerScript.movementSpeed = previousPlayerMovementSpeed;
            playerScript.jumpForce = previousPlayerJumpForce;

            return;
        }

        // Disabled

        if (playerScript.movementSpeed > 0f)
        {
            previousPlayerMovementSpeed = playerScript.movementSpeed;
        }

        if (playerScript.jumpForce > 0f)
        {
            previousPlayerJumpForce = playerScript.jumpForce;
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