using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

// Enum to define the results of matchmaker polling
public enum MatchmakerPollingResult
{
    Success,
    TicketCreationError,
    TicketCancellationError,
    TicketRetrievalError,
    MatchAssignmentError
}

// Class to store the matchmaking result information
public class MatchmakingResult
{
    public string ip; // IP address of the match server
    public int port; // Port of the match server
    public MatchmakerPollingResult result; // Result type of the matchmaking process
    public string resultMessage; // Detailed result message
}

// MatchplayMatchmaker manages the matchmaking process
public class MatchplayMatchmaker : IDisposable
{
    private string lastUsedTicket; // Stores the last used matchmaking ticket
    private CancellationTokenSource cancelToken; // Token to manage cancellation of matchmaking

    private const int TicketCooldown = 1000; // Time delay between ticket polling in milliseconds

    public bool IsMatchmaking { get; private set; } // Property to indicate if matchmaking is in progress

    // Method to start the matchmaking process asynchronously
    public async Task<MatchmakingResult> Matchmake(UserData data)
    {
        cancelToken = new CancellationTokenSource(); // Initialize the cancellation token

        // Get the queue name from user game preferences
        string queueName = data.userGamePreferences.ToMultiplayQueue();
        CreateTicketOptions createTicketOptions = new CreateTicketOptions(queueName); // Create ticket options with the queue name
        Debug.Log(createTicketOptions.QueueName); // Log the queue name

        // Create a list of players for the matchmaking request
        List<Player> players = new List<Player>
        {
            new Player(data.userAuthId, data.userGamePreferences)
        };

        try
        {
            IsMatchmaking = true; // Set matchmaking flag to true

            // Create a matchmaking ticket
            CreateTicketResponse createResult = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);
            lastUsedTicket = createResult.Id; // Store the created ticket ID

            try
            {
                // Poll for the matchmaking result until cancelled
                while (!cancelToken.IsCancellationRequested)
                {
                    // Check the status of the ticket
                    TicketStatusResponse checkTicket = await MatchmakerService.Instance.GetTicketAsync(lastUsedTicket);

                    // If a match is found
                    if (checkTicket.Type == typeof(MultiplayAssignment))
                    {
                        MultiplayAssignment matchAssignment = (MultiplayAssignment)checkTicket.Value;

                        if (matchAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                        {
                            // Return success result with match assignment details
                            return ReturnMatchResult(MatchmakerPollingResult.Success, "", matchAssignment);
                        }
                        if (matchAssignment.Status == MultiplayAssignment.StatusOptions.Timeout ||
                            matchAssignment.Status == MultiplayAssignment.StatusOptions.Failed)
                        {
                            // Return error result if match assignment failed or timed out
                            return ReturnMatchResult(MatchmakerPollingResult.MatchAssignmentError,
                                $"Ticket: {lastUsedTicket} - {matchAssignment.Status} - {matchAssignment.Message}", null);
                        }
                        Debug.Log($"Polled Ticket: {lastUsedTicket} Status: {matchAssignment.Status} ");
                    }

                    await Task.Delay(TicketCooldown); // Wait before polling again
                }
            }
            catch (MatchmakerServiceException e)
            {
                // Return error result if ticket retrieval fails
                return ReturnMatchResult(MatchmakerPollingResult.TicketRetrievalError, e.ToString(), null);
            }
        }
        catch (MatchmakerServiceException e)
        {
            // Return error result if ticket creation fails
            return ReturnMatchResult(MatchmakerPollingResult.TicketCreationError, e.ToString(), null);
        }

        // Return error result if matchmaking is cancelled
        return ReturnMatchResult(MatchmakerPollingResult.TicketRetrievalError, "Cancelled Matchmaking", null);
    }

    // Method to cancel the matchmaking process
    public async Task CancelMatchmaking()
    {
        if (!IsMatchmaking) { return; } // Exit if not matchmaking

        IsMatchmaking = false; // Set matchmaking flag to false

        if (cancelToken.Token.CanBeCanceled)
        {
            cancelToken.Cancel(); // Cancel the cancellation token
        }

        if (string.IsNullOrEmpty(lastUsedTicket)) { return; } // Exit if no ticket to cancel

        Debug.Log($"Cancelling {lastUsedTicket}"); // Log the cancellation

        await MatchmakerService.Instance.DeleteTicketAsync(lastUsedTicket); // Delete the matchmaking ticket
    }

    // Method to create and return a matchmaking result
    private MatchmakingResult ReturnMatchResult(MatchmakerPollingResult resultErrorType, string message, MultiplayAssignment assignment)
    {
        IsMatchmaking = false; // Set matchmaking flag to false

        if (assignment != null)
        {
            string parsedIp = assignment.Ip; // Get the IP address from the assignment
            int? parsedPort = assignment.Port; // Get the port from the assignment

            if (parsedPort == null)
            {
                // Return error result if port is missing
                return new MatchmakingResult
                {
                    result = MatchmakerPollingResult.MatchAssignmentError,
                    resultMessage = $"Port missing? - {assignment.Port}\n-{assignment.Message}"
                };
            }

            // Return success result with IP and port
            return new MatchmakingResult
            {
                result = MatchmakerPollingResult.Success,
                ip = parsedIp,
                port = (int)parsedPort,
                resultMessage = assignment.Message
            };
        }

        // Return error result if no assignment
        return new MatchmakingResult
        {
            result = resultErrorType,
            resultMessage = message
        };
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        _ = CancelMatchmaking(); // Cancel matchmaking if active

        cancelToken?.Dispose(); // Dispose of the cancellation token
    }
}
