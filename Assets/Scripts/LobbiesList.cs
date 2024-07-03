using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

// LobbiesList manages the list of lobbies and handles joining and refreshing lobbies
public class LobbiesList : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu; // Reference to the main menu script
    [SerializeField] private Transform lobbyItemParent; // Parent transform to hold lobby item UI elements
    [SerializeField] private LobbyItem lobbyItemPrefab; // Prefab for the lobby item UI elements

    private bool isRefreshing; // Flag to track if a refresh operation is in progress

    // This method is called when the script instance is enabled
    private void OnEnable()
    {
        RefreshList(); // Refresh the lobby list when the script is enabled
    }

    // Method to refresh the list of lobbies asynchronously
    public async void RefreshList()
    {
        if (isRefreshing) { return; } // Return if a refresh operation is already in progress

        isRefreshing = true; // Set the refresh flag

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions(); // Create options for querying lobbies
            options.Count = 25; // Set the number of lobbies to retrieve

            options.Filters = new List<QueryFilter>() // Set the filters for querying lobbies
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"), // Filter for lobbies with available slots
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0") // Filter for unlocked lobbies
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options); // Query the lobbies

            foreach (Transform child in lobbyItemParent) // Destroy existing lobby item UI elements
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results) // Create new lobby item UI elements for each retrieved lobby
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent); // Instantiate the lobby item prefab
                lobbyItem.Initialise(this, lobby); // Initialize the lobby item with lobby details
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e); // Log any exceptions
        }

        isRefreshing = false; // Reset the refresh flag
    }

    // Method to join a lobby asynchronously
    public void JoinAsync(Lobby lobby)
    {
        // Call the main menu's JoinAsync method to join the lobby
        mainMenu.JoinAsync(lobby);
    }
}
