using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BountyCoin class inherits from the Coin class
public class BountyCoin : Coin
{
    // Override the Collect method from the Coin class
    public override int Collect()
    {
        // Check if the code is running on the server
        if (!IsServer)
        {
            // Hide the coin if it's not on the server
            Show(false);
            // Return 0 since the coin is not collected
            return 0;
        }

        // Check if the coin has already been collected
        if (alreadyCollected) { return 0; }

        // Mark the coin as collected
        alreadyCollected = true;

        // Destroy the coin game object
        Destroy(gameObject);

        // Return the coin's value
        return coinValue;
    }
}
