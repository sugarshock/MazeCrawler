using Godot;


public partial class RuleResource : Resource
{
    [Export] public int Id;
    [Export] public int Orientation;
    [Export] public Vector3I Direction;
    [Export] public int NeighborId;
    [Export] public int NeighborOrientation;
}