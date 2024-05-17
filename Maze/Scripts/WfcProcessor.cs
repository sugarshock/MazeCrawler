using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public partial class WfcProcessor : Node3D
{
	
	[Export] public RuleTableResource RuleTable;
	private static Vector3I[] _directions = new Vector3I[] { new Vector3I(0, 0,1), new Vector3I(1, 0, 0), new Vector3I(0, 0, -1), new Vector3I(-1, 0, 0) };
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void ApplyTo(Chunk chunk, int yLayer)
	{
		Task.Run(async () =>
		{
			var possibilities = InitializePossibilities(chunk, yLayer);
			int patience = 200;
			while (possibilities is { Count: > 0 } && patience-- > 0)
				Collapse(chunk.Maze, possibilities);
		});
	}
	
	private void Collapse(Maze maze, Dictionary<Vector3I, HashSet<Possibility>> possibilities)
	{
		// find a cell with the smallest number of possibilities
		var cell = possibilities.OrderBy(x => x.Value.Count).First().Key;
		GD.Print(possibilities[cell].Count);
		
		// if its possibilities are empty, remove and return
		if (possibilities[cell].Count == 0)
		{
			possibilities.Remove(cell);
			return;
		}
		
		// choose a random possibility
		var fixedConf = possibilities[cell].OrderBy(x => Guid.NewGuid()).First();
		// collapse the cell
		possibilities.Remove(cell);
		
		// set the cell item
		maze.SetCellItem(cell,fixedConf.Id, orientation: fixedConf.Orientation);
		
		// update possibilities of all neighbours
		foreach (var dir in _directions)
		{
			var neighbour = cell + dir;
			if (possibilities.ContainsKey(neighbour))
			{
				List<RuleResource> newAllowForNeighbour = RuleTable.RuleTable.Where(x => x.NeighborId == fixedConf.Id && x.NeighborOrientation == fixedConf.Orientation && x.Direction == -dir).ToList();
				// remove all possibility from neighbour that are not in newAllowForNeighbour
				
				possibilities[neighbour].IntersectWith(newAllowForNeighbour.Select(x => new Possibility(){Id = x.Id, Orientation = x.Orientation}));
			}
		}
	}

	private Dictionary<Vector3I, HashSet<Possibility>> InitializePossibilities(Chunk chunk, int yLayer)
	{
		var maze = chunk.Maze;
		var usedCells = chunk.GetUsedCells()
			.Where(cell => cell.Y == yLayer)
			.Where(cell => chunk.GetCellItem(cell) == 0);
		
		var possibilities = new Dictionary<Vector3I, HashSet<Possibility>>();

		// Initialize possibilities for each cell based on wildcard placement (block 0)
		foreach (var cell in usedCells)
		{
			// for each neighbour
			foreach (var dir in _directions)
			{	
				var neighbour = cell + dir;
				
				HashSet<Possibility> cValidConfigs = new HashSet<Possibility>();
				
				// if neighbour is air
				if(maze.GetCellItem(neighbour) == GridMap.InvalidCellItem)
					RuleTable.RuleTable.Where(x => x.NeighborId == GridMap.InvalidCellItem && x.Direction == dir)
										.ToList()
										.ForEach(rule => cValidConfigs.Add(new Possibility(){Id = rule.Id, Orientation = rule.Orientation}));
				
				// if neighbour is a block
				else
				{
					// for each possible id for neighbour cell
					foreach (var nId in RuleTable.GetIds())
					{
						RuleTable.RuleTable.Where(x => x.NeighborId == nId && x.Direction == dir)
							.ToList()
							.ForEach(rule => cValidConfigs.Add(new Possibility(){Id = rule.Id, Orientation = rule.Orientation}));
					}
				}

				
				// if this is the first time touching this cell, add all the validConfigs to the possible
				if(!possibilities.ContainsKey(cell))
					possibilities[cell] = cValidConfigs;
				// else, intersect the validConfigs with the current possibilities
				else
					possibilities[cell].IntersectWith(cValidConfigs);
			}
		}

		return possibilities;
	}
	
	public static IEnumerable<RuleResource> Rotations(RuleResource rule)
	{
		var rotations = new List<RuleResource>();
		for (int i = 0; i < 4; i++)
		{
			yield return Rotate(rule, i);
		}
	}

	private static RuleResource Rotate(RuleResource rule, int rotationCount)
	{
		rule.Direction = Rotate(rule.Direction, rotationCount);
		rule.Orientation = Rotate(rule.Orientation, rotationCount);
		rule.NeighborOrientation = rule.NeighborId == (int) GridMap.InvalidCellItem ? rule.NeighborOrientation : Rotate(rule.NeighborOrientation, rotationCount);
		return rule;
	}
	
	private static Vector3I Rotate(Vector3I direction, int rotationCount)
	{
		for (int i = 0; i < rotationCount; i++)
		{
			direction = new Vector3I(-direction.Z, direction.Y, direction.X);
		}
		return direction;
	}
	
	public static int Rotate(int orientation, int rotationCount)
	{	
		// for rotationCount times
		for (int i = 0; i < rotationCount; i++)
		{
			orientation = orientation switch
			{
				0 => 16,
				16 => 10,
				10 => 22,
				22 => 0,
				_ => orientation
			};
		}	
		return orientation;
	}

	public record Possibility
	{
		public int Id;
		public int Orientation;
	}
}
