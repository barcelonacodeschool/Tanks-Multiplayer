using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

// ClientGameManager manages client-side game operations, including initialization and scene management
public class ClientGameManager
{
    private JoinAllocation allocation; // Allocation for joining a relay server

    private const string MenuSceneName = "Main Menu"; // Name of the main menu scene

    // Method to initialize Unity services and authenticate the client asynchronously
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync(); // Initialize Unity services

        AuthState authState = await AuthenticationWrapper.DoAuth(); // Perform authentication

        if (authState == AuthState.Authenticated)
        {
            return true; // Return true if authentication is successful
        }

        return false; // Return false if authentication fails
    }

    // Method to navigate to the main menu
    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName); // Load the main menu scene
    }

    // Method to start the client asynchronously using a join code
    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode); // Join relay allocation using the join code
        }
        catch (Exception e)
        {
            Debug.Log(e); // Log any exceptions
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); // Get the UnityTransport component from the NetworkManager

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); // Create relay server data using the allocation
        transport.SetRelayServerData(relayServerData); // Set the relay server data for the transport

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name") // Retrieve the player name from PlayerPrefs
        };
        string payload = JsonUtility.ToJson(userData); // Serialize the user data to JSON
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload); // Encode the JSON to bytes

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes; // Set the connection data for the NetworkManager

        NetworkManager.Singleton.StartClient(); // Start the client
    }
}
