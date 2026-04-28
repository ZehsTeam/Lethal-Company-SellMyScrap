using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace com.github.zehsteam.SellMyScrap.Extensions;

internal static class NetworkObjectExtensions
{
    public static IEnumerable<NetworkObjectReference> GetNetworkObjectReferences<T>(IEnumerable<T> input) where T : NetworkBehaviour
    {
        if (input == null) return [];

        return input
            .Where(networkBehaviour => networkBehaviour != null && networkBehaviour.IsSpawned)
            .Select(networkBehaviour => (NetworkObjectReference)networkBehaviour.NetworkObject);
    }

    public static IEnumerable<T> GetNetworkBehaviours<T>(IEnumerable<NetworkObjectReference> input) where T : NetworkBehaviour
    {
        if (input == null) return [];

        List<T> behaviours = [];

        foreach (var item in input)
        {
            if (!item.TryGet(out NetworkObject networkObject))
                continue;

            if (networkObject.TryGetComponent(out T behaviour))
            {
                behaviours.Add(behaviour);
            }
        }

        return behaviours;
    }
}
