using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;

public partial class CellsBaseMultiMeshInstance : MultiMeshInstance3D
{
	[Export] float CellSize = 0.1f;
	[Export] int Length = 100;
	[Export] double Alpha = 1e-4;

	private TemperatureCalculator temperCalc;
	private SurfaceAreaCells cells;
	public override void _Ready()
	{

		// Create the multimesh.
		Multimesh = new MultiMesh();

		Multimesh.UseColors = true;
		// Set the format first.
		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		// Then resize (otherwise, changing the format is not allowed)
		Multimesh.InstanceCount = Length * Length * 6;
		// Maybe not all of them should be visible at first.
		Multimesh.VisibleInstanceCount = Length * Length * 6;

		Multimesh.Mesh = new PlaneMesh();

		cells = new SurfaceAreaCells(Length);

		temperCalc = new TemperatureCalculator(Length, Alpha, cells);

		// Set the transform of the instances.
		foreach (var orintation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					Vector3 position;
					Vector3 rotation_axis;
					float rotation_angle;
					switch (orintation)
					{
						case AreaOrientation.Up:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								((Length + 1.0f) / 2.0f),
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(0, 0, 0);
							rotation_angle = 0;
							break;
						case AreaOrientation.Down:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								(-(Length + 1.0f) / 2.0f),
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(1, 0, 0);
							rotation_angle = (float)Math.PI;
							break;
						case AreaOrientation.Left:
							position = new Vector3(
								(-(Length + 1.0f) / 2.0f),
								(j - (Length - 1) / 2.0f),
								(i - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(0, 0, 1);
							rotation_angle = (float)(Math.PI / 2.0);
							break;
						case AreaOrientation.Right:
							position = new Vector3(
								((Length + 1.0f) / 2.0f),
								(j - (Length - 1) / 2.0f),
								(i - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(0, 0, 1);
							rotation_angle = -(float)(Math.PI / 2.0);
							break;
						case AreaOrientation.Forward:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								(j - (Length - 1) / 2.0f),
								((Length + 1.0f) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(1, 0, 0);
							rotation_angle = (float)(Math.PI / 2.0);
							break;
						case AreaOrientation.Backward:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								(j - (Length - 1) / 2.0f),
								(-(Length + 1.0f) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(1, 0, 0);
							rotation_angle = -(float)(Math.PI / 2.0);
							break;
						default:
							position = Vector3.Zero;
							rotation_axis = Vector3.Zero;
							rotation_angle = 0;
							break;
					}
					var multiMeshTransform =
						new Transform3D(Basis.FromScale(new Vector3(CellSize, CellSize, CellSize)) * new Basis(rotation_axis, rotation_angle), position);

					Multimesh.SetInstanceTransform((int)orintation * Length * Length + i * Length + j, multiMeshTransform);

					Multimesh.SetInstanceColor((int)orintation * Length * Length + i * Length + j, Colors.Pink);
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		return;
		temperCalc.Calculate(delta);

		// 随机生成温度
		foreach (AreaOrientation AreaOrientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			if (Randf() < 0.1)
			{
				// cellCalculator.cells[RandRange(0, cellCalculator.width - 1), RandRange(0, cellCalculator.height - 1)]
				//     .temperCalc = RandRange(-10, 255);
				var radius = RandRange(1, 10);
				var orintation = RandRange(0, 5);
				var width = RandRange(radius, temperCalc.Length - radius);
				var height = RandRange(radius, temperCalc.Length - radius);
				var temperature = RandRange(-100, 100);

				for (var i = 0; i < temperCalc.Length; i++)
				{
					for (var j = 0; j < temperCalc.Length; j++)
					{
						if (Mathf.Pow(i - width, 2) + Mathf.Pow(j - height, 2) < radius)
						{
							Multimesh.SetInstanceColor((int)orintation * Length * Length + i * Length + j, CalculateTemperatureColor(temperature));
						}
					}
				}
			}
		}

		// Multimesh.SetInstanceColor((int)orintation * Length * Length + i * Length + j, Colors.Pink);
		foreach (AreaOrientation orintation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < temperCalc.Length; i++)
			{
				for (int j = 0; j < temperCalc.Length; j++)
				{
					Multimesh.SetInstanceColor((int)orintation * Length * Length + i * Length + j, CalculateTemperatureColor((float)cells.surfaceCellNodes[orintation].Surface.Cell(i, j, 0).Temperature));
				}
			}
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

			// if (keyEvent.Keycode == Key.T)
			// {
			// 	Print(_mapType);
			// 	if (_mapType < MapType.temperCalcNum - 1)
			// 	{
			// 		_mapType++;
			// 	}
			// 	else
			// 	{
			// 		_mapType = 0;
			// 	}
			// }
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
