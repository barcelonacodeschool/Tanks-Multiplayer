using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    // Variables to store server IP, port, and query port
    private string serverIP;
    private int serverPort;
    private int queryPort;

    // Instance to handle backfilling
    private MatchplayBackfiller backfiller;

    // Network server instance
    private NetworkServer networkServer;

    // Service for managing multiplay allocation
    private MultiplayAllocationService multiplayAllocationService;

    // Constant for the game scene name
    private const string GameSceneName = "Game";

    // Constructor to initialize the server game manager with IP, port, query port, and network manager
    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;

        // Initialize the network server with the provided network manager
        networkServer = new NetworkServer(manager);

        // Initialize the multiplay allocation service
        multiplayAllocationService = new MultiplayAllocationService();
    }

    // Asynchronous method to start the game server
    public async Task StartGameServerAsync()
    {
        // Begin server checks
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            // Get matchmaker payload
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                // Start backfilling if payload is received
                await StartBackfill(matchmakerPayload);

                // Subscribe to user join and leave events
                networkServer.OnUserJoined += UserJoined;
                networkServer.OnUserLeft += UserLeft;
            }
            else
            {
                // Log a warning if payload times out
                Debug.LogWarning("Matchmaker payload timed out");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e); // Log any exceptions that occur
        }

        // Open network server connection and check if it started successfully
        if (!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("NetworkServer did not start as expected.");
            return;
        }

        // Load the game scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    // Method to get the matchmaker payload asynchronously
    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        // Start the task to get the matchmaker payload
        Task<MatchmakingResults> matchmakerPayloadTask =
            multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        // Wait for either the task to complete or a timeout
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            // Return the result if the task completed
            return matchmakerPayloadTask.Result;
        }

        // Return null if the task timed out
        return null;
    }

    // Method to start backfilling with the provided payload
    private async Task StartBackfill(MatchmakingResults payload)
    {
        // Initialize the backfiller with server details and payload
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}",
            payload.QueueName,
            payload.MatchProperties,
            20);

        // Begin backfilling if more players are needed
        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }

    // Method to handle user joining
    private void UserJoined(UserData user)
    {
        // Add the player to the backfill match
        backfiller.AddPlayerToMatch(user);
        // Increment the player count in the multiplay allocation service
        multiplayAllocationService.AddPlayer();

        // Stop backfilling if no more players are needed
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }

    // Method to handle user leaving
    private void UserLeft(UserData user)
    {
        // Remove the player from the backfill match
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        // Decrement the player count in the multiplay allocation service
        multiplayAllocationService.RemovePlayer();

        // Close the server if there are no more players
        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }

        // Resume backfilling if more players are needed
        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
    }

    // Method to close the server asynchronously
    private async void CloseServer()
    {
        // Stop backfilling
        await backfiller.StopBackfill();
        // Dispose of resources
        Dispose();
        // Quit the application
        Application.Quit();
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        // Unsubscribe from user join and leave events
        networkServer.OnUserJoined -= UserJoined;
        networkServer.OnUserLeft -= UserLeft;

        // Dispose of the backfiller if it exists
        backfiller?.Dispose();

        // Dispose of the multiplay allocation service and network server
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
