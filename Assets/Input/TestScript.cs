using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the MoveEvent from the inputReader
        // This means that the HandleMove method will be called whenever the MoveEvent is triggered
        inputReader.MoveEvent += HandleMove;
    }

    // This method is called when the script is being destroyed
    // It is used to clean up any event subscriptions
    private void OnDestroy()
    {
        // Unsubscribe from the MoveEvent to prevent potential memory leaks
        // This ensures that HandleMove is no longer called after the script is destroyed
        inputReader.MoveEvent -= HandleMove;
    }

    // Method to handle movement input
    // This method is called whenever the MoveEvent is triggered
    private void HandleMove(Vector2 movement)
    {
        // Just logging the movement vector to the console
        Debug.Log(movement);
    }
}
