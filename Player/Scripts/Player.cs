using Godot;
using System;

public partial class Player : CharacterBody3D
{

	public Action<Vector3> PositionChanged;

	[Export] public float Speed = 10.0f;
	public const float JumpVelocity = 5.0f;
	public const float SprintFactor = 1.5f;
	public const float SprintJumpFactor = 1.3f;

	public Node3D Armature;
	public AnimationTree AnimTree;
	private Node3D cameraPivot;
	private Vector3 internalVelocity = Vector3.Zero;
	private Fog fog;

	private bool isWalking = false;
	private bool isLanding = false;
	private bool isSprinting = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();


    public override void _Ready()
    {
     	cameraPivot = GetNode<Node3D>("CameraPivot");
		Armature = GetNode<Node3D>("Armature");
		AnimTree = GetNode<AnimationTree>("AnimationTree");
		AnimTree.Set("parameters/RootState/current", 0);
		fog = GetNode<Fog>("CameraPivot/Camera3D/Fog");
		fog.UncoverGauss(GlobalPosition);
    }



    public override void _PhysicsProcess(double delta)
	{
		/*var rot = SpringArmPivot.Rotation;
		rot.X = Mathf.LerpAngle(rot.X, 0, 0.5f);
		rot.Z = Mathf.LerpAngle(rot.Z, 0, 0.5f);
		SpringArmPivot.Rotation = rot;*/

		var rotA = Armature.Rotation;
		rotA.X = Mathf.LerpAngle(rotA.X, 0, 0.5f);
		rotA.Z = Mathf.LerpAngle(rotA.Z, 0, 0.5f);
		Armature.Rotation = rotA;

		/*springArm.SpringLength = Mathf.Lerp(springArm.SpringLength, 3, 0.15f);
		var pos = springArm.Position;
		pos.Y = Mathf.Lerp(pos.Y, 0.878f, 0.3f);
		springArm.Position = pos;*/

		if (IsOnFloor())
			ProcessOnGround();
		else
			ProcessInAir(delta);
		MoveAndSlide();

		// Set RootState based on IsOnFloor()
		AnimTree.Set("parameters/RootState/transition_request", IsOnFloor() ? "on-ground" : "in-air");
		// Set walking/idle based on current horizontal velocity
		AnimTree.Set("parameters/OnGround/blend_position", (new Vector2(Velocity.X, Velocity.Z).Length()) / Speed);
		// Set jump/fall/land based on vertical velocity
		AnimTree.Set("parameters/InAir/blend_position", Velocity.Y / JumpVelocity);

		if (!Velocity.IsZeroApprox())
		{
			PositionChanged?.Invoke(GlobalPosition);
			fog.UncoverGauss(GlobalPosition);
		}
			
	}


	private void ProcessOnGround()
	{	
		isSprinting = Input.IsActionPressed("sprint");
		isLanding = AnimTree.Get("parameters/Land/active").As<bool>();

		// Handle Jump.
		if (Input.IsActionJustPressed("jump"))
		{
			internalVelocity.Y = JumpVelocity;
			if(isSprinting)
			{
				internalVelocity.Y *= SprintJumpFactor;
			}
			AnimTree.Set("parameters/Land/request", 1);
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		direction = direction.Rotated(Vector3.Up, cameraPivot.Rotation.Y);
		if (direction != Vector3.Zero && !isLanding)
		{	
			isWalking = true;
			isSprinting = Input.IsActionPressed("sprint");
			var factor = isSprinting ? SprintFactor : 1.0f;

			internalVelocity.X = (float) Mathf.Lerp(internalVelocity.X, direction.X * Speed * factor, 0.5);
			internalVelocity.Z = (float) Mathf.Lerp(internalVelocity.Z, direction.Z * Speed * factor, 0.5);

			var rot = Armature.Rotation;
			rot.Y = Mathf.LerpAngle(rot.Y, Mathf.Atan2(-internalVelocity.X, -internalVelocity.Z), 0.5f);
			Armature.Rotation = rot;
		}
		else
		{
			internalVelocity.X = (float) Mathf.Lerp(internalVelocity.X, 0, 0.5);
			internalVelocity.Z = (float) Mathf.Lerp(internalVelocity.Z, 0, 0.5);
			isWalking = false;
			isSprinting = false;
		}
		Velocity = internalVelocity;
	}

	private void ProcessInAir(double delta)
	{	
		isLanding = false;
		isSprinting = false;

		internalVelocity.Y -= gravity * (float)delta;	
		
		// Get the input direction and handle the movement/deceleration. 
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		direction = direction.Rotated(Vector3.Up, cameraPivot.Rotation.Y);
		if (direction != Vector3.Zero && !isLanding)
		{	
			internalVelocity.X = (float) Mathf.Lerp(internalVelocity.X, direction.X * Speed, 0.5);
			internalVelocity.Z = (float) Mathf.Lerp(internalVelocity.Z, direction.Z * Speed, 0.5);
			var rot = Armature.Rotation;
			rot.Y = Mathf.LerpAngle(rot.Y, Mathf.Atan2(-internalVelocity.X, -internalVelocity.Z), 0.5f);
			Armature.Rotation = rot;
		}
		else
		{
			internalVelocity.X = (float) Mathf.Lerp(internalVelocity.X, 0, 0.5);
			internalVelocity.Z = (float) Mathf.Lerp(internalVelocity.Z, 0, 0.5);
		}

		//GD.Print("Process in Air: " + internalVelocity);
		Velocity = internalVelocity;
	}

	private void UpdateVisibility()
	{
		
	}
}
