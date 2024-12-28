# Maze Crawler

A dungeon crawler walking simulator set within a procedurally generated maze, meant as a playground for various algorithmic world-building techniques.

Developed in Godot 4.0 (Mono/C#).
All assets self-made or synthesized using Microsoft Designer.

https://github.com/user-attachments/assets/c204dc1d-ca4d-4a21-9e75-c4788dc36206

## Maze Generation
The maze is procedurally generated in chunks, with each chunk being created through Recursive Backtracking with randomized depth-first search (DFS).
Each chunk has clearly defined exits that function as the adjacent chunk's entrance.
Walls are initially created as simple uniform blocks on a Godot GridMap.

https://github.com/user-attachments/assets/c6be44c1-5cab-42b7-871e-d00d342efa1f

## Wave Function Collapse
The second step of the pipeline deploys the Wave Function Collapse algorithm to replace the wall blocks with context-sensitive 3D wall models.
The WFC rule table is inefficiently generated from a manually curated reference scene.
For further explanation of the WFC algorithm, please refer to ![this guide](https://robertheaton.com/2018/12/17/wavefunction-collapse-algorithm/).

https://github.com/user-attachments/assets/fcdf8c55-eb81-4c89-8f01-0a687f8a7505

## Fog of War Shader
The fog of war effect relies on a dynamic, discrete-resolution visibility map for each chunk. The visibility maps are stitched together and passed to a shader plane in front of the camera as a texture.
Through some matrix math magic, we map the visibility map coordinates to viewport pixel positions. This enables the shader to make already uncovered pixels transparent and replace covered pixels with a procedurally animated fog texture.
