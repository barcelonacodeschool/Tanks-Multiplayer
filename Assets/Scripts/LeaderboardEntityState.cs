using System;
using Unity.Collections;
using Unity.Netcode;

// Struct to represent the state of a leaderboard entity in a networked game
public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    // Client ID of the player
    public ulong ClientId;
    // Player name as a fixed-length string
    public FixedString32Bytes PlayerName;
    // Number of coins the player has
    public int Coins;

    // Method to serialize and deserialize the struct's data for network transmission
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId); // Serialize/deserialize the ClientId
        serializer.SerializeValue(ref PlayerName); // Serialize/deserialize the PlayerName
        serializer.SerializeValue(ref Coins); // Serialize/deserialize the Coins
    }

    // Method to check equality between two LeaderboardEntityState instances
    public bool Equals(LeaderboardEntityState other)
    {
        // Check if ClientId, PlayerName, and Coins are equal
        return ClientId == other.ClientId &&
            PlayerName.Equals(other.PlayerName) &&
            Coins == other.Coins;
    }
}
