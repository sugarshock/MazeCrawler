using Godot;
using System;
using Godot.Collections;

public partial class Inventory : Node3D
{
	[Export] public Godot.Collections.Array<Memory> Memories { get; set; } = new Array<Memory>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Events.MemoryCollected += OnMemoryCollected;
		Events.MemoryDelivered += OnMemoryDelivered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnMemoryCollected(Player player, Memory memory)
	{
		if(GetParent<Player>() == player)
			Memories.Add(memory);
	}
	
	public void OnMemoryDelivered(Player player, Memory memory, Soul soul)
	{
		if(GetParent<Player>() == player)
			Memories.Remove(memory);
	}
}
