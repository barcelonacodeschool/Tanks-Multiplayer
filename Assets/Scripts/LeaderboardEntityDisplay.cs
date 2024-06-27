using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

// Class to display leaderboard entity information
public class LeaderboardEntityDisplay : MonoBehaviour
{
    // Reference to the TMP_Text component to display the information
    [SerializeField] private TMP_Text displayText;

    // Private field to store the player's name
    private FixedString32Bytes playerName;

    // Public properties to get client ID and coins
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    // Method to initialize the leaderboard entity display
    public void Initialise(ulong clientId, FixedString32Bytes playerName, int coins)
    {
        ClientId = clientId; // Set the client ID
        this.playerName = playerName; // Set the player's name

        UpdateCoins(coins); // Update the coin display
    }

    // Method to update the coin count
    public void UpdateCoins(int coins)
    {
        Coins = coins; // Set the coin count

        UpdateText(); // Update the displayed text
    }

    // Private method to update the displayed text
    private void UpdateText()
    {
        // Set the display text to show the player's name and coin count
        displayText.text = $"[1] {playerName} - {Coins}";
    }
}
