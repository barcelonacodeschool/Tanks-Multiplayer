using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Class to manage the leaderboard in a networked game
public class Leaderboard : NetworkBehaviour
{
    // Reference to the parent transform that will hold leaderboard entities
    [SerializeField] private Transform leaderboardEntityHolder;
    // Reference to the prefab for displaying leaderboard entities
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    // Network list to store the state of leaderboard entities
    private NetworkList<LeaderboardEntityState> leaderboardEntities;

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
                // Instantiate a new leaderboard entity display when an entity is added
                Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                // Handle the removal of leaderboard entities if needed
                break;
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
    }
}
