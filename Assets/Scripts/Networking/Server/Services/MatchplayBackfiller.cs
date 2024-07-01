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
        this.maxPlayers = maxPlayers;

        // Create backfill properties from matchmaker payload properties
        BackfillTicketProperties backfillProperties = new BackfillTicketProperties(matchmakerPayloadProperties);

        // Initialize the local backfill ticket with the properties
        localBackfillTicket = new BackfillTicket
        {
            Id = matchmakerPayloadProperties.BackfillTicketId,
            Properties = backfillProperties
        };

        // Set up the options for creating a backfill ticket
        createBackfillOptions = new CreateBackfillTicketOptions
        {
            Connection = connection,
            QueueName = queueName,
            Properties = backfillProperties
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

    // Method to add a player to the match
    public void AddPlayerToMatch(UserData userData)
    {
        // Check if backfilling is in progress
        if (!IsBackfilling)
        {
            Debug.LogWarning("Can't add users to the backfill ticket before it's been created");
            return;
        }

        // Check if the player is already in the match
        if (GetPlayerById(userData.userAuthId) != null)
        {
            Debug.LogWarningFormat("User: {0} - {1} already in Match. Ignoring add.",
                userData.userName,
                userData.userAuthId);

            return;
        }

        // Create a new player from user data and add to the match properties
        Player matchmakerPlayer = new Player(userData.userAuthId, userData.userGamePreferences);
        MatchProperties.Players.Add(matchmakerPlayer);
        MatchProperties.Teams[0].PlayerIds.Add(matchmakerPlayer.Id);

        // Set the local data dirty flag to indicate changes
        localDataDirty = true;
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
        MatchProperties.Teams[0].PlayerIds.Remove(userId);
        localDataDirty = true;

        // Return the updated player count
        return MatchPlayerCount;
    }

    // Method to check if more players are needed
    public bool NeedsPlayers()
    {
        return MatchPlayerCount < maxPlayers;
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
        _ = StopBackfill();
    }
}
