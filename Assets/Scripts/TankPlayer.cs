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

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15; // Priority setting for the owner's camera

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(); // Network variable to store the player's name

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
        }

        // Check if this player is the owner
        if (IsOwner)
        {
            // Set the camera priority for the owner
            virtualCamera.Priority = ownerPriority;
        }
    }
}
