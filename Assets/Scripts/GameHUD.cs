using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Class representing the game HUD
public class GameHUD : MonoBehaviour
{
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
}