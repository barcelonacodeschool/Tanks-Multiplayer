using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

// HostGameManager manages host-side game operations, including starting a host and managing lobbies
public class HostGameManager
{
    private Allocation allocation; // Allocation for creating a relay server
    private string joinCode; // Join code for clients to join the relay server
    private string lobbyId; // ID of the created lobby

    // Field to store the network server instance
    private NetworkServer networkServer;

    private const int MaxConnections = 20; // Maximum number of connections to the relay server
    private const string GameSceneName = "Game"; // Name of the game scene

    // Method to start the host asynchronously
    public async Task StartHostAsync()
    {
        try
        {
            // Create an allocation for the game relay with the maximum number of connections
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            // Log any exceptions that occur during allocation creation
            Debug.Log(e);
            return;
        }

        try
        {
            // Get the join code for the allocation
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode); // Log the join code
        }
        catch (Exception e)
        {
            Debug.Log(e); // Log any exceptions that occur while getting the join code
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); // Get the UnityTransport component from the NetworkManager

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); // Create relay server data using the allocation
        transport.SetRelayServerData(relayServerData); // Set the relay server data for the transport

        try
        {
            // Set up lobby options and create a lobby
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false; // Set the lobby to be public
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode // Store the join code in the lobby data
                    )
                }
            };
            // Get the player's name and create a lobby with it
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MaxConnections, lobbyOptions);

            lobbyId = lobby.Id; // Store the lobby ID
            HostSingleton.Instance.StartCoroutine(HearbeatLobby(15)); // Start the lobby heartbeat coroutine
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e); // Log any exceptions that occur while creating the lobby
            return;
        }

        // Initialize the network server
        networkServer = new NetworkServer(NetworkManager.Singleton);

        // Prepare connection data with the user's name
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // Set the connection data for the network manager
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        // Start hosting the game
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single); // Load the game scene
    }

    // Coroutine to send heartbeat pings to keep the lobby alive
    private IEnumerator HearbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds); // Create a delay object to wait between pings
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId); // Send heartbeat ping
            yield return delay; // Wait for the specified delay time
        }
    }
}
