using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    // Reference to the Health component
    [SerializeField] private Health health;
    // Reference to the BountyCoin prefab
    [SerializeField] private BountyCoin coinPrefab;

    [Header("Settings")]
    // Distance over which coins are spread when spawning
    [SerializeField] private float coinSpread = 3f;
    // Percentage of total coins converted to bounty on death
    [SerializeField] private float bountyPercentage = 50f;
    // Number of bounty coins spawned on death
    [SerializeField] private int bountyCoinCount = 10;
    // Minimum value of each bounty coin
    [SerializeField] private int minBountyCoinValue = 5;
    // Layer mask for collision checks
    [SerializeField] private LayerMask layerMask;

    // Buffer to store coin colliders detected during overlap checks
    private Collider2D[] coinBuffer = new Collider2D[1];
    // Radius of the coin's collider
    private float coinRadius;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>(); // Network variable to track the total number of coins

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Get the radius of the coin's collider
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        // Subscribe to the OnDie event of the health component
        health.OnDie += HandleDie;
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Unsubscribe from the OnDie event of the health component
        health.OnDie -= HandleDie;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent<Coin>(out Coin coin)) { return; } // Check if the collider has a Coin component
        int coinValue = coin.Collect(); // Collect the coin and get its value

        if (!IsServer) { return; } // Ensure this logic runs only on the server

        TotalCoins.Value += coinValue; // Add the coin value to the total coins
    }

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire; // Deduct the cost from the total coins
    }

    // Method to handle player death
    private void HandleDie(Health health)
    {
        // Calculate the total bounty value as a percentage of total coins
        int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        // Calculate the value of each bounty coin
        int bountyCoinValue = bountyValue / bountyCoinCount;

        // If the bounty coin value is less than the minimum, do not spawn coins
        if (bountyCoinValue < minBountyCoinValue) { return; }

        // Spawn the specified number of bounty coins
        for (int i = 0; i < bountyCoinCount; i++)
        {
            // Instantiate a bounty coin at a random spawn point
            BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            // Set the value of the bounty coin
            coinInstance.SetValue(bountyCoinValue);
            // Spawn the coin as a networked object
            coinInstance.NetworkObject.Spawn();
        }
    }

    // Method to get a valid spawn point for coins
    private Vector2 GetSpawnPoint()
    {
        // Loop until a valid spawn point is found
        while (true)
        {
            // Calculate a random spawn point within the coin spread radius
            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            // Check if the spawn point is free of other colliders
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            // If no colliders are found at the spawn point, return it
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
