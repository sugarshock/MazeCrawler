using Godot;

public class Statics
{
    public static PackedScene TRIGGER_SCENE = ResourceLoader.Load<PackedScene>("res://Maze/Scenes/TriggerArea.tscn");
    public static PackedScene CHUNK_SCENE = ResourceLoader.Load<PackedScene>("res://Maze/Scenes/Chunk.tscn");
    public static PackedScene MEMORY_SCENE = ResourceLoader.Load<PackedScene>("res://Collectables/Scenes/MemoryInstance.tscn");
    public static PackedScene SOUL_SCENE = ResourceLoader.Load<PackedScene>("res://Collectables/Scenes/SoulInstance.tscn");
    
    public static Soul EXAMPLE_SOUL = ResourceLoader.Load<Soul>("res://Collectables/Resources/Souls/ExampleSoul.tres");
    public static Memory EXAMPLE_MEMORY = ResourceLoader.Load<Memory>("res://Collectables/Resources/Memories/ExampleMemory.tres");
    
    public static Vector2I[] Directions2I = new Vector2I[] { new Vector2I(0, 1), new Vector2I(1, 0), new Vector2I(0, -1), new Vector2I(-1, 0) };
    public static Vector3I[] Directions3I = new Vector3I[] { new Vector3I(0, 0, 1), new Vector3I(1, 0, 0), new Vector3I(0, 0, -1), new Vector3I(-1, 0, 0)};
}