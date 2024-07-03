using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAligner : MonoBehaviour
{
    private ParticleSystem.MainModule psMain; // Main module of the ParticleSystem

    // Method called when the script instance is being loaded
    private void Start()
    {
        // Get the main module of the ParticleSystem
        psMain = GetComponent<ParticleSystem>().main;
    }

    // Method called every frame to update the particle rotation
    private void Update()
    {
        // Align the start rotation of the particles with the inverse of the object's z rotation
        psMain.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
    }
}
