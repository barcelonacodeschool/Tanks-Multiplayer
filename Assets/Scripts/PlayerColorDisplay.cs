using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorDisplay : MonoBehaviour
{
    [SerializeField] private TeamColorLookup teamColorLookup; // Reference to the team colour lookup scriptable object
    [SerializeField] private TankPlayer player; // Reference to the TankPlayer script
    [SerializeField] private SpriteRenderer[] playerSprites; // Array of sprite renderers for the player

    private void Start()
    {
        // Handle the initial team colour change when the script starts
        HandleTeamChanged(-1, player.TeamIndex.Value);

        // Subscribe to the TeamIndex value changed event
        player.TeamIndex.OnValueChanged += HandleTeamChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the TeamIndex value changed event to prevent memory leaks
        player.TeamIndex.OnValueChanged -= HandleTeamChanged;
    }

    // Method to handle team changes
    private void HandleTeamChanged(int oldTeamIndex, int newTeamIndex)
    {
        // Get the new team colour from the team colour lookup
        Color teamColor = teamColorLookup.GetTeamColor(newTeamIndex);

        // Update the colour of all player sprites
        foreach (SpriteRenderer sprite in playerSprites)
        {
            sprite.color = teamColor;
        }
    }
}
