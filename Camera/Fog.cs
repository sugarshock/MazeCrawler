using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Fog : MeshInstance3D
{
	[Export] public float PlayerFov = 10;
	[Export] public float PlayerFovCore = 5;
	private ShaderMaterial _material;
	private Maze _maze;
	//public Dictionary<Vector3I, float> UncoveredCells = new Dictionary<Vector3I, float>();
	
	[Export(PropertyHint.ResourceType, "ImageTexture")] ImageTexture TestTexture { get; set; }
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_material = MaterialOverride as ShaderMaterial;
		_maze = GetTree().Root.GetNode<Maze>("/root/Maze");
		Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void UncoverGauss(Vector3 worldPos)
	{	
		var cell = _maze.LocalToMap(_maze.ToLocal(worldPos));
		var chunks = _maze.GetChunksAt(worldPos);
		
		foreach (var chunk in chunks)
			ApplyGaussOn(chunk, worldPos);
		
		UpdateTexture();
		
	}

	private void ApplyGaussOn(Chunk chunk, Vector3 worldPos)
	{
		// this is the world position that corresponds to texel (0,0) in the texture
		var worldOffset = GetChunkOffset(chunk);
		
		// this is the texel that corresponds to the current world position
		var texPos = worldPos - worldOffset;
		
		foreach(var x in Enumerable.Range((int)-PlayerFov, 2 * (int)PlayerFov))
		{
			foreach(var y in Enumerable.Range((int)-PlayerFov, 2 * (int)PlayerFov))
			{	
				var dist = Mathf.Sqrt(x * x + y * y);

				if (dist <= PlayerFovCore) // if cell is within core FoV radius
				{
					var pixel = new Vector2I((int) texPos.X + x, (int) texPos.Z + y);
					if(!chunk.InVisibilityMapBounds(pixel))
						continue;
					chunk.VisibilityMap[pixel.X, pixel.Y] = 1.0f;
				}
				
				else if (dist <= PlayerFov) // Check if the cell is within peripherie fov
				{
					var pixel = new Vector2I((int) texPos.X + x, (int) texPos.Z + y);
					if(!chunk.InVisibilityMapBounds(pixel))
						continue;

					dist -= PlayerFovCore;
					
					var value = Math.Max(
						Math.Clamp(1.0f / dist, 0f, 1f), 
						chunk.VisibilityMap[pixel.X, pixel.Y]
						);
					chunk.VisibilityMap[pixel.X, pixel.Y] = value;
				}
			}
		}
	}

	public void UpdateTexture()
	{
		var chunks = _maze.Chunks;
		
		// get pos and dimensions of the texture in pixels
		var textureOffset = CellToTexel(new Vector2I(chunks.Min(c => c.MinCell.X), chunks.Min(c => c.MinCell.Y)), -0.5f);
		var imgSize = CellToTexel(new Vector2I(chunks.Max(c => c.MaxCell.X), chunks.Max(c => c.MaxCell.Y)), +0.5f) -
		              textureOffset;

		var img = Image.Create((int) imgSize.X, (int) imgSize.Y, false, Image.Format.Rgb8);
		
		foreach (var chunk in chunks)
		{	
			// position of the chunk in the texture in pixels
			var chunkOffset = CellToTexel(chunk.MinCell) - textureOffset;

			for(var x = 0; x < chunk.VisibilityMap.GetLength(0); x++)
			{
				for(var y = 0; y < chunk.VisibilityMap.GetLength(1); y++)
				{
					var pixel = chunk.VisibilityMap[x, y];
					var pixPos = new Vector2I((int) chunkOffset.X + x, (int) chunkOffset.Y + y);
					img.SetPixel(pixPos.X, pixPos.Y, Color.FromHsv(0f, 0f, pixel));
				}
			}
		}
		
		var imageTexture = ImageTexture.CreateFromImage(img);

		
		_material.SetShaderParameter("offset", textureOffset);
		_material.SetShaderParameter("uncoveredWorldPositions", imageTexture);
		_material.SetShaderParameter("camera_transform", GetViewport().GetCamera3D().GlobalTransform);
		
		//GD.Print("Offset:" + _material.GetShaderParameter("offset"));
		//GD.Print("Uncovered area size:" + _material.GetShaderParameter("uncoveredWorldPositions").As<Texture2D>().GetSize());
		ResourceSaver.Save(imageTexture, "res://uncovered.png");


	}
	private void CarvePixelBlock(Image img, Vector2I pos, int radius, float value)
	{
		foreach (var x in Enumerable.Range(-radius, 2 * radius))
		{
			foreach (var y in Enumerable.Range(-radius, 2 * radius))
			{
				if(pos.X+x < 0 || pos.X+x >= img.GetWidth() || pos.Y+y < 0 || pos.Y+y >= img.GetHeight())
					continue;
				//GD.Print("Setting pixel at: " + (pos.X + x) + ", " + (pos.Y + y) + " to value: " + value);
				img.SetPixel(pos.X + x, pos.Y + y, Color.FromHsv(0f,0f, value));
			}
		}
	}

	private Vector3 GetChunkOffset(Chunk chunk)
	{	
		var cell = new Vector3I(chunk.MinCell.X, 1, chunk.MinCell.Y);
		return _maze.ToGlobal(_maze.MapToLocal(cell));
	}

	private Vector2 CellTo2DWorld(Vector3I cell)
	{
			var local = _maze.MapToLocal(cell);
			var global = _maze.ToGlobal(local);
			return new Vector2(global.X, global.Z);
	}
	
	private Vector2I CellToTexel(Vector2I cell, float cornerFactor = 0f)
	{
		var global = _maze.MapToGlobal(cell) + (cornerFactor * Vector3.One * _maze.CELL_SIZE);
		return new Vector2I((int) global.X, (int) global.Z);
	}
}
