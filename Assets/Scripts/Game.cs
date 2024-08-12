using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    [SerializeField] private GameObject rapierPrefab;

    private List<ulong> clients = new ();

    public static Game Instance;
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

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

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (clients.Count < 2)
        {
            clients.Add(request.ClientNetworkId);

            response.Approved = true;
            response.CreatePlayerObject = true;

            switch (clients.Count)
            {
                case 1:
                    response.Position = new Vector3(-6, -1);
                    response.Rotation = Quaternion.identity;
                    break;
                case 2:
                    response.Position = new Vector3(6, -1);
                    response.Rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
        else
        {
            response.Approved = false;
            response.Reason = "Server is full!";
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
            networkObject.TrySetParent(playerObject, false);
        }
    }

    public void RespawnPlayers()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clients[i]].PlayerObject;

            if (i == 0)
            {
                playerObject.transform.position = new Vector3(-6, -1);
                playerObject.transform.rotation = Quaternion.identity;
            }
            else if (i == 1)
            {
                playerObject.transform.position = new Vector3(6, -1);
                playerObject.transform.rotation = Quaternion.Euler(0, 180, 0);;
            }
        }
    }
}
