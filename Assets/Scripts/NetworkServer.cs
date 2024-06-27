using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This class represents a network server in Unity using the Netcode for GameObjects package
public class NetworkServer : IDisposable
{
    // Private field to store the NetworkManager instance
    private NetworkManager networkManager;

    // Dictionary to map client IDs to their authentication IDs
    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    // Dictionary to map authentication IDs to their user data
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    // Constructor to initialize the NetworkServer with a NetworkManager instance
    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager; // Assign the passed NetworkManager to the local field

        // Subscribe to the ConnectionApprovalCallback event to handle connection approvals
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        // Subscribe to the OnServerStarted event to handle network initialization
        networkManager.OnServerStarted += OnNetworkReady;
    }

    // Method to handle connection approval checks
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // Decode the payload data from the connection request
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);

        // Deserialize the payload into a UserData object
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        // Map the client ID to the authentication ID
        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        // Map the authentication ID to the user data
        authIdToUserData[userData.userAuthId] = userData;

        // Approve the connection request
        response.Approved = true;
        // Indicate that a player object should be created for this connection
        response.CreatePlayerObject = true;
    }

    // Method called when the network is ready
    private void OnNetworkReady()
    {
        // Subscribe to the OnClientDisconnectCallback event to handle client disconnections
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Method to handle client disconnections
    private void OnClientDisconnect(ulong clientId)
    {
        // Try to get the authentication ID for the disconnecting client
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            // Remove the client ID from the client-to-auth dictionary
            clientIdToAuth.Remove(clientId);
            // Remove the user data from the auth-to-user data dictionary
            authIdToUserData.Remove(authId);
        }
    }

    // Method to get user data by client ID
    public UserData GetUserDataByClientId(ulong clientId)
    {
        // Try to get the authentication ID for the given client ID
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            // Try to get the user data for the given authentication ID
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data; // Return the user data if found
            }

            return null; // Return null if user data not found
        }

        return null; // Return null if authentication ID not found
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        if (networkManager == null) { return; }

        // Unsubscribe from events to prevent memory leaks
        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        // Shut down the network manager if it is still listening
        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
