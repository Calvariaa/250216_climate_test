using Godot;
using static Godot.GD;
namespace _Climate.Scripts;
using System;
using System.Collections.Generic;

public class SpaceCellsBinder
{

	[Export] private int Length = 10;
	[Export] private int Depth = 10;

	private SpaceCells[] spaceCells;

	public SpaceCellsBinder()
	{
		// cells总数为正方体的表面积, 然后每有一层深度就加上一个表面积
		

		// spaceCells = new SpaceCells();
	}
}
