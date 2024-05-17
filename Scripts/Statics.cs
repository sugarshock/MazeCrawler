using Godot;

public class Statics
{
    public static PackedScene TRIGGER_SCENE = ResourceLoader.Load<PackedScene>("res://Maze/Scenes/TriggerArea.tscn");
    public static PackedScene CHUNK_SCENE = ResourceLoader.Load<PackedScene>("res://Maze/Scenes/Chunk.tscn");
    public static Vector2I[] Directions2I = new Vector2I[] { new Vector2I(0, 1), new Vector2I(1, 0), new Vector2I(0, -1), new Vector2I(-1, 0) };
    public static Vector3I[] Directions3I = new Vector3I[] { new Vector3I(0, 0, 1), new Vector3I(1, 0, 0), new Vector3I(0, 0, -1), new Vector3I(-1, 0, 0)};
}