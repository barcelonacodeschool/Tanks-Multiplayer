using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// ClientSingleton ensures a single instance of the client manager and handles its initialization
public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance; // Static instance of ClientSingleton

    public ClientGameManager GameManager { get; private set; } // Public property to access the game manager instance

    // Property to get the single instance of ClientSingleton
    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; } // Return existing instance if available

            instance = FindObjectOfType<ClientSingleton>(); // Find instance in the scene

            if (instance == null)
            {
                Debug.LogError("No ClientSingleton in the scene"); // Log an error if no instance found
                return null;
            }

            //Debug.Log("ClientSingleton instance found.");
            return instance;
        }
    }

    private bool isClientCreated = false; // Flag to track if the client is already created

    // This method is called when the script instance is being loaded
    private async void Start()
    {
        DontDestroyOnLoad(gameObject); // Prevent this game object from being destroyed on scene load
        //Debug.Log("ClientSingleton Start method called.");

        if (!isClientCreated)
        {
            isClientCreated = true; // Set flag to indicate client creation in progress
            bool clientCreated = await CreateClient(); // Create the client asynchronously
            //Debug.Log($"Client creation result: {clientCreated}");

            if (clientCreated)
            {
                GameManager.GoToMenu(); // Navigate to the menu if client creation is successful
            }
        }
    }

    // Method to create and initialize the client asynchronously
    public async Task<bool> CreateClient()
    {
        if (GameManager == null)
        {
            GameManager = new ClientGameManager(); // Instantiate the client game manager
            //Debug.Log("ClientGameManager instance created.");
        }

        bool initResult = await GameManager.InitAsync(); // Initialize the game manager asynchronously
        //Debug.Log($"ClientGameManager initialization result: {initResult}");

        return initResult; // Return the initialization result
    }
}