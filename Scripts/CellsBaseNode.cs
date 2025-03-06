using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;

public partial class CellsBaseNode : Node
{
	[Export] PackedScene cellScene;
	MeshInstance3D cellPrefab;

	[Export] float CellSize = 0.1f;
	[Export] int Length = 50;
	[Export] double Alpha = 1e-4;
	MeshInstance3d[,,] cellsMesh;

	private TemperatureCalculator temperCalc;
	private SurfaceAreaCells cells;

	// 温度分布图，变温分布图，气温距平分布图
	private enum MapType
	{
		temperCalcDistribution,
		temperCalcChangeDistribution,
		temperCalcAnomalyDistribution,
		temperCalcNum
	}

	private MapType _mapType = MapType.temperCalcDistribution;



	public override void _Ready()
	{

		cells = new SurfaceAreaCells(Length);
		cellsMesh = new MeshInstance3d[Length, Length, Length];
		cellPrefab = cellScene.Instantiate<MeshInstance3D>();

		temperCalc = new TemperatureCalculator(Length, Alpha, cells);

		// Create cells
		for (int i = 0; i < Length; i++)
		{
			for (int j = 0; j < Length; j++)  // Width
			{
				for (int k = 0; k < Length; k++)  // Height
				{
					if ((i == 0 || i == Length - 1) || (j == 0 || j == Length - 1) || (k == 0 || k == Length - 1))
					{
						MeshInstance3D cell = cellPrefab.Duplicate() as MeshInstance3D;
						AddChild(cell);
						cell.Scale = new Vector3(CellSize, CellSize, CellSize);
						cell.Position = new Vector3(
							(i - Length / 2) * CellSize,
							(j - Length / 2) * CellSize,
							(k - Length / 2) * CellSize
						);
						cellsMesh[i, j, k] = cell as MeshInstance3d;
					}
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		temperCalc.Calculate(delta);

		// 随机生成温度
		if (Randf() < 0.1)
		{
			// cellCalculator.cells[RandRange(0, cellCalculator.width - 1), RandRange(0, cellCalculator.height - 1)]
			//     .temperCalc = RandRange(-10, 255);
			var radius = RandRange(1, 10);
			var width = RandRange(radius, temperCalc.Length - radius);
			var height = RandRange(radius, temperCalc.Length - radius);
			var temperature = RandRange(-100, 100);

			for (var x = 0; x < temperCalc.Length; x++)
			{
				for (var y = 0; y < temperCalc.Length; y++)
				{
					if (Mathf.Pow(x - width, 2) + Mathf.Pow(y - height, 2) < radius)
					{
						cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Up].Surface.Cell(x, y, 0).Temperature = temperature;
					}
				}
			}
		}

		// Test Method
		// for (int i = 0; i < temperCalc.Length; i++)
		// {
		// 	cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Up].Surface.Cell(0, i, 0).Temperature = -120;
		// }

		// for (int i = 0; i < temperCalc.Length; i++)
		// {
		// 	cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Up].Surface.Cell(i, 0, 0).Temperature = 120;
		// }


		for (int x = 0; x < temperCalc.Length; x++)
		{
			for (int y = 0; y < temperCalc.Length; y++)
			{
				// Left
				cellsMesh[0, x, y].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Left].Surface.Cell(x, y, 0).Temperature;
				// Down
				cellsMesh[x, 0, y].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Down].Surface.Cell(x, y, 0).Temperature;
				// Backward
				cellsMesh[x, y, 0].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Backward].Surface.Cell(x, y, 0).Temperature;
				// Right
				cellsMesh[Length - 1, x, y].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Right].Surface.Cell(x, y, 0).Temperature;
				// Up
				cellsMesh[x, Length - 1, y].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Up].Surface.Cell(x, y, 0).Temperature;
				// Forward
				cellsMesh[x, y, Length - 1].Temperature =
					(float)cells.surfaceCellNodes[SurfaceAreaCells.Orientation.Forward].Surface.Cell(x, y, 0).Temperature;
			}
		}

		// switch (_mapType)
		// {
		// 	case MapType.temperCalcDistribution:

		// 		break;
		// 		// case MapType.temperCalcChangeDistribution:
		// 		//     for (int x = 0; x < temperCalc.Length; x++)
		// 		//     {
		// 		//         for (int y = 0; y < temperCalc.Length; y++)
		// 		//         {
		// 		//             cellsMesh[x, 0, y].Temperature = (float)cellsDerivative[x, y];
		// 		//         }
		// 		//     }

		// 		//     break;
		// 		// case MapType.temperCalcAnomalyDistribution:
		// 		//     for (int x = 0; x < temperCalc.Length; x++)
		// 		//     {
		// 		//         for (int y = 0; y < temperCalc.Length; y++)
		// 		//         {
		// 		//             cells[x, 0, y].temperCalc = (float)cellsAnomaly[x, y];
		// 		//         }
		// 		//     }

		// 		//     break;
		// }

	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.R)
			{
				temperCalc.ClearCells();
			}

			if (keyEvent.Keycode == Key.T)
			{
				Print(_mapType);
				if (_mapType < MapType.temperCalcNum - 1)
				{
					_mapType++;
				}
				else
				{
					_mapType = 0;
				}
			}
		}
	}
}
