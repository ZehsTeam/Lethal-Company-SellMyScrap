using com.github.zehsteam.SellMyScrap.Extensions;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.SellMyScrap.Helpers;

internal static class NetworkUtils
{
    private static readonly FieldInfo _rpcExecStageField = AccessTools.Field(typeof(NetworkBehaviour), "__rpc_exec_stage");

    public static bool IsConnected => NetworkManager.Singleton?.IsConnectedClient ?? false;
    public static bool IsServer => NetworkManager.Singleton?.IsServer ?? false;

    public static ulong LocalClientId => NetworkManager.Singleton?.LocalClientId ?? 0;

    public static int ConnectedPlayerCount => GameNetworkManager.Instance?.connectedPlayers ?? 0;

    public static bool IsLocalClientId(ulong clientId)
    {
        return clientId == LocalClientId;
    }

    public static bool HasClient(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
            return false;

        return NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId);
    }

    public static bool IsNetworkPrefab(GameObject prefab)
    {
        if (prefab == null)
            return false;

        if (NetworkManager.Singleton == null)
            return false;

        var prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        return prefabs.Any(x => x.Prefab == prefab);
    }

    public static void NetcodePatcherAwake()
    {
        try
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetLoadableTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    try
                    {
                        // Safely attempt to retrieve custom attributes
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                        if (attributes.Length > 0)
                        {
                            try
                            {
                                // Safely attempt to invoke the method
                                method.Invoke(null, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                Logger.LogWarning($"[{nameof(NetworkUtils)}] Failed to invoke method {method.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle errors when fetching custom attributes, due to missing types or dependencies
                        Logger.LogWarning($"[{nameof(NetworkUtils)}] Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catch any general exceptions that occur in the process
            Logger.LogError($"[{nameof(NetworkUtils)}] Failed to run NetcodePatcherAwake: {ex.Message}");
        }
    }

    /// <summary>
    /// This is used for when you are patching an RPC method and need to check if that method is actually executing the method body or sending the RPC.
    /// </summary>
    /// <param name="networkBehaviour">The instance of <see cref="NetworkBehaviour"/> where the original method is on.</param>
    /// <returns>True if the method is actually executing the method body. False if the method is sending the RPC.</returns>
    public static bool IsExecutingRPCMethod(NetworkBehaviour networkBehaviour)
    {
        if (networkBehaviour == null)
            return false;

        NetworkManager networkManager = networkBehaviour.NetworkManager;

        if (networkManager == null || !networkManager.IsListening)
            return false;

        if (_rpcExecStageField == null)
        {
            Logger.LogError($"[{nameof(NetworkUtils)}] {nameof(IsExecutingRPCMethod)}: Failed to find \"__rpc_exec_stage\" field.");
            return false;
        }

        object rpcExecStageValue = _rpcExecStageField.GetValue(networkBehaviour);

        //protected enum Unity.Netcode.NetworkBehaviour.__RpcExecStage
        //{
        //    Send = 0,
        //    Execute = 1,
        //    None = 0,
        //    Server = 1,
        //    Client = 2
        //}
        int rpcExecStageInt = (int)rpcExecStageValue;

        //Logger.LogInfo($"[{networkBehaviour.GetType().Name}] rpcExecStageInt: {rpcExecStageInt}", extended: true);

        return rpcExecStageInt == 0 || rpcExecStageInt == 1;
    }
}
