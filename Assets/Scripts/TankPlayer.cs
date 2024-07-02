using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;

// Class representing a tank player in a networked game
public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera
    [SerializeField] private SpriteRenderer minimapIconRenderer; // Reference to the Sprite Renderer
    [SerializeField] private Texture2D crosshair; // Reference to the crosshair texture
    [field: SerializeField] public Health Health { get; private set; } // Reference to the Health component
    [field: SerializeField] public CoinWallet Wallet { get; private set; } // Reference to the Wallet component

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15; // Priority setting for the owner's camera
    [SerializeField] private Color ownerColor; // Color for the owner's minimap icon

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(); // Network variable to store the player's name

    // Events to notify when a player is spawned or despawned
    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Check if this instance is running on the server
        if (IsServer)
        {
            // Get the user data for the owner client ID
            UserData userData =
                HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            // Set the player's name from the user data
            PlayerName.Value = userData.userName;

            // Invoke the OnPlayerSpawned event
            OnPlayerSpawned?.Invoke(this);
        }

        // Check if this player is the owner
        if (IsOwner)
        {
            // Set the camera priority for the owner
            virtualCamera.Priority = ownerPriority;

            // Set the minimap icon color for the owner
            minimapIconRenderer.color = ownerColor;

            // Set the cursor to the crosshair texture
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
        }
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        // Check if this instance is running on the server
        if (IsServer)
        {
            // Invoke the OnPlayerDespawned event
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
