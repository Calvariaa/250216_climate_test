using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;

public partial class CellsBaseMultiNode : Node
{
	[Export] PackedScene cellScene;
	MultiMeshInstance3D cellPrefab;

	[Export] float CellSize = 0.1f;
	[Export] int Length = 100;
	[Export] double Alpha = 1e-4;
	CellsBaseMultiMeshInstance cellsMultiMesh;

	private TemperatureCalculator temperCalc;
	private SurfaceAreaCells cells;


	public override void _Ready()
	{
		cells = new SurfaceAreaCells(Length);
		cellsMultiMesh = new CellsBaseMultiMeshInstance();
		cellPrefab = cellScene.Instantiate<MultiMeshInstance3D>();

		temperCalc = new TemperatureCalculator(Length, Alpha, cells);

		// Create cells
		MultiMeshInstance3D cell = cellPrefab.Duplicate() as MultiMeshInstance3D;
		AddChild(cell);
		cell.Scale = new Vector3(CellSize, CellSize, CellSize);

		cell.Position = new Vector3(10, 20, 30);
		cell.RotationDegrees = new Vector3(0, 60, 0);

		cellsMultiMesh = cell as CellsBaseMultiMeshInstance;
	}

	public override void _Process(double delta)
	{


	}

}
