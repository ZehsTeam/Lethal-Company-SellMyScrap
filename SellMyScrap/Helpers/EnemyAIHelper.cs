using GameNetcodeStuff;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class EnemyAIHelper
{
    public static void HitEnemyOnLocalClient(EnemyAI enemyAI, int force = 1, Vector3 hitDirection = default, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        if (enemyAI == null)
        {
            Plugin.Logger.LogError("EnemyAI instance is null!");
            return;
        }

        MethodInfo method = typeof(EnemyAI).GetMethod("HitEnemyOnLocalClient", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Plugin.Logger.LogError("HitEnemyOnLocalClient method not found on EnemyAI!");
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();

        object[] args;

        if (parameters.Length == 5) // v69
        {
            args = [force, hitDirection, playerWhoHit, playHitSFX, hitID];
        }
        else if (parameters.Length == 4) // v40
        {
            args = [force, hitDirection, playerWhoHit, playHitSFX];
        }
        else
        {
            Debug.LogError("Unexpected HitEnemyOnLocalClient method signature!");
            return;
        }

        method.Invoke(enemyAI, args);
    }

    public static void HitFromExplosion(EnemyAI enemyAI, float distance)
    {
        if (enemyAI == null)
        {
            Plugin.Logger.LogError("EnemyAI instance is null!");
            return;
        }

        MethodInfo method = typeof(EnemyAI).GetMethod("HitFromExplosion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Plugin.Logger.LogWarning("HitFromExplosion method not found on EnemyAI!");
            return;
        }

        object[] args = [distance];

        method.Invoke(enemyAI, args);
    }
}
