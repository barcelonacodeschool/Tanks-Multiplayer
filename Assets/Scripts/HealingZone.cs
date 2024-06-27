using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// HealingZone class inherits from NetworkBehaviour for networked functionality
public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    // Reference to the UI Image component representing the heal power bar
    [SerializeField] private Image healPowerBar;

    [Header("Settings")]
    // Maximum healing power available
    [SerializeField] private int maxHealPower = 30;
    // Cooldown time before healing can start again
    [SerializeField] private float healCooldown = 60f;
    // Rate at which healing ticks occur
    [SerializeField] private float healTickRate = 1f;
    // Number of coins deducted per healing tick
    [SerializeField] private int coinsPerTick = 10;
    // Amount of health restored per healing tick
    [SerializeField] private int healthPerTick = 10;

    // Remaining time for cooldown
    private float remainingCooldown;
    // Timer to keep track of heal ticks
    private float tickTimer;

    // List to keep track of players currently in the healing zone
    private List<TankPlayer> playersInZone = new List<TankPlayer>();

    // Network variable to track the current heal power
    private NetworkVariable<int> HealPower = new NetworkVariable<int>();

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // If this is the client, subscribe to the HealPower value change event
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value); // Initialize the heal power bar
        }

        // If this is the server, set the initial heal power
        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        // If this is the client, unsubscribe from the HealPower value change event
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    // Method called when another collider enters the trigger collider
    private void OnTriggerEnter2D(Collider2D col)
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Check if the collider's attached Rigidbody has a TankPlayer component
        if (!col.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)) { return; }

        // Add the player to the list of players in the healing zone
        playersInZone.Add(player);
    }

    // Method called when another collider exits the trigger collider
    private void OnTriggerExit2D(Collider2D col)
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Check if the collider's attached Rigidbody has a TankPlayer component
        if (!col.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)) { return; }

        // Remove the player from the list of players in the healing zone
        playersInZone.Remove(player);
    }

    private void Update()
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Handle cooldown logic
        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;

            if (remainingCooldown <= 0f)
            {
                // Reset heal power after cooldown
                HealPower.Value = maxHealPower;
            }
            else
            {
                return; // Skip the rest of the update if still on cooldown
            }
        }

        // Handle healing ticks
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate)
        {
            foreach (TankPlayer player in playersInZone)
            {
                // Stop if out of heal power
                if (HealPower.Value == 0) { break; }

                // Skip if player is at max health
                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) { continue; }

                // Skip if player doesn't have enough coins
                if (player.Wallet.TotalCoins.Value < coinsPerTick) { continue; }

                // Deduct coins and restore health
                player.Wallet.SpendCoins(coinsPerTick);
                player.Health.RestoreHealth(healthPerTick);

                // Decrease heal power
                HealPower.Value -= 1;

                // Start cooldown if out of heal power
                if (HealPower.Value == 0)
                {
                    remainingCooldown = healCooldown;
                }
            }

            // Reset tick timer
            tickTimer = tickTimer % (1 / healTickRate);
        }
    }

    // Method to handle changes in heal power value
    private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
    {
        // Update the heal power bar fill amount
        healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
    }
}
