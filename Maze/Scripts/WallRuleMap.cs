using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using Array = Godot.Collections.Array;

[Tool]
public partial class WallRuleMap : GridMap
{
	[Export] public RuleTableResource RuleTable = new RuleTableResource();
	[Export] public bool RefreshRules
	{
		get => false;
		set => RefreshRuleTable();
	}
	private readonly Vector3I[] _directions = new Vector3I[]
	{
		new Vector3I(0, 0, 1), 
		new Vector3I(1, 0, 0), 
		new Vector3I(0, 0, -1), 
		new Vector3I(-1, 0, 0)
	};
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RefreshRuleTable()
	{
		RuleTable = new RuleTableResource();
		var used = GetUsedCells();
		foreach (var cell in used)
		{
			foreach (Vector3I neighbor in GetNeighbors(cell))
			{
				CreateRule(cell, neighbor);
			}
		}
	}

	private void CreateRule(Vector3I cell, Vector3I neighbor)
	{
			var rule = new RuleResource()
			{	Id = GetCellItem(cell), 
				Orientation = GetCellItemOrientation(cell),
				Direction = neighbor - cell,
				NeighborId = GetCellItem(neighbor), 
				NeighborOrientation = GetCellItemOrientation(neighbor)
			};
			RuleTable.AddUnique(rule);
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

	private IEnumerable GetNeighbors(Vector3I cell)
	{
		foreach(var dir in _directions)
		{
			var neighbor = cell + dir;
				yield return neighbor;
		}
	}
}
