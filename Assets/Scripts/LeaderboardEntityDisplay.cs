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

    // Private field to store the player's display name
    private FixedString32Bytes displayName;

    // Public properties to get team index, client ID, and coins
    public int TeamIndex { get; private set; }
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    // Method to initialize the leaderboard entity with client ID, display name, and coins
    public void Initialise(ulong clientId, FixedString32Bytes displayName, int coins)
    {
        ClientId = clientId; // Set the client ID
        this.displayName = displayName; // Set the display name

        UpdateCoins(coins); // Update the coin count
    }

    // Method to initialize the leaderboard entity with team index, display name, and coins
    public void Initialise(int teamIndex, FixedString32Bytes displayName, int coins)
    {
        TeamIndex = teamIndex; // Set the team index
        this.displayName = displayName; // Set the display name

        UpdateCoins(coins); // Update the coin count
    }

    // Method to set the display color
    public void SetColor(Color color)
    {
        displayText.color = color; // Set the text color
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
        displayText.text = $"[{transform.GetSiblingIndex() + 1}] {displayName} - {Coins}";
    }
}
