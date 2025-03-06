using Godot;
using System;
using static Godot.GD;


public partial class MeshInstance3d : MeshInstance3D
{
	StandardMaterial3D material3D;
	public float Temperature
	{
		get
		{
			temperature ??= 0;
			return (float)temperature;
		}
		set
		{
			temperature = value;
			SetColor(Temperature, material3D);
		}
	}

	private float? temperature;

	public override void _Ready()
	{
		material3D = new StandardMaterial3D();
		MaterialOverride = material3D;
		SetColor(Temperature, material3D);
		// Print(temperature);
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
}
