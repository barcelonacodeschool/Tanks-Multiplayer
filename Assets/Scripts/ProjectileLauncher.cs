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
    [SerializeField] private GameObject muzzleFlash; // Reference to the muzzle flash GameObject
    [SerializeField] private Collider2D playerCollider; // Reference to the player's collider
    [SerializeField] private CoinWallet wallet; // Reference to the player's coin wallet

    [Header("Settings")]
    [SerializeField] private float projectileSpeed; // Speed of the projectiles
    [SerializeField] private float fireRate; // Rate of fire for the projectiles
    [SerializeField] private float muzzleFlashDuration; // Duration of the muzzle flash effect
    [SerializeField] private int costToFire; // Cost in coins to fire a projectile

    private bool shouldFire; // Flag to track if the primary fire action should be performed
    private float timer; // Timer to control the rate of fire
    private float muzzleFlashTimer; // Timer for the muzzle flash effect
    
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
        // Handle muzzle flash timer
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime; // Decrease the timer by the time elapsed since last frame

            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false); // Disable the muzzle flash when the timer runs out
            }
        }

        if (!IsOwner) { return; } // Check if the local player owns this object

        if (timer > 0)
        {
            timer -= Time.deltaTime; // Decrease the fire rate timer by the time elapsed since last frame
        }

        if (!shouldFire) { return; } // Check if the primary fire action should be performed

        if (timer > 0) { return; } // Ensure the fire rate interval has passed

        if (wallet.TotalCoins.Value < costToFire) { return; } // Check if the player has enough coins to fire

        // Call the server RPC to handle projectile firing on the server
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        // Spawn a dummy projectile on the client for visual feedback
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        timer = 1 / fireRate; // Reset the fire rate timer
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
        if (wallet.TotalCoins.Value < costToFire) { return; } // Check if the player has enough coins to fire

        wallet.SpendCoins(costToFire); // Deduct the cost to fire from the player's wallet

        // Instantiate the projectile on the server
        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        // Set the projectile's direction
        projectileInstance.transform.up = direction;

        // Ignore collision between the player and the projectile
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        // Set the projectile's velocity if it has a Rigidbody2D component
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }

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
        // Activate the muzzle flash effect
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration; // Reset the muzzle flash timer

        // Instantiate the dummy projectile on the client
        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        // Set the projectile's direction
        projectileInstance.transform.up = direction;

        // Ignore collision between the player and the projectile
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        // Set the projectile's velocity if it has a Rigidbody2D component
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}
