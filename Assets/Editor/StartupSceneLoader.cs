using System;
using UnityEditor;
using UnityEditor.SceneManagement;

// Static class to automatically load the startup scene when entering play mode in the Unity editor
[InitializeOnLoad]
public static class StartupSceneLoader
{
    // Static constructor to subscribe to the playModeStateChanged event
    static StartupSceneLoader()
    {
        // Subscribe to the playModeStateChanged event to handle play mode state changes
        EditorApplication.playModeStateChanged += LoadStartupScene;
    }

    // Method to load the startup scene based on the play mode state
    private static void LoadStartupScene(PlayModeStateChange state)
    {
        // If exiting edit mode, prompt the user to save any modified scenes
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        // If entering play mode, check if the active scene is not the startup scene (build index 0)
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (EditorSceneManager.GetActiveScene().buildIndex != 0)
            {
                // Load the startup scene if the active scene is not the startup scene
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}
