using Godot;
using System;
using static Godot.GD;
using _Climate.Scripts;

public partial class CellsBaseMeshInstance : MeshInstance3D
{
	[Export] Node3D node3D;
	StandardMaterial3D material3D;


	private float? _temperature;
	public float Temperature
	{
		get
		{
			_temperature ??= 0;
			return (float)_temperature;
		}
		set
		{
			_temperature = value;
			SetColor(Temperature, material3D);
		}
	}

	// private AreaOrientation? _orientation;
	// public AreaOrientation Orientation
	// {
	// 	get
	// 	{
	// 		_orientation ??= AreaOrientation.Up;
	// 		return (AreaOrientation)_orientation;
	// 	}
	// 	set
	// 	{
	// 		_orientation = value;
	// 		SetRotation(Orientation, node3D);
	// 	}
	// }


	public override void _Ready()
	{
		material3D = new StandardMaterial3D();
		MaterialOverride = material3D;
		SetColor(Temperature, material3D);
		// SetRotation(Orientation, node3D);
		// Print(_temperature);
	}

	private static void SetColor(float _temperature, StandardMaterial3D material3D)
	{
		Color color;
		var temperature = Mathf.Clamp(_temperature, -120, 120);
		// 将temperature的-120到0度映射到Hue的300/360到65/360, 0到120度映射到Hue的65/360到0
		if (temperature > 0)
		{
			color = Color.FromHsv((65.0f - temperature * 13 / 24.0f) / 360.0f, 0.64f, 1);
		}
		else
		{
			color = Color.FromHsv((65.0f - temperature * 47 / 24.0f) / 360.0f, 0.64f, 1);
		}

		material3D.AlbedoColor = color;
	}

	// private static void SetRotation(AreaOrientation Orientation, Node3D node3D)
	// {
	// 	Vector3 RotationDegrees;
	// 	switch (Orientation)
	// 	{
	// 		case AreaOrientation.Up:
	// 			RotationDegrees = new Vector3(0, 0, 0);
	// 			break;
	// 		case AreaOrientation.Down:
	// 			RotationDegrees = new Vector3(180, 0, 0);
	// 			break;
	// 		case AreaOrientation.Left:
	// 			RotationDegrees = new Vector3(0, 0, 90);
	// 			break;
	// 		case AreaOrientation.Right:
	// 			RotationDegrees = new Vector3(0, 0, -90);
	// 			break;
	// 		case AreaOrientation.Forward:
	// 			RotationDegrees = new Vector3(90, 0, 0);
	// 			break;
	// 		case AreaOrientation.Backward:
	// 			RotationDegrees = new Vector3(-90, 0, 0);
	// 			break;
	// 		default:
	// 			RotationDegrees = new Vector3(0, 0, 0);
	// 			break;
	// 	}

	// 	node3D.Rotation = RotationDegrees;
	// }
}
