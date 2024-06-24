using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>(); // Network variable to track the total number of coins

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
}

