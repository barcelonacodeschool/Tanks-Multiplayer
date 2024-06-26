using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

// LobbyItem represents a UI element for a single lobby in the lobby list
public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText; // Text element to display the lobby name
    [SerializeField] private TMP_Text lobbyPlayersText; // Text element to display the number of players in the lobby

    private LobbiesList lobbiesList; // Reference to the lobbies list manager
    private Lobby lobby; // Reference to the lobby data

    // Method to initialize the lobby item with lobby details
    public void Initialise(LobbiesList lobbiesList, Lobby lobby)
    {
        this.lobbiesList = lobbiesList; // Set the reference to the lobbies list manager
        this.lobby = lobby; // Set the reference to the lobby data

        lobbyNameText.text = lobby.Name; // Set the lobby name text
        lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}"; // Set the lobby players text
    }

    // Method to join the selected lobby
    public void Join()
    {
        lobbiesList.JoinAsync(lobby); // Call the join method on the lobbies list manager
    }
}
