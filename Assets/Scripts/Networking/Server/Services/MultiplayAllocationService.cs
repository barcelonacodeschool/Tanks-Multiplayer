using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class MultiplayAllocationService : IDisposable
{
    // Reference to the Multiplay service
    private IMultiplayService multiplayService;

    // Callbacks for handling server events
    private MultiplayEventCallbacks serverCallbacks;

    // Handler for server queries
    private IServerQueryHandler serverCheckManager;

    // Event subscription manager for server events
    private IServerEvents serverEvents;

    // Token source for canceling server checks
    private CancellationTokenSource serverCheckCancel;

    // ID of the current allocation
    string allocationId;

    // Constructor to initialize the allocation service
    public MultiplayAllocationService()
    {
        try
        {
            // Instantiate the Multiplay service and cancellation token
            multiplayService = MultiplayService.Instance;
            serverCheckCancel = new CancellationTokenSource();
        }
        catch (Exception ex)
        {
            // Log any errors during initialization
            Debug.LogWarning($"Error creating Multiplay allocation service.\n{ex}");
        }
    }

    // Method to subscribe to matchmaker allocation and wait for an allocation
    public async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        // Return null if the Multiplay service is not initialized
        if (multiplayService == null) { return null; }

        allocationId = null;

        // Set up server event callbacks
        serverCallbacks = new MultiplayEventCallbacks();
        serverCallbacks.Allocate += OnMultiplayAllocation;

        // Subscribe to server events
        serverEvents = await multiplayService.SubscribeToServerEventsAsync(serverCallbacks);

        // Wait for the allocation ID and payload
        string allocationID = await AwaitAllocationID();
        MatchmakingResults matchmakingPayload = await GetMatchmakerAllocationPayloadAsync();

        return matchmakingPayload;
    }

    // Method to wait for the allocation ID from the server configuration
    private async Task<string> AwaitAllocationID()
    {
        ServerConfig config = multiplayService.ServerConfig;
        Debug.Log($"Awaiting Allocation. Server Config is:\n" +
            $"-ServerID: {config.ServerId}\n" +
            $"-AllocationID: {config.AllocationId}\n" +
            $"-Port: {config.Port}\n" +
            $"-QPort: {config.QueryPort}\n" +
            $"-logs: {config.ServerLogDirectory}");

        // Loop until the allocation ID is received
        while (string.IsNullOrEmpty(allocationId))
        {
            string configID = config.AllocationId;

            // Check if the config has an allocation ID and set it if the local ID is still null
            if (!string.IsNullOrEmpty(configID) && string.IsNullOrEmpty(allocationId))
            {
                Debug.Log($"Config had AllocationID: {configID}");
                allocationId = configID;
            }

            // Wait for a short delay before checking again
            await Task.Delay(100);
        }

        return allocationId;
    }

    // Method to get the matchmaking payload allocation
    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        // Get the allocation payload as MatchmakingResults
        MatchmakingResults payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();

        // Serialize the payload to JSON for logging
        string modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
        Debug.Log(nameof(GetMatchmakerAllocationPayloadAsync) + ":" + Environment.NewLine + modelAsJson);

        return payloadAllocation;
    }

    // Callback method for handling allocation events
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");

        // Set the allocation ID if it's not null or empty
        if (string.IsNullOrEmpty(allocation.AllocationId)) { return; }

        allocationId = allocation.AllocationId;
    }

    // Method to begin server checks
    public async Task BeginServerCheck()
    {
        if (multiplayService == null) { return; }

        // Start the server query handler
        serverCheckManager = await multiplayService.StartServerQueryHandlerAsync((ushort)20, "ServerName", "", "0", "");

        // Start the server check loop
        ServerCheckLoop(serverCheckCancel.Token);
    }

    // Method to set the server name
    public void SetServerName(string name)
    {
        serverCheckManager.ServerName = name;
    }

    // Method to set the build ID
    public void SetBuildID(string id)
    {
        serverCheckManager.BuildId = id;
    }

    // Method to set the maximum number of players
    public void SetMaxPlayers(ushort players)
    {
        serverCheckManager.MaxPlayers = players;
    }

    // Method to increment the current player count
    public void AddPlayer()
    {
        serverCheckManager.CurrentPlayers++;
    }

    // Method to decrement the current player count
    public void RemovePlayer()
    {
        serverCheckManager.CurrentPlayers--;
    }

    // Method to set the current map
    public void SetMap(string newMap)
    {
        serverCheckManager.Map = newMap;
    }

    // Method to set the game mode
    public void SetMode(string mode)
    {
        serverCheckManager.GameType = mode;
    }

    // Loop to continuously check the server status
    private async void ServerCheckLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Update the server check status
            serverCheckManager.UpdateServerCheck();
            await Task.Delay(100);
        }
    }

    // Callback method for handling deallocation events
    private void OnMultiplayDeAllocation(MultiplayDeallocation deallocation)
    {
        Debug.Log(
                $"Multiplay Deallocated : ID: {deallocation.AllocationId}\nEvent: {deallocation.EventId}\nServer{deallocation.ServerId}");
    }

    // Callback method for handling error events
    private void OnMultiplayError(MultiplayError error)
    {
        Debug.Log($"MultiplayError : {error.Reason}\n{error.Detail}");
    }

    // Method to dispose of resources and unsubscribe from events
    public void Dispose()
    {
        // Unsubscribe from event callbacks
        if (serverCallbacks != null)
        {
            serverCallbacks.Allocate -= OnMultiplayAllocation;
            serverCallbacks.Deallocate -= OnMultiplayDeAllocation;
            serverCallbacks.Error -= OnMultiplayError;
        }

        // Cancel the server check if needed
        if (serverCheckCancel != null)
        {
            serverCheckCancel.Cancel();
        }

        // Unsubscribe from server events
        serverEvents?.UnsubscribeAsync();
    }
}
