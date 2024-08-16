using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    [SerializeField] private GameObject rapierPrefab;
    [SerializeField] private GameObject scorePrefab;

    private enum Side
    {
        LEFT,
        RIGHT
    }
    
    private class Client
    {
        public readonly Side side;
        public Score score;
        public Rapier rapier;

        public Client(Side side)
        {
            this.side = side;
        }
    }

    private readonly Dictionary<ulong, Client> clients = new ();

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
            response.Approved = true;
            response.CreatePlayerObject = true;

            switch (clients.Count)
            {
                case 0:
                    clients.Add(request.ClientNetworkId, new Client(Side.LEFT));
                    response.Position = new Vector3(-6, -1);
                    response.Rotation = Quaternion.identity;
                    break;
                case 1:
                    clients.Add(request.ClientNetworkId, new Client(Side.RIGHT));
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
            var noRapier = rapier.GetComponent<NetworkObject>();
            noRapier.SpawnWithOwnership(connectionEventData.ClientId);
            
            var playerObject = NetworkManager.Singleton.ConnectedClients[connectionEventData.ClientId].PlayerObject;
            noRapier.TrySetParent(playerObject, false);
            
            var score = Instantiate(scorePrefab);
            var noScore = score.GetComponent<NetworkObject>();
            var cpScore = score.GetComponent<Score>();
            
            noScore.SpawnWithOwnership(connectionEventData.ClientId);

            if (clients[connectionEventData.ClientId].side == Side.LEFT)
            {
                cpScore.SetPosition(new Vector2(-50, -15));
            }
            else
            {
                cpScore.SetPosition(new Vector2(50, -15));
            }

            clients[connectionEventData.ClientId].score = cpScore;
            clients[connectionEventData.ClientId].rapier = rapier.GetComponent<Rapier>();
        }
        else if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            var noScore = clients[connectionEventData.ClientId].score.GetComponent<NetworkObject>();
            noScore.Despawn();
            Destroy(noScore.gameObject);
            
            var noRapier = clients[connectionEventData.ClientId].rapier.GetComponent<NetworkObject>();
            noRapier.Despawn();
            Destroy(noRapier.gameObject);
        }
    }

    public void RespawnPlayers()
    {
        foreach (var client in clients)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[client.Key].PlayerObject;
            
            if (client.Value.side == Side.LEFT)
            {
                playerObject.transform.position = new Vector3(-6, -1);
                playerObject.transform.rotation = Quaternion.identity;
            }
            else
            {
                playerObject.transform.position = new Vector3(6, -1);
                playerObject.transform.rotation = Quaternion.Euler(0, 180, 0);;
            }
        }
    }

    public void UpdateScore(ulong winnerClientId, ulong loserClientId)
    {
        if (clients.TryGetValue(winnerClientId, out var winner))
        {
            winner.score.IncrementScore();
        }
        
        if (clients.TryGetValue(loserClientId, out var loser))
        {
            loser.score.DecrementScore();
        }
    }

    public Rapier.Stance GetStance(ulong clientId)
    {
        return clients[clientId].rapier.GetStance();
    }
}
