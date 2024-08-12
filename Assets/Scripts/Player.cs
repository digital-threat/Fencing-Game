using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, IPlayer
{
    [SerializeField] private float speed;
    [SerializeField] private InputAction moveAction;
    
    private NetworkTransform networkTransform;
    

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        if (IsLocalPlayer)
        {
            moveAction.Enable();
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            var moveValue = moveAction.ReadValue<float>();
            if (moveValue != 0)
            {
                MoveRPC(moveValue);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void MoveRPC(float data)
    {
        transform.position += new Vector3(speed * Time.deltaTime * data, 0);
        
        if (data > 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 180))
        {
            var rotation = Quaternion.Euler(0, 0, 0);
            networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
        }
        else if (data < 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 0))
        {
            var rotation = Quaternion.Euler(0, 180, 0);
            networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
        }
    }
}
