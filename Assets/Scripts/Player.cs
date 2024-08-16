using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction emojiAction1;
    [SerializeField] private InputAction emojiAction2;
    [SerializeField] private InputAction emojiAction3;
    
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
            emojiAction1.Enable();
            emojiAction2.Enable();
            emojiAction3.Enable();
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
        var newPosition = transform.position + new Vector3(speed * Time.deltaTime * data, 0);
        newPosition.x = Mathf.Clamp(newPosition.x, -9, 9);

        transform.position = newPosition;
        
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
