using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;

public partial class CellsBaseNode : Node
{
	[Export] Node3D node3D;
	[Export] PackedScene cellScene;
	MeshInstance3D cellPrefab;

	[Export] float CellSize = 0.1f;
	[Export] uint Length = 64;
	[Export] float Alpha = 1e-4F;
	CellsBaseMeshInstance[,,] cellsMesh;

	private TemperatureCalculator temperCalc;
	private SurfaceAreaCells cells;
	StandardMaterial3D material3D;

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
		cellsMesh = new CellsBaseMeshInstance[Length, Length, Enum.GetValues(typeof(AreaOrientation)).Length];
		cellPrefab = cellScene.Instantiate<MeshInstance3D>();

		temperCalc = new TemperatureCalculator(Length, Alpha, cells);

		// Create cells

		foreach (var orintation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					MeshInstance3D cell = cellPrefab.Duplicate() as MeshInstance3D;
					AddChild(cell);
					cell.Scale = new Vector3(CellSize, CellSize, CellSize);

					switch (orintation)
					{
						case AreaOrientation.Up:
							cell.Position = new Vector3(
								(i - (Length - 1) / 2.0f) * CellSize,
								(Length / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(0, 0, 0);
							break;
						case AreaOrientation.Down:
							cell.Position = new Vector3(
								(i - (Length - 1) / 2.0f) * CellSize,
								(-Length / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(180, 0, 0);
							break;
						case AreaOrientation.Left:
							cell.Position = new Vector3(
								(-Length / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize,
								(i - (Length - 1) / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(0, 0, 90);
							break;
						case AreaOrientation.Right:
							cell.Position = new Vector3(
								(Length / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize,
								(i - (Length - 1) / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(0, 0, -90);
							break;
						case AreaOrientation.Forward:
							cell.Position = new Vector3(
								(i - (Length - 1) / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize,
								(Length / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(90, 0, 0);
							break;
						case AreaOrientation.Backward:
							cell.Position = new Vector3(
								(i - (Length - 1) / 2.0f) * CellSize,
								(j - (Length - 1) / 2.0f) * CellSize,
								(-Length / 2.0f) * CellSize
							);
							cell.RotationDegrees = new Vector3(-90, 0, 0);
							break;
					}

					cellsMesh[i, j, (int)orintation] = cell as CellsBaseMeshInstance;
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		temperCalc.Calculate(delta);

		// 随机生成温度
		foreach (AreaOrientation AreaOrientation in Enum.GetValues(typeof(AreaOrientation)))
		{
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
							cells.surfaceCellNodes[AreaOrientation].Surface.Cell(x, y, 0).Temperature = temperature;
						}
					}
				}
			}
		}

		// Test Method
		// for (int i = 0; i < temperCalc.Length; i++)
		// {
		// 	cells.surfaceCellNodes[AreaOrientation.Up].Surface.Cell(0, i, 0).Temperature = -120;
		// }

		// for (int i = 0; i < temperCalc.Length; i++)
		// {
		// 	cells.surfaceCellNodes[AreaOrientation.Up].Surface.Cell(i, 0, 0).Temperature = 120;
		// }


		foreach (AreaOrientation orintation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < temperCalc.Length; i++)
			{
				for (int j = 0; j < temperCalc.Length; j++)
				{
					cellsMesh[i, j, (int)orintation].Temperature =
						(float)cells.surfaceCellNodes[orintation].Surface.Cell(i, j, 0).Temperature;
				}
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
