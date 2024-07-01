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

public class ServerGameManager : IDisposable
{
    // Variables to store server IP, port, and query port
    private string serverIP;
    private int serverPort;
    private int queryPort;

    // Network server instance
    private NetworkServer networkServer;

    // Service for managing multiplay allocation
    private MultiplayAllocationService multiplayAllocationService;

    // Constant for the game scene name
    private const string GameSceneName = "Game";

    // Constructor to initialize the server game manager with IP, port, query port, and network manager
    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;

        // Initialize the network server with the provided network manager
        networkServer = new NetworkServer(manager);

        // Initialize the multiplay allocation service
        multiplayAllocationService = new MultiplayAllocationService();
    }

    // Asynchronous method to start the game server
    public async Task StartGameServerAsync()
    {
        // Begin server checks
        await multiplayAllocationService.BeginServerCheck();

        // Open network server connection and check if it started successfully
        if (!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("NetworkServer did not start as expected.");
            return;
        }

        // Load the game scene
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    // Dispose method to clean up resources
    public void Dispose()
    {
        // Dispose of the multiplay allocation service and network server
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
