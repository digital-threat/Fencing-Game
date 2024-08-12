using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rapier : NetworkBehaviour
{
    private enum Stance
    {
        BOTTOM,
        MIDDLE,
        TOP
    }

    private enum State
    {
        GUARD,
        STRIKE,
    }
    
    [SerializeField] private Vector3 offset;
    [SerializeField] private float stanceHeightDiff;
    [SerializeField] private float strikeReach;
    [SerializeField] private float strikeDuration;
    
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction strikeAction;

    private new BoxCollider2D collider;
    
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[2];
    private readonly Dictionary<Stance, float> stances = new ();

    private State state = State.GUARD;
    private Stance stance = Stance.MIDDLE;

    private void Awake()
    {
        stances.Add(Stance.BOTTOM, -stanceHeightDiff);
        stances.Add(Stance.MIDDLE, 0);
        stances.Add(Stance.TOP, stanceHeightDiff);

        collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            moveAction.Enable();
            strikeAction.Enable();
            
            moveAction.performed += OnMoveRapier;
            strikeAction.performed += OnStrike;
        }

        if (IsServer)
        {
            transform.localPosition = offset;
        }
    }

    private void OnStrike(InputAction.CallbackContext context)
    {
        if (state == State.GUARD)
        {
            StrikeRPC();
        }
    }

    private void OnMoveRapier(InputAction.CallbackContext context)
    {
        if (state == State.GUARD)
        {
            var moveRapierValue = context.ReadValue<float>();
            switch (moveRapierValue)
            {
                case > 0:
                    MoveRapierRPC(1);
                    break;
                case < 0:
                    MoveRapierRPC(-1);
                    break;
            }
        }
    }

    private IEnumerator StrikeCoroutine()
    {
        state = State.STRIKE;
        
        int hits = collider.Cast(transform.right, hitBuffer, 0);
        for (int i = 0; i < hits; i++)
        {
            if (hitBuffer[i].transform.CompareTag("Player"))
            {
                Debug.Log("Overlaps player");
            }
        }
        
        hits = collider.Cast(transform.right, hitBuffer, strikeReach);
        for (int i = 0; i < hits; i++)
        {
            if (hitBuffer[i].transform.CompareTag("Player"))
            {
                Game.Instance.RespawnPlayers();
            }
        }
        
        transform.localPosition += new Vector3(strikeReach, 0);
        yield return new WaitForSeconds(strikeDuration);
        transform.localPosition -= new Vector3(strikeReach, 0);
        
        state = State.GUARD;
    }
    
    [Rpc(SendTo.Server)]
    private void MoveRapierRPC(int data)
    {
        stance += data;
        stance = (Stance)Mathf.Clamp((int)stance, (int)Stance.BOTTOM , (int)Stance.TOP);
            
        transform.localPosition = offset + new Vector3(0, stances[stance]);
    }
    
    [Rpc(SendTo.Server)]
    private void StrikeRPC()
    {
        StartCoroutine(StrikeCoroutine());
    }
}
