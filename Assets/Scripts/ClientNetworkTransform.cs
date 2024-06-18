using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

// ClientNetworkTransform class extends NetworkTransform to handle client-side networked transformations
public class ClientNetworkTransform : NetworkTransform
{
    // This method is called when the network object is spawned
    public override void OnNetworkSpawn()
    {
        // Call the base class's OnNetworkSpawn method
        base.OnNetworkSpawn();
        // Set CanCommitToTransform to true if this client owns the object
        CanCommitToTransform = IsOwner;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Ensure that CanCommitToTransform is true only if this client owns the object
        CanCommitToTransform = IsOwner;
        // Call the base class's Update method
        base.Update();

        // Check if the NetworkManager is not null (i.e., networking is active)
        if (NetworkManager != null)
        {
            // Check if the client is connected or the server is listening for connections
            if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
            {
                // If the client owns the object, commit the transform to the server
                if (CanCommitToTransform)
                {
                    // Try to commit the current transform to the server with the local time
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }

    // This method determines if the server has authority over the transform
    // Returning false means the client has authority
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
