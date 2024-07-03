using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    // Static instance of the ServerSingleton
    private static ServerSingleton instance;

    // Property to get the game manager
    public ServerGameManager GameManager { get; private set; }

    // Property to get the singleton instance
    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }

            // Find the instance of ServerSingleton in the scene
            instance = FindObjectOfType<ServerSingleton>();

            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    // Method called when the script instance is being loaded
    private void Start()
    {
        // Ensure the game object is not destroyed when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    // Asynchronous method to create the server
    public async Task CreateServer(NetworkObject playerPrefab)
    {
        // Initialize Unity Services
        await UnityServices.InitializeAsync();

        // Create the game manager with the server IP, port, query port, and network manager
        GameManager = new ServerGameManager(
            ApplicationData.IP(),
            ApplicationData.Port(),
            ApplicationData.QPort(),
            NetworkManager.Singleton,
            playerPrefab
        );
    }

    // Method called when the MonoBehaviour will be destroyed
    private void OnDestroy()
    {
        // Dispose of the game manager if it exists
        GameManager?.Dispose();
    }
}
