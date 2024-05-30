using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CollectableProcessor : Node3D
{

	[Export] public Godot.Collections.Array<Memory> MemoriesToSpawn { get; private set; } = new Godot.Collections.Array<Memory>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void ApplyTo(Chunk chunk, int nMemories, int nSouls, int yLayer)
	{
		GD.Print("Spawning collectables...");
		var emptyCells =
			chunk.GetUsedCellsByItem((int)6)
				.Select(cell => cell with { Y = (cell.Y + 1) })
				.Where(cell => chunk.GetCellItem(cell) == (int)GridMap.InvalidCellItem)
				.ToList();
		
		// first take care of memories from previously spawned souls
		while(nMemories > 0 && emptyCells.Count > 0 && MemoriesToSpawn.Count > 0)
		{
			var cell = emptyCells.MinBy(c => Guid.NewGuid());
			emptyCells.Remove(cell);
			var memInstance = Statics.MEMORY_SCENE.Instantiate<MemoryInstance>();
			memInstance.Memory = MemoriesToSpawn.OrderBy(m => Guid.NewGuid()).First();
			MemoriesToSpawn.Remove(memInstance.Memory);
			memInstance.Position = chunk.MapToLocal(cell)  - new Vector3(0, chunk.Maze.CELL_SIZE / 2f, 0);;
			chunk.AddChild(memInstance);
			nMemories--;
		}
	
		// now spawn new souls
		while(nSouls > 0 && emptyCells.Count > 0)
		{	
			// select random soul from directory
			var soul = (Soul) Statics.EXAMPLE_SOUL.Duplicate();
			// select corresponding memori(es)
			var memory = (Memory) Statics.EXAMPLE_MEMORY.Duplicate();

			var cell = emptyCells.MinBy(c => Guid.NewGuid());
			emptyCells.Remove(cell);
			var soulInstance = Statics.SOUL_SCENE.Instantiate<SoulInstance>();
			soulInstance.Soul = soul;
			soulInstance.Position = chunk.MapToLocal(cell) - new Vector3(0, chunk.Maze.CELL_SIZE / 2f, 0);
			chunk.AddChild(soulInstance);
			nSouls--;
		
			MemoriesToSpawn.Add(memory);
		}

	}
}
