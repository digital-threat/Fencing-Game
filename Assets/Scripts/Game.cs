using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    [SerializeField] private GameObject rapierPrefab;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
        }
    }

    private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        if (connectionEventData.EventType == ConnectionEvent.ClientConnected)
        {
            var rapier = Instantiate(rapierPrefab);
            var networkObject = rapier.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(connectionEventData.ClientId);
            
            var playerObject = NetworkManager.Singleton.ConnectedClients[connectionEventData.ClientId].PlayerObject;
            networkObject.TrySetParent(playerObject);
        }
    }

    // [Rpc(SendTo.NotServer)]
    // private void RapierSetParentRPC(GameObject rapier)
    // {
    //     var followTarget = rapier.GetComponent<FollowTarget>();
    //     followTarget.target = null;
    // }
}
