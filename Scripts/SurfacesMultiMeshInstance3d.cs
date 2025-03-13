using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;
using System.Diagnostics;

public partial class SurfacesMultiMeshInstance3d : MultiMeshInstance3D
{
	[Export] private string ComputePath;
	[Export] float CellSize = 0.1f;
	[Export] uint Length = 32;
	[Export] float Alpha = 1e-4F;

	private TemperatureComputeCalculator temperComputeCalc;
	private SurfaceAreaCells cells;
	private ShaderMaterial localShaderMaterial;

	// private Texture2Drd TemperatureTexture;

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
		PlaneMesh.Size = new Vector2(1.0f, 1.0f);
		PlaneMesh.Material = Material;

		Multimesh.Mesh = PlaneMesh;

		Multimesh.InstanceCount = (int)(Length * Length * 6);
		Multimesh.VisibleInstanceCount = (int)(Length * Length * 6);

		cells = new SurfaceAreaCells(Length);

		// localShaderMaterial = Position with { X = 100.0f };	

		localShaderMaterial = this.MaterialOverride as ShaderMaterial;

		// TemperatureTexture = (Texture2Drd)localShaderMaterial.GetShaderParameter("temperature_texture");
		localShaderMaterial.SetShaderParameter("cell_length", (uint)Length);


		// Print(localShaderMaterial.GetShaderParameter("temperature_texture"));

		temperComputeCalc = new TemperatureComputeCalculator(ComputePath, Length, Alpha, cells);

		foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					// if (i == 0) cells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature = -120;
					// 所以x==0是左，y==0是上

					var id = (int)orientation * (int)Length * (int)Length + i * (int)Length + j;
					Multimesh.SetInstanceCustomData(id, new Color((uint)id, (uint)Length, 0, 0));
				}
			}
		}

		MapCellsToCube();
	}

	public override void _Process(double delta)
	{
		// localShaderMaterial.SetShaderParameter("temperature_data", temperComputeCalc.computeShaderInstance.GetBuffer(0));

		temperComputeCalc.computeShaderInstance.UpdateBuffer((float)delta, 0, 2);

		localShaderMaterial.SetShaderParameter("temperature_texture", temperComputeCalc.computeShaderInstance.GetBufferRid(1u, 0));
		// localShaderMaterial.SetShaderParameter("temperature_texture", Tool.ConvertToByteArray(temperComputeCalc.LocalCellsList));

		// temperComputeCalc.UpdateCompute();

		// localShaderMaterial.SetShaderParameter("temperature_data", temperComputeCalc.computeShaderInstance.GetFloatArrayResult(0));

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
}
