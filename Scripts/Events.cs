using Godot;
using System;

public static class Events
{
    public static Action<Chunk> PlayerEnteredChunk;
    public static Action<Vector3> PlayerPositionChanged;
}
