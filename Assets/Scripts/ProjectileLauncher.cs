using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ProjectileLauncher handles the spawning and launching of projectiles
public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader; // Reference to the InputReader scriptable object
    [SerializeField] private Transform projectileSpawnPoint; // Transform where projectiles will be spawned
    [SerializeField] private GameObject serverProjectilePrefab; // Prefab used for projectiles on the server
    [SerializeField] private GameObject clientProjectilePrefab; // Prefab used for projectiles on the client

    [Header("Settings")]
    [SerializeField] private float projectileSpeed; // Speed of the projectiles

    private bool shouldFire; // Flag to track if the primary fire action should be performed

    // This method is called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; } // Check if the local player owns this object

        // Subscribe to the PrimaryFireEvent from the inputReader
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    // This method is called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; } // Check if the local player owns this object

        // Unsubscribe from the PrimaryFireEvent to prevent memory leaks
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; } // Check if the local player owns this object

        if (!shouldFire) { return; } // Check if the primary fire action should be performed

        // Call the server RPC to handle projectile firing on the server
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        // Spawn a dummy projectile on the client for visual feedback
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
    }

    // Method to handle the primary fire input action
    void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire; // Set the shouldFire flag based on input
    }

    // Server RPC to handle projectile firing on the server
    [ServerRpc]
    void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        // Instantiate the projectile on the server
        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        // Set the projectile's direction
        projectileInstance.transform.up = direction;

        // Call the client RPC to spawn dummy projectiles on all clients
        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    // Client RPC to spawn dummy projectiles on all clients
    [ClientRpc]
    void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) { return; } // Skip spawning if this is the owner client

        // Spawn a dummy projectile on the client
        SpawnDummyProjectile(spawnPos, direction);
    }

    // Method to spawn a dummy projectile
    void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        // Instantiate the dummy projectile on the client
        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        // Set the projectile's direction
        projectileInstance.transform.up = direction;
    }
}
