using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    private enum RapierPosition
    {
        BOTTOM,
        MIDDLE,
        TOP
    }

    [SerializeField] private Transform rapier;
    [SerializeField] private float speed;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction moveRapierAction;
    [SerializeField] private InputAction strikeAction;

    private RapierPosition rapierPosition = RapierPosition.MIDDLE;
    
    private readonly NetworkVariable<int> moveRapierInput = new ();
    private readonly NetworkVariable<float> moveInput = new ();
    
    public override void OnNetworkSpawn()
    {
        // if (!IsOwner)
        // {
        //     enabled = false;
        // }
    }

    private void Start()
    {
        if (IsLocalPlayer)
        {
            moveAction.Enable();
            moveRapierAction.Enable();
            moveRapierAction.performed += OnMoveRapier;
            strikeAction.Enable();
        }
        
        if (IsServer)
        {
            transform.position -= new Vector3(0, 1);
        }
    }

    private void OnMoveRapier(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            var moveRapierValue = context.ReadValue<float>();
            if (moveRapierValue > 0)
            {
                MoveRapierRPC(1);
            }
            else if (moveRapierValue < 0)
            {
                MoveRapierRPC(-1);
            }
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (moveInput.Value > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveInput.Value < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            rapierPosition += moveRapierInput.Value;
            rapierPosition = (RapierPosition)Mathf.Clamp((int)rapierPosition, (int)RapierPosition.BOTTOM , (int)RapierPosition.TOP);
            rapier.position = transform.position + new Vector3(0.7f, (int)rapierPosition * 0.5f);
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

    [Rpc(SendTo.Server)]
    private void MoveRapierRPC(int data)
    {
        moveRapierInput.Value = data;
    }
}
