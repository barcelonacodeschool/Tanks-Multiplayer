using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to manage spawn points in the game
public class SpawnPoint : MonoBehaviour
{
    // Static list to store all spawn points
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    // Method called when the object is enabled
    private void OnEnable()
    {
        // Add this spawn point to the list
        spawnPoints.Add(this);
    }

    // Method called when the object is disabled
    private void OnDisable()
    {
        // Remove this spawn point from the list
        spawnPoints.Remove(this);
    }

    // Static method to get a random spawn position
    public static Vector3 GetRandomSpawnPos()
    {
        // Return zero vector if there are no spawn points
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        // Return the position of a randomly selected spawn point
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    // Method to draw gizmos in the editor when this object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue; // Set gizmo color to blue
        Gizmos.DrawSphere(transform.position, 1); // Draw a sphere at the spawn point's position
    }
}
