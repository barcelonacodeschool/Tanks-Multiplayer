using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// ClientSingleton ensures a single instance of the client manager and handles its initialization
public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance; // Static instance of ClientSingleton

    private ClientGameManager gameManager; // Instance of the client game manager

    // Property to get the single instance of ClientSingleton
    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; } // Return existing instance if available

            instance = FindObjectOfType<ClientSingleton>(); // Find instance in the scene

            if (instance == null)
            {
                Debug.LogError("No ClientSingleton in the scene!"); // Log an error if no instance found
                return null;
            }

            return instance;
        }
    }

    // This method is called when the script instance is being loaded
    private void Start()
    {
        DontDestroyOnLoad(gameObject); // Prevent this game object from being destroyed on scene load
    }

    // Method to create and initialize the client asynchronously
    public async Task CreateClient()
    {
        gameManager = new ClientGameManager(); // Instantiate the client game manager

        await gameManager.InitAsync(); // Initialize the game manager asynchronously
    }
}