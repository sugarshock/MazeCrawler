using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Godot.Collections;
using Vector3I = Godot.Vector3I;

public partial class Maze : Node
{
	[Export] public int CELL_SIZE { get; set; } = 10;
	public List<Chunk> Chunks { get; set; } = new List<Chunk>();
	
	public HashSet<Vector3I> Visited { get; init; } = new HashSet<Vector3I>();
	private WfcProcessor _wfcProcessor;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_wfcProcessor = GetNode<WfcProcessor>("WfcProcessor");
		BuildChunk(new Vector3I(0,0,3));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public int GetCellItem(Vector3I cell)
	{
		var chunk = Chunks.FirstOrDefault(c => c.Contains(cell), null);
		return chunk?.GetCellItem(cell) ?? (int) GridMap.InvalidCellItem;
	}
	
	public bool SetCellItem(Vector3I cell, int value, Chunk chunk = null, int orientation = 0)
	{
		if(chunk == null)
			chunk = Chunks.FirstOrDefault(c => c.Contains(cell), null);
		if (chunk == null)
			return false;
		chunk.SetCellItem(cell, value, orientation);
		return true;
	}

	private void BuildChunk(Vector3I entrance)
	{	
		GD.Print("Building new chunk...");
		if (GetChunkAt(entrance) != null)
			throw new Exception("Trying to build already existing chunk");
		
		var bounds = GetNextChunkPos(entrance);
		var chunk = Statics.CHUNK_SCENE.Instantiate<Chunk>().Setup(this, bounds.Min, bounds.Max);
		Chunks.Add(chunk);
		AddChild(chunk);
		
		BuildFloor(chunk);
		GD.Print("Starting to dig");
		var possibleExits = Dig(entrance, chunk);
		SelectExits(chunk, 4, possibleExits);
		_wfcProcessor.ApplyTo(chunk, chunk.MinCell.Y);

		chunk.PlayerEntered += () => BuildSurroundingChunks(chunk);
	}
	
	private (Vector3I Min, Vector3I Max) GetNextChunkPos(Vector3I entrance)
	{
		// for now, make regular chunks of size 10 x 10
		(Vector3I Min, Vector3I Max) bounds;
		
		
		var chunkSize = 10;
		
		bounds.Min = new Vector3I();
		bounds.Min.X = (int)Math.Floor((double)entrance.X / chunkSize) * chunkSize;
		bounds.Min.Z = (int)Math.Floor((double)entrance.Z / chunkSize) * chunkSize;
		bounds.Max = bounds.Min + new Vector3I(chunkSize-1, 0, chunkSize-1);
		
		
		return bounds;
	}
	
	private PriorityQueue<(Vector3I entrance, Vector3I exit), int> Dig(
		Vector3I pos, 
		Chunk chunk, 
		int depth = 0, 
		PriorityQueue<(Vector3I entrance, Vector3I exit), int> possibleConnections = null)
	{
		if (possibleConnections == null)
			possibleConnections = new PriorityQueue<(Vector3I entrance, Vector3I exit), int>(new IntMaxComparer());
		
		Visited.Add(pos);
		SetCellItem(pos, -1);
		
		// 1 for rows that are wall, (2,0,2) for free cells
		var wallMod = new Vector3I( 1+Math.Abs(pos.X) % 2, 0 , 1+Math.Abs(pos.Z % 2));
		
		// find potential connections to outside at this cell and add to priority queue
		var exitCandidates = Statics.Directions3I
			.Select(dir => pos + (dir * wallMod))
			.Where(exit => !chunk.Contains(exit))
			.Select(exit => (pos, exit));
		
		possibleConnections.EnqueueRange(exitCandidates, depth);
		

		
		// directions where next odd cell within radius
		var possibleDirs = Statics.Directions3I
			.Where(d => chunk.Contains(pos + (d*wallMod)))
			.OrderBy(x => Guid.NewGuid());

		foreach (var dir in possibleDirs)
		{	
			if(Visited.Contains(pos + (dir * wallMod)))
				continue;
			if (pos + dir != pos + (dir * wallMod))
			{
				SetCellItem(pos + dir, -1);
				Visited.Add(pos + dir);
			}
			Dig(pos + (dir * wallMod), chunk, depth + 1, possibleConnections);
		}
		return possibleConnections;
	}

	private void SelectExits(Chunk chunk, int numberOfConnections, PriorityQueue<(Vector3I entrance, Vector3I exit), int> possibleExits)
	{
		// first get all connections imposed by surrounding chunks
		var imposedCons = GetSurroundingChunks(chunk).SelectMany(c => c.Connections)
			.Where(con => GetChunkAt(con.Value) == chunk)
			.Select(kv => (kv.Value, kv.Key));
	
		// get those that connect to a possible path in the current chunk
		var intersection = possibleExits.UnorderedItems.Where(item => imposedCons.Contains(item.Element)).ToList();
		// connect the paths
		foreach (var con in intersection)
		{
			var exitCell = con.Element.exit;
			chunk.SetCellItem(exitCell, -1);
			Visited.Add(exitCell);
		}
		
		// now take care of remaining exits to empty chunks
		var numRemaining = numberOfConnections - intersection.Count;
		while (numRemaining > 0 && possibleExits.Count > 0)
		{
			var deepest = possibleExits.Dequeue();
			
			if (intersection.Select(_ => _.Element).Contains(deepest))
				continue;
			if (chunk.Connections.ContainsKey(deepest.entrance))
				continue;

			var vec = (deepest.exit - deepest.entrance);
			if (vec.Length() > 1)
			{
				var cellBetween = deepest.entrance + (Vector3I)vec / 2;
				SetCellItem(cellBetween, -1);
				Visited.Add(cellBetween);
			}
				
				
			SetCellItem(deepest.entrance, -1);
			Visited.Add(deepest.entrance);
			chunk.Connections.Add(deepest.entrance, deepest.exit);
			numRemaining--;
		}
	}
	
	private void BuildFloor(Chunk chunk)
	{
		for (int x = chunk.MinCell.X; x <= chunk.MaxCell.X; x++)
		{
			for (int z = chunk.MinCell.Z; z <= chunk.MaxCell.Z; z++)
			{
				SetCellItem(new Vector3I(x, -1, z), 6, chunk);
			}
		}
	}
	
	private void BuildSurroundingChunks(Chunk chunk)
	{
		foreach (var dir in Statics.Directions3I)
		{
			var cons = chunk.Connections.Where(kv => (kv.Key + dir == kv.Value) || (kv.Key + (2 * dir) == kv.Value))
				.ToList();
			if (cons.Count > 0)
			{
				var entry = cons.First().Value;
				if (GetChunkAt(entry) == null)
					BuildChunk(entry);
			}
			else
			{
				var midCell = chunk.LocalToMap(chunk.Aabb.GetCenter());
				if (GetChunkAt(midCell + (6 * dir)) == null)
					BuildChunk(midCell + (6 * dir));
			}
		}
	}

	public List<Chunk> GetSurroundingChunks(Chunk chunk)
	{
		var list = new List<Chunk>()
		{
			GetChunkAt(chunk.MinCell + new Vector3I(-1, 0, 0)),
			GetChunkAt(chunk.MinCell + new Vector3I(0, 0, -1)),
			GetChunkAt(chunk.MaxCell + new Vector3I(1, 0, 0)),
			GetChunkAt(chunk.MaxCell + new Vector3I(0, 0, 1))
		};
		list.RemoveAll(chunk => chunk == null);
		return list;
	}
	
	public Vector3 MapToLocal(Vector3I cell)
	{
		return Chunks.First().MapToLocal(cell);
	}
	
	public Vector3I LocalToMap(Vector3 position)
	{
		return Chunks.First().LocalToMap(position);
	}
	
	public Chunk GetChunkAt(Vector3I cell)
	{
		return Chunks.FirstOrDefault(c => c.Contains(cell), null);
	}
	
	public Chunk GetChunkAt(Vector3 position)
	{	
		if(Chunks.Count == 0)
			return null;
		return GetChunkAt(LocalToMap(position));
	}
	
	/*private bool WithinRadius(Vector2I pos, Vector2I center, int radius)
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
	

	
	public Mesh[] GetGrassMeshes()
	{
		
		throw new NotImplementedException();
		
		foreach (var variant in GetMeshes())
		{
			GD.Print(variant);
		}

		return new Mesh[] { };
	}*/

}

public class IntMaxComparer : IComparer<int>
{
	public int Compare(int x, int y) => y.CompareTo(x);
}
