using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This class handles dealing damage upon contact with another collider
public class DealDamageOnContact : MonoBehaviour
{
    // Reference to the Projectiles component
    [SerializeField] private Projectiles projectile;
    // Amount of damage to deal
    [SerializeField] private int damage = 5;

    // This method is called when another collider enters the trigger collider attached to this object
    private void OnTriggerEnter2D(Collider2D col)
    {
        // Check if the collider has an attached Rigidbody2D component
        if (col.attachedRigidbody == null) { return; }

        // Check if the projectile has a valid team index
        if (projectile.TeamIndex != -1)
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

        // Try to get the Health component from the attached Rigidbody2D
        if (col.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage); // Deal damage to the health component
        }
    }
}
