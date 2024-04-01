using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using Godot.Collections;

public partial class Maze : GridMap
{
	[Export] public float CELL_SIZE {get => Scale.X; set => Scale = new Vector3(value, value, value);}
	public List<Chunk> Chunks { get; set; } = new List<Chunk>();
	
	private static PackedScene TRIGGER_PACK = ResourceLoader.Load<PackedScene>("res://Maze/Scenes/TriggerArea.tscn");
	private static Vector2I[] _directions = new Vector2I[] { new Vector2I(0, 1), new Vector2I(1, 0), new Vector2I(0, -1), new Vector2I(-1, 0) };
	
	private HashSet<Vector2I> _visited = new HashSet<Vector2I>();
	private WfcProcessor _wfcProcessor;
	private GrassMultiMesh _grassMultiMesh;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_visited = new HashSet<Vector2I>();
		BuildChunk(new Vector2I(3,3), 7);
		_grassMultiMesh = GetNode<GrassMultiMesh>("GrassMultiMesh");
		_wfcProcessor = GetNode<WfcProcessor>("WfcProcessor");
		_wfcProcessor.InitializePossibilities(this, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void BuildChunk( Vector2I entrance, int radius)
	{	
		if(radius % 2 == 0)
			throw new Exception("Radius must be odd");
		if(GetCellItem(entrance) == 0)
			throw new Exception("Entrance must not be a wall");
		
		BuildFloor(entrance, radius * 10);
		//_grassMultiMesh.PopulateMeshes(GetMeshes());
		var chunk = InitializeChunk(entrance, radius);
		VisitCell(entrance, chunk, 1);
		CreateExits(chunk, 1);
		
	}
	
	
	private void VisitCell(Vector2I pos, Chunk chunk, int depth)
	{
		_visited.Add(pos);
		SetCellItem(pos, -1);
		
		var wallMod = new Vector2I( 1+Math.Abs(pos.X) % 2, 1+Math.Abs(pos.Y % 2));
		
		// find potential exits at this cell and add to priority queue
		var possibleExits = _directions.Where(d => !chunk.Contains(pos + (d * wallMod)));
		chunk.PossibleExits.EnqueueRange(possibleExits, depth);
		
		// directions where next odd cell within radius
		var possibleDirs = _directions
			.Where(d => chunk.Contains(pos + (d*wallMod)))
			.OrderBy(x => Guid.NewGuid());

		foreach (var dir in possibleDirs)
		{	
			if(_visited.Contains(pos + (dir * wallMod)))
				continue;
			if (pos + dir != pos + (dir * wallMod))
			{
				SetCellItem(pos + dir, -1);
				_visited.Add(pos + dir);
			}
			VisitCell(pos + (dir * wallMod), chunk, depth + 1);
		}

	}

	private void CreateExits(Chunk chunk, int numberOfExits)
	{
		foreach (int i in Enumerable.Range(0, Math.Min(numberOfExits, chunk.PossibleExits.Count)))
		{
			var deepest = chunk.PossibleExits.Dequeue();
			SetCellItem(deepest, -1);
			_visited.Add(deepest);

			
			var trigger = TRIGGER_PACK.Instantiate<Area3D>();
			trigger.Position = MapToLocal(new Vector3I(chunk.Entrance.X, 1, chunk.Entrance.Y));
			trigger.BodyEntered += (body) =>
			{
				if (body is Player)
				{
					BuildChunk(deepest, 7);
					_wfcProcessor.InitializePossibilities(this, 1);
					trigger.Position = trigger.Position with { Y = 100 };
				}

			};
			AddChild(trigger);
		}
	}

	// Initialize a chunk of the maze around the center
	private Chunk InitializeChunk(Vector2I entrance, int radius )
	{
		var chunk = new Chunk()
		{
			Entrance = entrance,
			Radius = radius,
			VisibilityMap = new float[(int) (radius * 2f * CELL_SIZE), (int) (radius * 2f * CELL_SIZE)]
		};
		Chunks.Add(chunk);
		
		for (int x = chunk.MinCell.X; x <= chunk.MaxCell.X; x++)
		{
			for (int y = chunk.MinCell.Y; y <= chunk.MaxCell.Y; y++)
			{	
				var pos = new Vector2I(x, y);
				if(_visited.Contains(pos) || GetCellItem(pos) != InvalidCellItem)
					continue;
				if (x % 2 == 0 || y % 2 == 0)
				{	
					SetCellItem(pos, 0);
					// set neighboring walls to wildcard as well
					_directions.Select(d => pos + d).Where(neighbor => GetCellItem(neighbor) > 0).ToList().ForEach(neighbor => SetCellItem(neighbor, 0));
				}
					
			}
		}

		return chunk;
	}
	
	private void BuildFloor(Vector2I pos, int radius)
	{
		for (int x = pos.X - radius; x <= pos.X + radius; x++)
		{
			for (int y = pos.Y - radius; y <= pos.Y + radius; y++)
			{
				SetCellItem(new Vector3I(x, 0, y), 6);
			}
		}
	}
	
	private void SetCellItem(Vector2I pos, int item)
	{
		SetCellItem(new Vector3I(pos.X, 1, pos.Y), item);
	}
	
	private int GetCellItem(Vector2I pos)
	{
		return GetCellItem(new Vector3I(pos.X, 1, pos.Y));
	}
	
	private bool WithinRadius(Vector2I pos, Vector2I center, int radius)
	{
		return Math.Abs(pos.X - center.X) <= radius && Math.Abs(pos.Y - center.Y) <= radius;
	}

	public List<Chunk> GetChunksAt(Vector3 worldPos)
	{
		var cell = LocalToMap(ToLocal(worldPos));

		var chunks = Chunks.Where(c =>
			cell.X >= c.MinCell.X && cell.X <= c.MaxCell.X && cell.Z >= c.MinCell.Y && cell.Z <= c.MaxCell.Y);
		
		return chunks.ToList();
	}
	
	public Vector3 MapToGlobal(Vector2I cell)
	{
		return ToGlobal(MapToLocal(new Vector3I(cell.X, 1, cell.Y)));
	}
	
	public Mesh[] GetGrassMeshes()
	{
		
		throw new NotImplementedException();
		
		foreach (var variant in GetMeshes())
		{
			GD.Print(variant);
		}

		return new Mesh[] { };
	}

}

public record Chunk
{
	public Vector2I Entrance;
	public int Radius;
	public List<Vector2I> Exits = new List<Vector2I>();
	public PriorityQueue<Vector2I, int> PossibleExits = new PriorityQueue<Vector2I, int>(new IntMaxComparer());
	public Vector2I MinCell => Entrance - new Vector2I(Radius, Radius);
	public Vector2I MaxCell => Entrance + new Vector2I(Radius, Radius);

	public float[,] VisibilityMap;

	public bool Contains(Vector2I pos)
	{
		return Math.Abs(pos.X - Entrance.X) <= Radius && Math.Abs(pos.Y - Entrance.Y) <= Radius;
	}

	public bool InVisibilityMapBounds(Vector2I pos)
	{
		return pos.X >= 0 && pos.X < VisibilityMap.GetLength(0) && pos.Y >= 0 && pos.Y < VisibilityMap.GetLength(1);
	}
}

public class IntMaxComparer : IComparer<int>
{
	public int Compare(int x, int y) => y.CompareTo(x);
}
