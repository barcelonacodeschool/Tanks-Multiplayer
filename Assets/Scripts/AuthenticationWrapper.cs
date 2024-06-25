using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

// AuthenticationWrapper handles user authentication processes
public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated; // Current authentication state
    private static TaskCompletionSource<AuthState> authTaskCompletionSource; // Task completion source for managing async auth tasks

    // Method to perform authentication, retries up to a maximum number of tries
    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        Debug.Log("Starting authentication...");

        if (AuthState == AuthState.Authenticated)
        {
            Debug.Log("Already authenticated.");
            return AuthState; // Return if already authenticated
        }

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already in the process of authenticating.");
            return await authTaskCompletionSource.Task;  // Wait for the current authentication to complete
        }

        AuthState = AuthState.Authenticating; // Set state to authenticating
        authTaskCompletionSource = new TaskCompletionSource<AuthState>(); // Initialize the task completion source

        int tries = 0;
        while (AuthState == AuthState.Authenticating && tries < maxTries)
        {
            Debug.Log($"Attempt {tries + 1} to authenticate...");

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Attempt anonymous sign-in

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated; // Set state to authenticated if sign-in is successful
                    Debug.Log("Authentication successful.");
                    authTaskCompletionSource.SetResult(AuthState); // Set the result for the task completion source
                    break;
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Authentication failed with exception: {ex.Message}");
                AuthState = AuthState.Error; // Set state to error if an exception occurs
                authTaskCompletionSource.SetResult(AuthState); // Set the result for the task completion source
                break;
            }

            tries++;
            Debug.LogWarning("Authentication failed. Retrying...");
            await Task.Delay(2000); // Wait for 2 seconds before retrying
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogError("Authentication failed after maximum retries.");
            AuthState = AuthState.Error; // Set state to error if maximum retries are reached
            authTaskCompletionSource.SetResult(AuthState); // Set the result for the task completion source
        }

        return AuthState; // Return the current authentication state
    }
}

// Enum to represent different authentication states
public enum AuthState
{
    NotAuthenticated, // State when not authenticated
    Authenticating,   // State when authentication is in progress
    Authenticated,    // State when authenticated
    Error,            // State when an error occurs
    TimeOut           // State when authentication times out
}