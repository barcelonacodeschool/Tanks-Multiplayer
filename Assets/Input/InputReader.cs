using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

// This script allows the creation of an Input Reader asset that handles player input.
// It listens for input actions defined in the Controls script and triggers corresponding events.
[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    // Events triggered by player input
    // These events can be subscribed to by other scripts to respond to input.
    public event Action<Vector2> MoveEvent; // Event for movement input, with a Vector2 parameter representing direction
    public event Action<bool> PrimaryFireEvent; // Event for primary fire input, with a bool parameter indicating if the action is performed or not

    public Vector2 AimPosition { get; private set; } // Property to store the aim position as a Vector2

    private Controls controls; // Reference to the input controls, defined in another script (Controls)

    // This method is called when the script is enabled (e.g., when the game starts)
    private void OnEnable()
    {
        // Initialize controls if they are not already initialized
        if (controls == null)
        {
            controls = new Controls(); // Create a new instance of Controls, which defines input actions
            controls.Player.SetCallbacks(this); // Set this script as the callback handler for player input actions
        }

        controls.Player.Enable(); // Enable the player controls so they start listening for input
    }

    // This method is called when the move input action is triggered
    // InputSystem calls this method automatically based on player input
    public void OnMove(InputAction.CallbackContext context)
    {
        // Invoke the MoveEvent, passing the current input value as a Vector2
        // The input value represents the direction of movement
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    // This method is called when the primary fire input action is triggered
    // InputSystem calls this method automatically based on player input
    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed) // Check if the input action is performed (button pressed)
        {
            PrimaryFireEvent?.Invoke(true); // Invoke the PrimaryFireEvent with true to indicate firing
        }
        else if (context.canceled) // Check if the input action is canceled (button released)
        {
            PrimaryFireEvent?.Invoke(false); // Invoke the PrimaryFireEvent with false to indicate stopping fire
        }
    }

    // This method is called when the aim input action is triggered
    // InputSystem calls this method automatically based on player input
    public void OnAim(InputAction.CallbackContext context)
    {
        // Read the aim position from the input and store it in AimPosition
        AimPosition = context.ReadValue<Vector2>();
    }
}
