using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text queueStatusText; // Text field to display queue status
    [SerializeField] private TMP_Text queueTimerText; // Text field to display queue timer
    [SerializeField] private TMP_Text findMatchButtonText; // Text field for the find match button
    [SerializeField] private TMP_InputField joinCodeField; // Input field for the join code

    private bool isMatchmaking; // Flag to indicate if matchmaking is in progress
    private bool isCancelling; // Flag to indicate if matchmaking cancellation is in progress

    // Method called when the script instance is being loaded
    private void Start()
    {
        if (ClientSingleton.Instance == null) { return; } // Return if there is no ClientSingleton instance

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Set the cursor to the default

        queueStatusText.text = string.Empty; // Clear the queue status text
        queueTimerText.text = string.Empty; // Clear the queue timer text
    }

    // Method called when the find match button is pressed
    public async void FindMatchPressed()
    {
        if (isCancelling) { return; } // Return if cancellation is in progress

        if (isMatchmaking)
        {
            queueStatusText.text = "Cancelling..."; // Update the queue status text
            isCancelling = true;
            await ClientSingleton.Instance.GameManager.CancelMatchmaking(); // Await the cancellation of matchmaking
            isCancelling = false;
            isMatchmaking = false;
            findMatchButtonText.text = "Find Match"; // Update the button text
            queueStatusText.text = string.Empty; // Clear the queue status text
            return;
        }

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); // Start matchmaking asynchronously
        findMatchButtonText.text = "Cancel"; // Update the button text
        queueStatusText.text = "Searching..."; // Update the queue status text
        isMatchmaking = true;
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
        await HostSingleton.Instance.GameManager.StartHostAsync(); // Await the start of the host
    }

    // Method to start the client asynchronously
    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text); // Await the start of the client with the join code
    }
}
