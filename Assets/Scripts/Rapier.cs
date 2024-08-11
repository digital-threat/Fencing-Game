using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Rapier : NetworkBehaviour
{
    private enum RapierPosition
    {
        BOTTOM,
        MIDDLE,
        TOP
    }
    
    [SerializeField] private Vector3 offset;
    [SerializeField] private float moveHeight;

    
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction strikeAction;

    private Dictionary<RapierPosition, float> positions = new ();
    
    private RapierPosition rapierPosition = RapierPosition.MIDDLE;

    private readonly NetworkVariable<int> moveRapierInput = new ();


    private void Awake()
    {
        positions.Add(RapierPosition.BOTTOM, -moveHeight);
        positions.Add(RapierPosition.MIDDLE, 0);
        positions.Add(RapierPosition.TOP, moveHeight);
    }

    private void Start()
    {
        if (IsOwner)
        {
            moveAction.Enable();
            strikeAction.Enable();
        }

        if (IsServer)
        {
            transform.localPosition = offset;
            moveRapierInput.OnValueChanged = OnMoveInput;
        }
    }

    private void OnMoveInput(int previousvalue, int newvalue)
    {
        rapierPosition += moveRapierInput.Value;
        rapierPosition = (RapierPosition)Mathf.Clamp((int)rapierPosition, (int)RapierPosition.BOTTOM , (int)RapierPosition.TOP);
            
        transform.localPosition = offset + new Vector3(0, positions[rapierPosition]);
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        var moveRapierValue = moveAction.ReadValue<float>();
        switch (moveRapierValue)
        {
            case > 0:
                MoveRapierRPC(1);
                break;
            case < 0:
                MoveRapierRPC(-1);
                break;
            default:
                MoveRapierRPC(0);
                break;
        }
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRapierRPC(int data)
    {
        moveRapierInput.Value = data;
    }
}
