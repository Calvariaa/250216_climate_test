using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;

public partial class Node3d : Node
{
	[Export] PackedScene cellScene;
	MeshInstance3D cellPrefab;

	[Export] float CellSize = 0.1f;
	[Export] int Length = 50;
	[Export] double Alpha = 1e-4;
	MeshInstance3d[,,] cellsMesh;

	private TemperatureCalculator temperCalc;
	private SpaceCells cells;

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
		temperCalc = new TemperatureCalculator(Length, Alpha, cells);

		cellsMesh = new MeshInstance3d[Length, Length, Length];
		cellPrefab = cellScene.Instantiate<MeshInstance3D>();

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
						cells.Cell(x, y, 0).Temperature = temperature;
					}
				}
			}
		}


		switch (_mapType)
		{
			case MapType.temperCalcDistribution:
				for (int x = 0; x < temperCalc.Length; x++)
				{
					for (int y = 0; y < temperCalc.Length; y++)
					{
						cellsMesh[0, x, y].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
						cellsMesh[x, 0, y].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
						cellsMesh[x, y, 0].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
						cellsMesh[Length - 1, x, y].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
						cellsMesh[x, Length - 1, y].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
						cellsMesh[x, y, Length - 1].Temperature = (float)temperCalc.Cells.Cell(x, y, 0).Temperature;
					}
				}

				break;
				// case MapType.temperCalcChangeDistribution:
				//     for (int x = 0; x < temperCalc.Length; x++)
				//     {
				//         for (int y = 0; y < temperCalc.Length; y++)
				//         {
				//             cellsMesh[x, 0, y].Temperature = (float)temperCalc.CellsDerivative[x, y];
				//         }
				//     }

				//     break;
				// case MapType.temperCalcAnomalyDistribution:
				//     for (int x = 0; x < temperCalc.Length; x++)
				//     {
				//         for (int y = 0; y < temperCalc.Length; y++)
				//         {
				//             cells[x, 0, y].temperCalc = (float)temperCalc.CellsAnomaly[x, y];
				//         }
				//     }

				//     break;
		}

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
