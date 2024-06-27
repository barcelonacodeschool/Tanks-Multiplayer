using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Class to handle player respawning in a networked game
public class RespawnHandler : NetworkBehaviour
{
    // Reference to the player prefab for respawning
    [SerializeField] private TankPlayer playerPrefab;
    // Percentage of coins the player keeps on respawn
    [SerializeField] private float keptCoinPercentage;

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
        // Calculate the amount of coins the player keeps on respawn
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        // Destroy the player's game object
        Destroy(player.gameObject);

        // Start the coroutine to respawn the player
        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    // Coroutine to respawn the player
    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        // Wait for the next frame
        yield return null;

        // Instantiate a new player instance at a random spawn position
        TankPlayer playerInstance = Instantiate(
            playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        // Spawn the new player instance as a networked player object
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        // Add the kept coins to the new player instance's wallet
        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }
}
