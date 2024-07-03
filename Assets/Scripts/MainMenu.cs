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
            // Cancel matchmaking (implementation not shown)
            isCancelling = false;
            isMatchmaking = false;
            findMatchButtonText.text = "Find Match"; // Update the button text
            queueStatusText.text = string.Empty; // Clear the queue status text
            return;
        }

        // Start queue
        findMatchButtonText.text = "Cancel"; // Update the button text
        queueStatusText.text = "Searching..."; // Update the queue status text
        isMatchmaking = true;
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
