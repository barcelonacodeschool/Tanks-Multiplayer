using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchplayBackfiller : IDisposable
{
    // Options for creating a backfill ticket
    private CreateBackfillTicketOptions createBackfillOptions;

    // Local reference to the backfill ticket
    private BackfillTicket localBackfillTicket;

    // Flag to indicate if local data has changed and needs updating
    private bool localDataDirty;

    // Maximum number of players allowed in the match
    private int maxPlayers;

    // Constant for the delay between ticket checks
    private const int TicketCheckMs = 1000;

    // Property to get the current number of players in the match
    private int MatchPlayerCount => localBackfillTicket?.Properties.MatchProperties.Players.Count ?? 0;

    // Property to get the match properties from the local backfill ticket
    private MatchProperties MatchProperties => localBackfillTicket.Properties.MatchProperties;

    // Property to indicate if backfilling is in progress
    public bool IsBackfilling { get; private set; }

    // Constructor to initialize the backfiller with connection details and match properties
    public MatchplayBackfiller(string connection, string queueName, MatchProperties matchmakerPayloadProperties, int maxPlayers)
    {
        this.maxPlayers = maxPlayers; // Set the maximum number of players

        // Create backfill properties from matchmaker payload properties
        BackfillTicketProperties backfillProperties = new BackfillTicketProperties(matchmakerPayloadProperties);

        // Initialize the local backfill ticket with the properties
        localBackfillTicket = new BackfillTicket
        {
            Id = matchmakerPayloadProperties.BackfillTicketId, // Set the backfill ticket ID
            Properties = backfillProperties // Set the backfill properties
        };

        // Set up the options for creating a backfill ticket
        createBackfillOptions = new CreateBackfillTicketOptions
        {
            Connection = connection, // Set the connection string
            QueueName = queueName, // Set the queue name
            Properties = backfillProperties // Set the backfill properties
        };
    }

    // Method to start the backfilling process
    public async Task BeginBackfilling()
    {
        // Check if already backfilling to prevent multiple starts
        if (IsBackfilling)
        {
            Debug.LogWarning("Already backfilling, no need to start another.");
            return;
        }

        Debug.Log($"Starting backfill Server: {MatchPlayerCount}/{maxPlayers}");

        // Create the backfill ticket if it doesn't exist
        if (string.IsNullOrEmpty(localBackfillTicket.Id))
        {
            localBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(createBackfillOptions);
        }

        // Set the backfilling flag to true
        IsBackfilling = true;

        // Start the backfill loop to monitor and update the ticket
        BackfillLoop();
    }

    // Method to remove a player from the match
    public int RemovePlayerFromMatch(string userId)
    {
        // Find the player by ID
        Player playerToRemove = GetPlayerById(userId);
        if (playerToRemove == null)
        {
            Debug.LogWarning($"No user by the ID: {userId} in local backfill Data.");
            return MatchPlayerCount;
        }

        // Remove the player from match properties and set the dirty flag
        MatchProperties.Players.Remove(playerToRemove);
        GetTeamByUserId(userId).PlayerIds.Remove(userId);
        localDataDirty = true;

        // Return the updated player count
        return MatchPlayerCount;
    }

    // Method to check if more players are needed
    public bool NeedsPlayers()
    {
        return MatchPlayerCount < maxPlayers;
    }

    // Method to get the team by user ID
    public Team GetTeamByUserId(string userId)
    {
        return MatchProperties.Teams.FirstOrDefault(t => t.PlayerIds.Contains(userId));
    }

    // Helper method to find a player by ID
    private Player GetPlayerById(string userId)
    {
        return MatchProperties.Players.FirstOrDefault(
            p => p.Id.Equals(userId));
    }

    // Method to stop the backfilling process
    public async Task StopBackfill()
    {
        // Check if backfilling is in progress
        if (!IsBackfilling)
        {
            Debug.LogError("Can't stop backfilling before we start.");
            return;
        }

        // Delete the backfill ticket and reset the flag and ID
        await MatchmakerService.Instance.DeleteBackfillTicketAsync(localBackfillTicket.Id);
        IsBackfilling = false;
        localBackfillTicket.Id = null;
    }

    // Loop to continuously monitor and update the backfill ticket
    private async void BackfillLoop()
    {
        while (IsBackfilling)
        {
            // Update the backfill ticket if there are changes
            if (localDataDirty)
            {
                await MatchmakerService.Instance.UpdateBackfillTicketAsync(localBackfillTicket.Id, localBackfillTicket);
                localDataDirty = false;
            }
            // Otherwise, approve the backfill ticket
            else
            {
                localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(localBackfillTicket.Id);
            }

            // Stop backfilling if no more players are needed
            if (!NeedsPlayers())
            {
                await StopBackfill();
                break;
            }

            // Wait for a specified delay before the next iteration
            await Task.Delay(TicketCheckMs);
        }
    }

    // Method to dispose of resources and stop backfilling
    public void Dispose()
    {
        _ = StopBackfill(); // Stop backfilling and dispose resources
    }
}
