using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Class representing the game HUD
public class GameHUD : NetworkBehaviour
{
    // Reference to the lobby code text UI element
    [SerializeField] private TMP_Text lobbyCodeText;

    // Network variable to store the lobby code
    private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>("");

    // Method called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Check if the instance is a client
        if (IsClient)
        {
            // Subscribe to the lobby code value change event
            lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
            // Handle the initial lobby code change
            HandleLobbyCodeChanged(string.Empty, lobbyCode.Value);
        }

        // Check if the instance is the host
        if (!IsHost) { return; }

        // Set the lobby code value to the host's join code
        lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
    }

    // Method called when the network object is despawned
    public override void OnNetworkDespawn()
    {
        // Check if the instance is a client
        if (IsClient)
        {
            // Unsubscribe from the lobby code value change event
            lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
        }
    }

    // Method to handle leaving the game
    public void LeaveGame()
    {
        // Check if the current instance is the host
        if (NetworkManager.Singleton.IsHost)
        {
            // Shut down the game manager if host
            HostSingleton.Instance.GameManager.Shutdown();
        }

        // Disconnect the client from the game
        ClientSingleton.Instance.GameManager.Disconnect();
    }

    // Method to handle lobby code changes
    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        // Update the lobby code text UI element with the new code
        lobbyCodeText.text = newCode.ToString();
    }
}
