using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rapier : NetworkBehaviour
{
    public enum Stance
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
        transform.localPosition += new Vector3(strikeReach, 0);


        // No hit if rapier is already inside the other player.
        if (!OverlapsPlayer())
        {
            int hits = collider.Cast(transform.right, hitBuffer, strikeReach);
            for (int i = 0; i < hits; i++)
            {
                if (hitBuffer[i].transform.CompareTag("Player"))
                { 
                    var otherClientId = hitBuffer[i].transform.GetComponent<NetworkObject>().OwnerClientId;

                    // No hit if the other player is in the same stance
                    if (Game.Instance.GetStance(otherClientId) != stance)
                    {
                        Game.Instance.RespawnPlayers();
                        Game.Instance.UpdateScore(OwnerClientId, otherClientId);
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(strikeDuration);
        
        transform.localPosition -= new Vector3(strikeReach, 0);
        state = State.GUARD;
    }
    
    private bool OverlapsPlayer()
    {
        int hits = collider.Cast(transform.right, hitBuffer, 0);
        for (int i = 0; i < hits; i++)
        {
            if (hitBuffer[i].transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    public Stance GetStance()
    {
        return stance;
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
