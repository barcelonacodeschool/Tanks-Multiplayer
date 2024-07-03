using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text queueStatusText; // Text field to display queue status
    [SerializeField] private TMP_Text queueTimerText; // Text field to display queue timer
    [SerializeField] private TMP_Text findMatchButtonText; // Text field for the find match button
    [SerializeField] private TMP_InputField joinCodeField; // Input field for the join code
    [SerializeField] private Toggle teamToggle; // Toggle for team-based matchmaking
    [SerializeField] private Toggle privateToggle; // Toggle for private lobbies

    private bool isMatchmaking; // Flag to indicate if matchmaking is in progress
    private bool isCancelling; // Flag to indicate if matchmaking cancellation is in progress
    private bool isBusy; // Flag to indicate if an operation is in progress
    private float timeInQueue; // Timer for how long the user has been in the queue

    // Method called when the script instance is being loaded
    private void Start()
    {
        if (ClientSingleton.Instance == null) { return; } // Return if there is no ClientSingleton instance

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Set the cursor to the default

        queueStatusText.text = string.Empty; // Clear the queue status text
        queueTimerText.text = string.Empty; // Clear the queue timer text
    }

    // Method called every frame to update the queue timer
    private void Update()
    {
        if (isMatchmaking)
        {
            timeInQueue += Time.deltaTime; // Increment the time in queue
            TimeSpan ts = TimeSpan.FromSeconds(timeInQueue); // Convert time to TimeSpan
            queueTimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds); // Update the queue timer text
        }
    }

    // Method called when the find match button is pressed
    public async void FindMatchPressed()
    {
        if (isCancelling) { return; } // Return if cancellation is in progress

        if (isMatchmaking)
        {
            queueStatusText.text = "Cancelling..."; // Update the queue status text
            isCancelling = true; // Set cancelling flag
            await ClientSingleton.Instance.GameManager.CancelMatchmaking(); // Await the cancellation of matchmaking
            isCancelling = false; // Reset cancelling flag
            isMatchmaking = false; // Reset matchmaking flag
            isBusy = false; // Reset busy flag
            findMatchButtonText.text = "Find Match"; // Update the button text
            queueStatusText.text = string.Empty; // Clear the queue status text
            queueTimerText.text = string.Empty; // Clear the queue timer text
            return;
        }

        if (isBusy) { return; } // Return if another operation is in progress

        ClientSingleton.Instance.GameManager.MatchmakeAsync(teamToggle.isOn, OnMatchMade); // Start matchmaking asynchronously
        findMatchButtonText.text = "Cancel"; // Update the button text
        queueStatusText.text = "Searching..."; // Update the queue status text
        timeInQueue = 0f; // Reset the time in queue
        isMatchmaking = true; // Set matchmaking flag
        isBusy = true; // Set busy flag
    }

    // Callback method to handle match result
    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatusText.text = "Connecting..."; // Update status text on success
                break;
            case MatchmakerPollingResult.TicketCreationError:
                queueStatusText.text = "TicketCreationError"; // Update status text on ticket creation error
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                queueStatusText.text = "TicketCancellationError"; // Update status text on ticket cancellation error
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                queueStatusText.text = "TicketRetrievalError"; // Update status text on ticket retrieval error
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                queueStatusText.text = "MatchAssignmentError"; // Update status text on match assignment error
                break;
        }
    }

    // Method to start the host asynchronously
    public async void StartHost()
    {
        if (isBusy) { return; } // Return if another operation is in progress

        isBusy = true; // Set busy flag

        await HostSingleton.Instance.GameManager.StartHostAsync(privateToggle.isOn); // Await the start of the host

        isBusy = false; // Reset busy flag
    }

    // Method to start the client asynchronously
    public async void StartClient()
    {
        if (isBusy) { return; } // Return if another operation is in progress

        isBusy = true; // Set busy flag

        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text); // Await the start of the client with the join code

        isBusy = false; // Reset busy flag
    }

    // Method to join a lobby asynchronously
    public async void JoinAsync(Lobby lobby)
    {
        if (isBusy) { return; } // Return if another operation is in progress

        isBusy = true; // Set busy flag

        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id); // Attempt to join the lobby by ID
            string joinCode = joiningLobby.Data["JoinCode"].Value; // Retrieve the join code from the lobby data

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode); // Start the client with the join code
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e); // Log any exceptions
        }

        isBusy = false; // Reset busy flag
    }
}
