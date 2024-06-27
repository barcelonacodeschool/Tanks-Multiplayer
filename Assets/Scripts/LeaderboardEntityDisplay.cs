using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Class to display leaderboard entity information
public class LeaderboardEntityDisplay : MonoBehaviour
{
    // Reference to the TMP_Text component to display the information
    [SerializeField] private TMP_Text displayText;
    // Color for the player's display text if it's the local player
    [SerializeField] private Color myColour;

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

        // Check if the client ID is the same as the local client ID
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Change the text color to the specified color for the local player
            displayText.color = myColour;
        }

        UpdateCoins(coins); // Update the coin display
    }

    // Method to update the coin count
    public void UpdateCoins(int coins)
    {
        Coins = coins; // Set the coin count

        UpdateText(); // Update the displayed text
    }

    // Method to update the displayed text
    public void UpdateText()
    {
        // Set the display text to show the player's ranking, name, and coin count
        displayText.text = $"[{transform.GetSiblingIndex() + 1}] {playerName} - {Coins}";
    }
}
