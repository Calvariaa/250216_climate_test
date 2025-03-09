using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;
public partial class CellsMappingMeshInstance3d : MeshInstance3D
{
	
	[Export] private string ComputePath;
	[Export] float CellSize = 0.1f;
	[Export] uint Length = 128;
	[Export] float Alpha = 1e-4F;

	private TemperatureCalculator temperCalc;
	private TemperatureComputeCalculator temperComputeCalc;
	private SurfaceAreaCells cells;

	enum CellsType
	{
		CellsCube,
		CellsFlat,
		CellsTypeNum
	}
	CellsType _cellsType = CellsType.CellsCube;

	public override void _Ready()
	{
		cells = new SurfaceAreaCells(Length);

		// temperCalc = new TemperatureCalculator(Length, Alpha, cells);
		temperComputeCalc = new TemperatureComputeCalculator(ComputePath, Length, Alpha, cells);
	}

	public override void _Process(double delta)
	{
		// temperCalc.Calculate(delta);
		temperComputeCalc.Calculate(delta);

		// 随机生成温度
		if (Randf() < 0.1)
		{
			var radius = RandRange(1, 100);
			var orientation = RandRange(0, 5);
			var width = RandRange(radius, Length - radius);
			var height = RandRange(radius, Length - radius);
			var temperature = RandRange(-100, 100);

			for (var i = 0; i < Length; i++)
			{
				for (var j = 0; j < Length; j++)
				{
					if (Mathf.Pow(i - width, 2) + Mathf.Pow(j - height, 2) < radius)
					{
						cells.surfaceCellNodes[(AreaOrientation)orientation].Surface.Cell(i, j, 0).Temperature = temperature;
					}
				}
			}
		}

		// CellsMesh.SetInstanceColor((int)orientation * Length * Length + i * Length + j, Colors.Pink);
		foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					// if (i == 0) cells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature = -120;
					// 所以x==0是左，y==0是上
					// CellsMesh.SetInstanceColor((int)orientation * (int)Length * (int)Length + i * (int)Length + j, CalculateTemperatureColor((float)cells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature));
				}
			}
		}
	}


	private Color CalculateTemperatureColor(float temperature)
	{
		float clampedTemp = Mathf.Clamp(temperature, -120, 120);
		float hue;

		if (clampedTemp > 0)
		{
			hue = (65.0f - clampedTemp * 13 / 24.0f) / 360.0f;
		}
		else
		{
			hue = (65.0f - clampedTemp * 47 / 24.0f) / 360.0f;
		}

		return Color.FromHsv(hue, 0.64f, 1);
	}
}
