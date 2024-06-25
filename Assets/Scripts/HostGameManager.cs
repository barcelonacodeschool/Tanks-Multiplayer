using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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

    private const int MaxConnections = 20; // Maximum number of connections to the relay server
    private const string GameSceneName = "Game"; // Name of the game scene

    // Method to start the host asynchronously
    public async Task StartHostAsync()
    {
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections); // Create relay allocation
        }
        catch (Exception e)
        {
            Debug.Log(e); // Log any exceptions
            return;
        }

        try
        {
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId); // Get join code for the relay allocation
            Debug.Log(joinCode); // Log the join code
        }
        catch (Exception e)
        {
            Debug.Log(e); // Log any exceptions
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); // Get the UnityTransport component from the NetworkManager

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); // Create relay server data using the allocation
        transport.SetRelayServerData(relayServerData); // Set the relay server data for the transport

        try
        {
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
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                "My Lobby", MaxConnections, lobbyOptions); // Create the lobby

            lobbyId = lobby.Id; // Store the lobby ID

            HostSingleton.Instance.StartCoroutine(HearbeatLobby(15)); // Start the lobby heartbeat coroutine
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e); // Log any exceptions
            return;
        }

        NetworkManager.Singleton.StartHost(); // Start the host

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single); // Load the game scene
    }

    // Coroutine to send heartbeat pings to keep the lobby alive
    private IEnumerator HearbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds); // Create delay object
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId); // Send heartbeat ping
            yield return delay; // Wait for the specified delay time
        }
    }
}
