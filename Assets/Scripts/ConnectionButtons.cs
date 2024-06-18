using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectionButtons : MonoBehaviour
{
    // Method to start the game as a host
    // This method is called when a button is clicked in the UI
    public void StartHost()
    {
        // Start the network manager in host mode
        // This creates a server and connects the host client to it
        NetworkManager.Singleton.StartHost();
    }

    // Method to start the game as a client
    // This method is called when a button is clicked in the UI
    public void StartClient()
    {
        // Start the network manager in client mode
        // This connects the client to an existing server
        NetworkManager.Singleton.StartClient();
    }
}
