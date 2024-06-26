using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// NameSelector handles player name input and validation, and transitions to the next scene
public class NameSelector : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField; // Input field for entering the player name
    [SerializeField] private Button connectButton; // Button to connect and proceed to the next scene
    [SerializeField] private int minNameLength = 1; // Minimum length for the player name
    [SerializeField] private int maxNameLength = 12; // Maximum length for the player name

    public const string PlayerNameKey = "PlayerName"; // Key to store and retrieve the player name in PlayerPrefs

    // This method is called when the script instance is being loaded
    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Skip name input if running in headless mode
            return;
        }

        nameField.text = PlayerPrefs.GetString(PlayerNameKey, string.Empty); // Load the saved player name from PlayerPrefs
        HandleNameChanged(); // Update the connect button state based on the loaded name
    }

    // Method to handle changes in the name input field
    public void HandleNameChanged()
    {
        connectButton.interactable =
            nameField.text.Length >= minNameLength && // Enable connect button if name length is within valid range
            nameField.text.Length <= maxNameLength;
    }

    // Method to handle the connect button click
    public void Connect()
    {
        PlayerPrefs.SetString(PlayerNameKey, nameField.text); // Save the player name in PlayerPrefs

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Load the next scene
    }
}
