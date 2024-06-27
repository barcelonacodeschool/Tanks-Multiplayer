using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// Class to manage the leaderboard in a networked game
public class Leaderboard : NetworkBehaviour
{
    // Reference to the parent transform that will hold leaderboard entities
    [SerializeField] private Transform leaderboardEntityHolder;
    // Reference to the prefab for displaying leaderboard entities
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
    // Number of entities to display on the leaderboard
    [SerializeField] private int entitiesToDisplay = 8;

    // Network list to store the state of leaderboard entities
    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    // List to store the instantiated leaderboard entity displays
    private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

    // Update is called once per frame
    private void Awake()
    {
        // Initialize the network list for leaderboard entities
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // Subscribe to changes in the leaderboard entities list
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            // Handle existing leaderboard entities
            foreach (LeaderboardEntityState entity in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            // Find and handle all existing players on the server
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }

            // Subscribe to player spawned and despawned events
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            // Unsubscribe from changes in the leaderboard entities list
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        if (IsServer)
        {
            // Unsubscribe from player spawned and despawned events
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    // Method to handle changes in the leaderboard entities list
    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                // Check if the entity is already displayed
                if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    // Instantiate a new leaderboard entity display when an entity is added
                    LeaderboardEntityDisplay leaderboardEntity =
                        Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    // Initialize the display with the entity data
                    leaderboardEntity.Initialise(
                        changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    // Add the display to the list of displays
                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                // Find the display to remove
                LeaderboardEntityDisplay displayToRemove =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    // Remove the display from the parent and destroy it
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    // Remove the display from the list
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                // Find the display to update
                LeaderboardEntityDisplay displayToUpdate =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null)
                {
                    // Update the coins in the display
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }
        // Sort the displays by the coin count in descending order
        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

        // Update the display order and visibility based on the sorted list
        for (int i = 0; i < entityDisplays.Count; i++)
        {
            // Set the sibling index to maintain the visual order
            entityDisplays[i].transform.SetSiblingIndex(i);
            // Update the text to reflect the new order
            entityDisplays[i].UpdateText();
            // Activate or deactivate the display based on the index
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }

        // Find the local player's display
        LeaderboardEntityDisplay myDisplay =
            entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        // Ensure the local player's display is visible if it is outside the top entities
        if (myDisplay != null)
        {
            if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                // Hide the last display in the top list and show the local player's display
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    // Method to handle player spawn events
    private void HandlePlayerSpawned(TankPlayer player)
    {
        // Add a new entry to the leaderboard for the spawned player
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        // Subscribe to changes in the player's coin count
        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    // Method to handle player despawn events
    private void HandlePlayerDespawned(TankPlayer player)
    {
        // Remove the player's entry from the leaderboard
        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if (entity.ClientId != player.OwnerClientId) { continue; }

            leaderboardEntities.Remove(entity);
            break;
        }

        // Unsubscribe from changes in the player's coin count
        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    // Method to handle changes in the player's coin count
    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        // Update the coin count in the leaderboard entity
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId != clientId) { continue; }

            // Create a new leaderboard entity state with updated coins
            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newCoins
            };

            return;
        }
    }
}
