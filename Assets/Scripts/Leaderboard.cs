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
}
