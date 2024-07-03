using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// ApplicationController manages the launch and mode of the application (client or host)
public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab; // Reference to the client singleton prefab
    [SerializeField] private HostSingleton hostPrefab; // Reference to the host singleton prefab
    [SerializeField] private ServerSingleton serverPrefab; // Reference to the server singleton prefab
    [SerializeField] private NetworkObject playerPrefab; // Reference to the player prefab

    private ApplicationData appData; // Instance of ApplicationData to manage application-specific data

    private const string GameSceneName = "Game"; // Name of the game scene

    // This method is called when the script instance is being loaded
    private async void Start()
    {
        DontDestroyOnLoad(gameObject); // Prevent this game object from being destroyed on scene load

        // Launch the application in the appropriate mode based on whether it is a dedicated server
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    // Method to launch the application in the appropriate mode (dedicated server or client/host)
    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            Application.targetFrameRate = 60; // Set the target frame rate for the application

            // Initialize application data for a dedicated server
            appData = new ApplicationData();

            // Instantiate the server singleton
            ServerSingleton serverSingleton = Instantiate(serverPrefab);

            // Load the game scene asynchronously and start the server
            StartCoroutine(LoadGameSceneAsync(serverSingleton));
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(hostPrefab); // Instantiate the host singleton
            hostSingleton.CreateHost(playerPrefab); // Create the host

            ClientSingleton clientSingleton = Instantiate(clientPrefab); // Instantiate the client singleton
            await clientSingleton.CreateClient(); // Create the client asynchronously

            // Logic to navigate to the main menu can be added here
        }
    }

    // Coroutine to load the game scene asynchronously and start the server
    private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
    {
        // Start loading the game scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameSceneName);

        // Wait until the scene is fully loaded
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // Create the server with the player prefab
        Task createServerTask = serverSingleton.CreateServer(playerPrefab);
        yield return new WaitUntil(() => createServerTask.IsCompleted);

        // Start the game server asynchronously
        Task startServerTask = serverSingleton.GameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServerTask.IsCompleted);
    }
}
