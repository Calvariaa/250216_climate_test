using Godot;
using System;
namespace _Climate.Scripts;

public class Cell
{
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
			_temperature = Math.Clamp(value, -120, 120);
		}
	}

	public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

	public string Name { get; set; } = "Cell_";
}
