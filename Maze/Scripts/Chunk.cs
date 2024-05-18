using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Chunk : GridMap
{
	
	[Export] public Vector3I MinCell { get; private set; }
	[Export] public Vector3I MaxCell { get; private set; }

	public Maze Maze { get; private set; }
	public Dictionary<Vector3I, Vector3I> Connections { get; private set; }
	public float[,] VisibilityMap { get; private set; }
	
	public Aabb Aabb { get; private set; }

	public bool IsBoundary = true;
	
	
	private Area3D _triggerArea;
	
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Maze == null)
			throw new Exception("Scene must be set up before being added to the tree");
		
		_triggerArea = GetNode<Area3D>("TriggerArea");
		_triggerArea.Position = Aabb.GetCenter();
		_triggerArea.Scale = Aabb.Size;
		_triggerArea.BodyEntered += OnBodyEntered;
	}
	
	public Chunk Setup(Maze maze, Vector3I minCell, Vector3I maxCell)
	{
		Maze = maze;
		MinCell = minCell;
		MaxCell = maxCell;
		Connections = new Dictionary<Vector3I, Vector3I>();
		CellSize = new Vector3(maze.CELL_SIZE, maze.CELL_SIZE, maze.CELL_SIZE);
		CellScale = maze.CELL_SIZE;
		
		InitializeChunk();
		SetupVisibilityMap();
		
		Aabb = new Aabb()
		{
			Position = MapToLocal(MinCell),
			End = MapToLocal(MaxCell)
		};
		
		return this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public bool Contains(Vector3I cell)
	{
		return cell.X >= MinCell.X && cell.X <= MaxCell.X 
		                           && cell.Z >= MinCell.Z && cell.Z <= MaxCell.Z;
	}

	private void SetupVisibilityMap()
	{
		var sizeX = (MaxCell.X - MinCell.X) + 1;
		var sizeZ = (MaxCell.Z - MinCell.Z) + 1;
		var resolution = Maze?.CELL_SIZE ?? 1;

		VisibilityMap = new float[sizeX * resolution, sizeZ * resolution];
	}
	
	private void InitializeChunk()
	{
		for (int x = MinCell.X; x <= MaxCell.X; x++)
		{
			for (int z = MinCell.Z; z <= MaxCell.Z; z++)
			{	
				var pos = new Vector3I(x, MinCell.Y, z);
				if(Maze.Visited.Contains(pos) || GetCellItem(pos) != InvalidCellItem)
					continue;
				if (x % 2 == 0 || z % 2 == 0)
				{	
					SetCellItem(pos, 0);
					Maze.Visited.Add(pos);
					// set neighboring walls to wildcard as well
					Statics.Directions3I
						.Select(d => pos + d).Where(neighbor => GetCellItem(neighbor) > 0).ToList()
						.ForEach(neighbor => SetCellItem(neighbor, 0));
				}
					
			}
		}
	}
	
	public bool InVisibilityMapBounds(Vector2I pos)
	{
		return pos.X >= 0 && pos.X < VisibilityMap.GetLength(0) && pos.Y >= 0 && pos.Y < VisibilityMap.GetLength(1);
	}
	
	public void OnBodyEntered(Node body)
	{
		if (body is Player)
		{	
			Events.PlayerEnteredChunk?.Invoke(this);
			GD.Print("Player entered chunk" + Name);
		}
			
	}

	
	
}
