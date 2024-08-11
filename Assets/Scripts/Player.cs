using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private InputAction moveAction;
    
    private readonly NetworkVariable<float> moveInput = new ();

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
        
        if (IsServer)
        {
            transform.position -= new Vector3(0, 1);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (moveInput.Value > 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 180))
            {
                var rotation = Quaternion.Euler(0, 0, 0);
                networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
            }
            else if (moveInput.Value < 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 0))
            {
                var rotation = Quaternion.Euler(0, 180, 0);
                networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            var moveValue = moveAction.ReadValue<float>();
            if (moveValue != 0 || moveInput.Value != 0)
            {
                MoveRPC(moveValue);
            }
        }
        
        if (IsServer)
        {
            transform.position += new Vector3(speed * Time.deltaTime * moveInput.Value, 0);
        }
    }

    [Rpc(SendTo.Server)]
    private void MoveRPC(float data)
    {
        moveInput.Value = data;
    }
}
