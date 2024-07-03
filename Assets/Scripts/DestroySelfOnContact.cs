using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles the destruction of the game object upon contact with another collider
public class DestroySelfOnContact : MonoBehaviour
{
    // Reference to the Projectiles component
    [SerializeField] private Projectiles projectile;

    // This method is called when another collider enters the trigger collider attached to this object
    void OnTriggerEnter2D(Collider2D col)
    {
        // Check if the projectile has a valid team index
        if (projectile.TeamIndex != -1)
        {
            // Check if the collider has an attached Rigidbody2D component
            if (col.attachedRigidbody != null)
            {
                // Try to get the TankPlayer component from the attached Rigidbody2D
                if (col.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
                {
                    // Check if the player belongs to the same team as the projectile
                    if (player.TeamIndex.Value == projectile.TeamIndex)
                    {
                        return; // Return early if the player is on the same team
                    }
                }
            }
        }

        // Destroy the game object if the conditions are not met
        Destroy(gameObject);
    }
}
