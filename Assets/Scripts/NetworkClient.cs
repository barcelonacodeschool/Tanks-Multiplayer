using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Class to manage client-side network operations
public class NetworkClient : IDisposable
{
    // Private field to store the NetworkManager instance
    private NetworkManager networkManager;

    // Constant for the menu scene name
    private const string MenuSceneName = "Main Menu";

    // Constructor to initialize the NetworkClient with a NetworkManager instance
    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager; // Assign the passed NetworkManager to the local field

        // Subscribe to the OnClientDisconnectCallback event to handle client disconnections
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Method to handle client disconnections
    private void OnClientDisconnect(ulong clientId)
    {
        // Check if the disconnected client is not the local client or a special client ID
        if (clientId != 0 && clientId != networkManager.LocalClientId) { return; }

        // Check if the active scene is not the menu scene, and if so, load the menu scene
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        // If the client is still connected, shut down the network manager
        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        // Unsubscribe from the OnClientDisconnectCallback event if the network manager is not null
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
