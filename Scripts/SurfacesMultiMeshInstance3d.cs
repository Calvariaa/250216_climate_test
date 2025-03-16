using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;
using System.Diagnostics;

public partial class SurfacesMultiMeshInstance3d : MultiMeshInstance3D
{
	[Export] private string ComputePath;
	[Export] float CellSize = 0.1f;
	[Export(PropertyHint.Range, "32,1024,32,or_greater")] uint Length = 32; //  范围是大于等于32且为32倍数
	[Export(PropertyHint.ExpEasing)] float Alpha = 1e-4F;

	private TemperatureComputeCalculator temperComputeCalc;
	private SurfaceAreaCells cells;
	private ShaderMaterial localShaderMaterial;

	enum CellsType
	{
		CellsCube,
		CellsFlat,
		CellsTypeNum
	}
	CellsType _cellsType = CellsType.CellsCube;
	public override void _Ready()
	{
		// Create the multimesh.
		Multimesh = new MultiMesh();

		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;

		var Material = new ShaderMaterial();

		Multimesh.UseCustomData = true;

		var PlaneMesh = new PlaneMesh();
		// var PlaneMesh = new PointMesh();
		PlaneMesh.Size = new Vector2(1.0f, 1.0f);
		PlaneMesh.Material = Material;

		Multimesh.Mesh = PlaneMesh;

		Multimesh.InstanceCount = (int)(Length * Length * 6);
		Multimesh.VisibleInstanceCount = (int)(Length * Length * 6);

		cells = new SurfaceAreaCells(Length);

		localShaderMaterial = this.MaterialOverride as ShaderMaterial;
		localShaderMaterial.SetShaderParameter("cell_length", (uint)Length);

		Print("cell_length: ", string.Join(", ", localShaderMaterial.GetShaderParameter("cell_length")));

		temperComputeCalc = new TemperatureComputeCalculator(ComputePath, localShaderMaterial, Length, Alpha, cells);


		foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					// if (i == 0) cells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature = -120;
					// 所以x==0是左，y==0是上

					var id = (int)orientation * (int)Length * (int)Length + i * (int)Length + j;
					Multimesh.SetInstanceCustomData(id, new Color((uint)Length, (uint)orientation, (uint)i, (uint)j));
				}
			}
		}

		MapCellsToCube();
	}

	public override void _Process(double delta)
	{
		temperComputeCalc.Calculate(delta);
	}

	private void MapCellsToCube()
	{
		// Set the transform of the instances.
		foreach (var orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					Vector3 position;
					Vector3 rotation_axis;
					float rotation_angle;
					switch (orientation)
					{
						case AreaOrientation.Up:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								((Length) / 2.0f),
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(0, 0, 0);
							rotation_angle = 0;
							break;
						case AreaOrientation.Down:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								(-(Length) / 2.0f),
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(1, 0, 0);
							rotation_angle = (float)Math.PI;
							break;
						case AreaOrientation.Left:
							position = new Vector3(
								(-(Length) / 2.0f),
								(j - (Length - 1) / 2.0f),
								(i - (Length - 1) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(0, 0, 1);
							rotation_angle = (float)(Math.PI / 2.0);
							break;
						case AreaOrientation.Right:
							position = new Vector3(
								((Length) / 2.0f),
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
								((Length) / 2.0f)
							) * CellSize;
							rotation_axis = new Vector3(1, 0, 0);
							rotation_angle = (float)(Math.PI / 2.0);
							break;
						case AreaOrientation.Backward:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								(j - (Length - 1) / 2.0f),
								(-(Length) / 2.0f)
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

					Multimesh.SetInstanceTransform((int)orientation * (int)Length * (int)Length + i * (int)Length + j, multiMeshTransform);
				}
			}
		}
	}

	private void MapCellsToFlat()
	{
		// Set the transform of the instances.
		foreach (var orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					Vector3 position;
					switch (orientation)
					{
						case AreaOrientation.Up:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								0,
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							break;
						case AreaOrientation.Down:
							position = new Vector3(
								(i - (Length - 1) / 2.0f + Length * 2),
								0,
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							break;
						case AreaOrientation.Left:
							position = new Vector3(
								(i - (Length - 1) / 2.0f - Length),
								0,
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							break;
						case AreaOrientation.Right:
							position = new Vector3(
								(i - (Length - 1) / 2.0f + Length),
								0,
								(j - (Length - 1) / 2.0f)
							) * CellSize;
							break;
						case AreaOrientation.Forward:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								0,
								(j - (Length - 1) / 2.0f + Length)
							) * CellSize;
							break;
						case AreaOrientation.Backward:
							position = new Vector3(
								(i - (Length - 1) / 2.0f),
								0,
								(j - (Length - 1) / 2.0f - Length)
							) * CellSize;
							break;
						default:
							position = Vector3.Zero;
							break;
					}
					var multiMeshTransform =
						new Transform3D(Basis.FromScale(new Vector3(CellSize, CellSize, CellSize)), position);

					Multimesh.SetInstanceTransform((int)orientation * (int)Length * (int)Length + i * (int)Length + j, multiMeshTransform);
				}
			}
		}
	}

	public override void _Input(InputEvent @event)
	{

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			//if (keyEvent.Keycode == Key.R)
			//{
			//temperCalc.ClearCells();
			//}

			if (keyEvent.Keycode == Key.Tab)
			{
				Print(_cellsType);
				if (_cellsType < CellsType.CellsTypeNum - 1)
				{
					_cellsType++;
				}
				else
				{
					_cellsType = 0;
				}

				switch (_cellsType)
				{
					case CellsType.CellsCube:
						MapCellsToCube();
						break;
					case CellsType.CellsFlat:
						MapCellsToFlat();
						break;
				}
			}
		}
	}
}
