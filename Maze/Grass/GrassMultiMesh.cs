using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GrassMultiMesh : MultiMeshInstance3D
{
	private GDScript _grassFactory;

	[Export] public Mesh GrassMesh { get; set; }
	
	[Export] public Vector2 BladeWidth { get; set; } = new Vector2(0.01f, 0.02f);
	[Export] public Vector2 BladeHeight { get; set; } = new Vector2(0.04f, 0.08f);
	[Export] public Vector2 SwayYaw { get; set; } = new Vector2(0.0f, 10.0f);
	[Export] public Vector2 SwayPitch { get; set; } = new Vector2(0.0f, 10.0f);
	[Export] public float Density { get; set; } = 1.0f;
	
	public override void _Ready()
	{
		_grassFactory = ResourceLoader.Load<GDScript>("res://Maze/Grass/grass_factory.gd");
		var mesh = new PlaneMesh();
		mesh.Size = new Vector2(100, 100);
		mesh.SubdivideWidth = 10;
		mesh.SubdivideDepth = 10;
		
		var arraymesh = new ArrayMesh();
		arraymesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, mesh.GetMeshArrays());
		
		PopulateMesh(arraymesh);
	}
	


	public void PopulateMesh(Mesh mesh)
	{
		Multimesh = new MultiMesh();
		Multimesh.Mesh = GrassMesh;
		// call return empty variant for some reason...
		var spawnPoints = _grassFactory.Call("generate", mesh, Density, BladeWidth, BladeHeight, SwayPitch, SwayYaw)
			.AsGodotArray();
		if(spawnPoints.Count == 0)
			return;

		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		Multimesh.UseCustomData = true;
		Multimesh.UseColors = true;
		Multimesh.InstanceCount = spawnPoints.Count;

		foreach(var index in Enumerable.Range(0, spawnPoints.Count))
		{
			var spawn = spawnPoints[index].AsGodotArray();
			var basis = new Basis(Vector3.Up, Mathf.DegToRad(GD.RandRange(0, 359)));
			Multimesh.SetInstanceTransform(index, spawn[0].AsTransform3D());
			Multimesh.SetInstanceCustomData(index, spawn[1].AsColor());
		}
	}


	public void PopulateMeshes(Godot.Collections.Array meshes)
	{
		throw new NotImplementedException();
		Multimesh.InstanceCount = 0;
		var spawnPoints = GenerateBladeSpawns(new Mesh[]{});
		if(spawnPoints.Count == 0)
			return;

		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		Multimesh.UseCustomData = true;
		Multimesh.UseColors = true;
		Multimesh.InstanceCount = spawnPoints.Count;

		foreach(var index in Enumerable.Range(0, spawnPoints.Count))
		{
			var spawn = spawnPoints[index].AsGodotArray();
			var basis = new Basis(Vector3.Up, Mathf.DegToRad(GD.RandRange(0, 359)));
			Multimesh.SetInstanceTransform(index, spawn[0].AsTransform3D());
			Multimesh.SetInstanceCustomData(index, spawn[1].AsColor());
		}

	}

	private List<Variant> GenerateBladeSpawns(Mesh[] meshes)
	{
		return meshes.SelectMany
			(
				mesh => _grassFactory.Call("generate", mesh, Density, BladeWidth, BladeHeight,SwayPitch, SwayYaw).AsGodotArray()
			).ToList();
	}
	
	
	


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
