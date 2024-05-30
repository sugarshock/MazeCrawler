using Godot;
using System;
using System.Linq;

public partial class SoulInstance : CharacterBody3D
{
	public Soul Soul { get; set; }
	
	private Area3D _triggerArea;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_triggerArea = GetNode<Area3D>("MemoryTriggerArea");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void OnTriggerEnter(Node body)
	{
		if (body is Player player)
		{
			var correspMem = player.Inventory.Memories.FirstOrDefault(m => m.SoulId == Soul.Id);
			if(correspMem == null)
				return;
			Events.MemoryDelivered?.Invoke(player, correspMem, Soul);
			// Play animation
			CallDeferred(MethodName.QueueFree);
		}
	}
	
}
