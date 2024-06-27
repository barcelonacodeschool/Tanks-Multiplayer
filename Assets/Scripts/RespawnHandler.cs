using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Class to handle player respawning in a networked game
public class RespawnHandler : NetworkBehaviour
{
    // Reference to the player prefab
    [SerializeField] private NetworkObject playerPrefab;

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Find all existing players and handle their spawn
        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (TankPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        // Subscribe to the player spawned and despawned events
        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Unsubscribe from the player spawned and despawned events
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    // Method to handle player spawn
    private void HandlePlayerSpawned(TankPlayer player)
    {
        // Subscribe to the player's OnDie event
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    // Method to handle player despawn
    private void HandlePlayerDespawned(TankPlayer player)
    {
        // Unsubscribe from the player's OnDie event
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    // Method to handle player death
    private void HandlePlayerDie(TankPlayer player)
    {
        // Destroy the player's game object
        Destroy(player.gameObject);

        // Start the respawn coroutine
        StartCoroutine(RespawnPlayer(player.OwnerClientId));
    }

    // Coroutine to respawn the player
    private IEnumerator RespawnPlayer(ulong ownerClientId)
    {
        yield return null; // Wait for the next frame

        // Instantiate the player prefab at a random spawn position
        NetworkObject playerInstance = Instantiate(
            playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        // Spawn the player object for the given client ID
        playerInstance.SpawnAsPlayerObject(ownerClientId);
    }
}
