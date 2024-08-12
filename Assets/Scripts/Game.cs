using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    [SerializeField] private GameObject rapierPrefab;
    [SerializeField] private GameObject scorePrefab;

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
            var noRapier = rapier.GetComponent<NetworkObject>();
            noRapier.SpawnWithOwnership(connectionEventData.ClientId);
            
            var playerObject = NetworkManager.Singleton.ConnectedClients[connectionEventData.ClientId].PlayerObject;
            noRapier.TrySetParent(playerObject, false);

            var score = Instantiate(scorePrefab);
            var noScore = score.GetComponent<NetworkObject>();
            noScore.SpawnWithOwnership(connectionEventData.ClientId);

            var tmpScore = score.GetComponentInChildren<TMP_Text>();
            var rtScore = tmpScore.GetComponent<RectTransform>();

            if (clients.IndexOf(connectionEventData.ClientId) == 0)
            {
                rtScore.anchoredPosition = new Vector2(-50, -15);
            }
            else
            {
                rtScore.anchoredPosition = new Vector2(50, -15);
            }
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
