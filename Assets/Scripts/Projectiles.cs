using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    // Property to store the team index
    public int TeamIndex { get; private set; }

    // Method to initialize the projectile with a team index
    public void Initialise(int teamIndex)
    {
        TeamIndex = teamIndex; // Set the team index
    }
}