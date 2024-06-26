using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

// Class representing a tank player in a networked game
public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15; // Priority setting for the owner's camera

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Check if this player is the owner
        if (IsOwner)
        {
            // Set the camera priority for the owner
            virtualCamera.Priority = ownerPriority;
        }
    }
}
