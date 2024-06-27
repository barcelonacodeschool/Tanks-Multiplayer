using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

// Class to display the player's name in the UI
public class PlayerNameDisplay : MonoBehaviour
{
    // Reference to the TankPlayer component
    [SerializeField] private TankPlayer player;
    // Reference to the TMP_Text component to display the player's name
    [SerializeField] private TMP_Text playerNameText;

    // Method called at the start of the game
    private void Start()
    {
        // Update the player name text initially
        HandlePlayerNameChanged(string.Empty, player.PlayerName.Value);

        // Subscribe to the PlayerName OnValueChanged event to handle name changes
        player.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    // Method to handle player name changes
    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        // Update the player name text with the new name
        playerNameText.text = newName.ToString();
    }

    // Method called when the object is destroyed
    private void OnDestroy()
    {
        // Unsubscribe from the PlayerName OnValueChanged event to prevent memory leaks
        player.PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }
}
