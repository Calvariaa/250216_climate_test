using Godot;
using System;
namespace _Climate.Scripts;

public class Cell
{
	private double _temperature;
	public double Temperature {
		get => _temperature;
		set {
			_temperature = Math.Clamp(value, -120, 120);
		}
	}

	public Vector3 Position { get; set; }

	public string Name { get; set; }
}
