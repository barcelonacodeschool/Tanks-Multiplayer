using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject for looking up team colors
[CreateAssetMenu(fileName = "NewTeamColorLookup", menuName = "Team Color Lookup")]
public class TeamColorLookup : ScriptableObject
{
    // Array of colors for each team
    [SerializeField] private Color[] teamColors;

    // Method to get the color for a specific team index
    public Color GetTeamColor(int teamIndex)
    {
        // Check if the team index is within the valid range
        if (teamIndex < 0 || teamIndex >= teamColors.Length)
        {
            // Return a random color if the index is out of range
            return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        else
        {
            // Return the color for the specified team index
            return teamColors[teamIndex];
        }
    }
}
