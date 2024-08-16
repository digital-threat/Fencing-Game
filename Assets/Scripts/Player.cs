using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private List<GameObject> emojiPrefabs;
    [SerializeField] private float emojiDuration;
    [SerializeField] private float emojiCooldown;
    [SerializeField] private float speed;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction turnAction;
    [SerializeField] private InputAction emojiAction1;
    [SerializeField] private InputAction emojiAction2;
    [SerializeField] private InputAction emojiAction3;
    
    private NetworkTransform networkTransform;

    private bool canSpawnEmoji = true;
    

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        if (IsLocalPlayer)
        {
            moveAction.Enable();
            turnAction.Enable();
            
            emojiAction1.Enable();
            emojiAction2.Enable();
            emojiAction3.Enable();
            
            emojiAction1.performed += OnEmojiAction1;
            emojiAction2.performed += OnEmojiAction2;
            emojiAction3.performed += OnEmojiAction3;
        }
    }

    private void OnEmojiAction1(InputAction.CallbackContext context)
    {
        SpawnEmojiRPC(0, OwnerClientId);
    }
    
    private void OnEmojiAction2(InputAction.CallbackContext context)
    {
        SpawnEmojiRPC(1, OwnerClientId);
    }
    
    private void OnEmojiAction3(InputAction.CallbackContext context)
    {
        SpawnEmojiRPC(2, OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void SpawnEmojiRPC(int prefabIndex, ulong clientId)
    {
        if (canSpawnEmoji)
        {
            var emoji = Instantiate(emojiPrefabs[prefabIndex]);
            var noEmoji = emoji.GetComponent<NetworkObject>();
            noEmoji.SpawnWithOwnership(clientId);
            noEmoji.TrySetParent(GetComponent<NetworkObject>(), false);
            StartCoroutine(EmojiCoroutine(noEmoji));
        }
    }

    private IEnumerator EmojiCoroutine(NetworkObject emoji)
    {
        canSpawnEmoji = false;
        yield return new WaitForSeconds(emojiDuration);
        
        emoji.Despawn();
        Destroy(emoji.gameObject);
        
        yield return new WaitForSeconds(emojiCooldown);
        canSpawnEmoji = true;
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            var moveValue = moveAction.ReadValue<float>();
            var holdDirection = turnAction.IsPressed();
            if (moveValue != 0)
            {
                MoveRPC(moveValue, holdDirection);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void MoveRPC(float moveData, bool holdDirection)
    {
        var newPosition = transform.position + new Vector3(speed * Time.deltaTime * moveData, 0);
        newPosition.x = Mathf.Clamp(newPosition.x, -9, 9);

        transform.position = newPosition;
        
        if (!holdDirection && moveData > 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 180))
        {
            var rotation = Quaternion.Euler(0, 0, 0);
            networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
        }
        else if (!holdDirection && moveData < 0 && Mathf.Approximately(transform.rotation.eulerAngles.y, 0))
        {
            var rotation = Quaternion.Euler(0, 180, 0);
            networkTransform.SetState(rotIn: rotation, teleportDisabled: false);
        }
    }
}
