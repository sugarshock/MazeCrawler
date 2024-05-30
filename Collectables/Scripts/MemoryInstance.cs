using Godot;
using System;

public partial class MemoryInstance : Node3D
{
	public Memory Memory { get; set; }

	private Area3D _triggerArea; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_triggerArea = GetNode<Area3D>("Area3D");
		_triggerArea.BodyEntered += OnBodyEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			Events.MemoryCollected?.Invoke(player, Memory);
			// Play animation
			CallDeferred(MethodName.QueueFree);
		}
	}
}
