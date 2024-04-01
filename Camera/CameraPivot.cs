using Godot;
using System;

public partial class CameraPivot : Node3D
{
	[Export] public float rotationSpeed = 3.0f;

	[Export] public float zoomFactor = 1.0f;
	[Export] public float minZoom = 0.2f;
	[Export] public float maxZoom = 5.0f;
	[Export] public float zoomDuration = 0.2f;


	private Camera3D camera;
	private PlaneMesh fogMask;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		fogMask = (PlaneMesh) camera?.GetNode<MeshInstance3D>("Fog")?.Mesh;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float rotateDir = Input.GetAxis("rotate_right", "rotate_left");
		RotateY(rotateDir * rotationSpeed * (float) delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if(@event.IsActionPressed("zoom_in"))
			SetZoomLevel(camera.Size - zoomFactor);
		else if(@event.IsActionPressed("zoom_out"))
			SetZoomLevel(camera.Size + zoomFactor);
	}

	public void SetZoomLevel(float value)
	{
		var tween = GetTree().CreateTween();
		value = Math.Clamp(value, minZoom, maxZoom);
		tween.TweenProperty(camera, "size", value, zoomDuration)
			.SetTrans(Tween.TransitionType.Quad) //circ!
			.SetEase(Tween.EaseType.Out);
		
		/*tween.Parallel()
			.TweenProperty(fogMask, "size", new Vector2((value/10) * 18, value), zoomDuration)
			.SetTrans(Tween.TransitionType.Quad) //circ!
			.SetEase(Tween.EaseType.Out);*/

	}
}
