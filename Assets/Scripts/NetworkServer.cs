using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This class represents a network server in Unity using the Netcode for GameObjects package
public class NetworkServer
{
    // Private field to store the NetworkManager instance
    private NetworkManager networkManager;

    // Constructor to initialize the NetworkServer with a NetworkManager instance
    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager; // Assign the passed NetworkManager to the local field

        // Subscribe to the ConnectionApprovalCallback event to handle connection approvals
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
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

        // Log the user name to the console for debugging purposes
        Debug.Log(userData.userName);

        // Approve the connection request
        response.Approved = true;
        // Indicate that a player object should be created for this connection
        response.CreatePlayerObject = true;
    }
}
