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

    // List to keep track of players currently in the healing zone
    private List<TankPlayer> playersInZone = new List<TankPlayer>();

    // Method called when another collider enters the trigger collider
    private void OnTriggerEnter2D(Collider2D col)
    {
        // Only execute on the server
        if (!IsServer) { return; }

        // Check if the collider's attached Rigidbody has a TankPlayer component
        if (!col.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)) { return; }

        // Add the player to the list of players in the healing zone
        playersInZone.Add(player);

        // Log the player's entry to the console
        Debug.Log($"Entered: {player.PlayerName.Value}");
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

        // Log the player's exit to the console
        Debug.Log($"Left: {player.PlayerName.Value}");
    }
}
