using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void OnGUI()
    {
        if (GUILayout.Button("Host"))
        {
            networkManager.StartHost();
        }

        if (GUILayout.Button("Join"))
        {
            networkManager.StartClient();
        }
    }

    private void Awake()
    {
        //GetComponent<UnityTransport>().SetDebugSimulatorParameters(packetDelay: 25, packetJitter: 2, dropRate: 1);
    }
}
