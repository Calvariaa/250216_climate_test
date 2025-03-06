using Godot;
using static Godot.GD;

namespace _Climate.Scripts;

using System;
using System.Collections.Generic;

public class SpaceCells
{
	private enum Orientation
	{
		Up = 0,
		Down,
		Left,
		Right,
		Forward,
		Backward,
	}
	
	private enum Direction
	{
		Up = 0,
		Down,
		Left,
		Right
	}

	// 名字太长了但是不这样写enum就得强制转换为int做数组下标我讨厌你
	private class SurfaceCellsOrientationArray<TEnum, TValue>
		where TEnum : Enum
	{
		private TValue[] array = new TValue[Enum.GetValues(typeof(TEnum)).Length];

		public TValue this[TEnum index]
		{
			// cs wtmcnm
			get => array[Convert.ToInt32(index)];
			set => array[Convert.ToInt32(index)] = value;
		}
	}

	// 链表连接六个面和他们的旋转对应关系反正后面会写额啊
	private class SurfaceCellNode
	{
		public SurfaceCellsOrientationArray<Orientation, SurfaceCells> Surface { get; set; }
		public Dictionary<Orientation, (SurfaceCellNode Node, int Rotation)> Neighbors { get; set; }

		public SurfaceCellNode(SurfaceCellsOrientationArray<Orientation, SurfaceCells> surface)
		{
			Surface = surface;
			Neighbors = new Dictionary<Orientation, (SurfaceCellNode, int)>();
		}
	}

	private int Length;
	private int Depth;

	private SurfaceCellsOrientationArray<Orientation, SurfaceCells> surfaceCells =
		new SurfaceCellsOrientationArray<Orientation, SurfaceCells>();
	private SurfaceCellsOrientationArray<Orientation, SurfaceCellNode> surfaceCellNodes =
		new SurfaceCellsOrientationArray<Orientation, SurfaceCellNode>();

	public SpaceCells(int length, int depth)
	{
		Length = length;
		Depth = depth;

		foreach (Orientation dir in Enum.GetValues(typeof(Orientation)))
		{
			surfaceCellNodes[dir] = new SurfaceCellNode(
				new SurfaceCellsOrientationArray<Orientation, SurfaceCells>()
			);
		}

		SetNeighbors();
	}

	private void SetNeighbors()
	{
		// Up
		surfaceCellNodes[Orientation.Up].Neighbors[Orientation.Forward] = (
			surfaceCellNodes[Orientation.Forward],
			0
		);
		surfaceCellNodes[Orientation.Up].Neighbors[Orientation.Right] = (
			surfaceCellNodes[Orientation.Right],
			0
		);
		surfaceCellNodes[Orientation.Up].Neighbors[Orientation.Backward] = (
			surfaceCellNodes[Orientation.Backward],
			0
		);
		surfaceCellNodes[Orientation.Up].Neighbors[Orientation.Left] = (
			surfaceCellNodes[Orientation.Left],
			0
		);

		// Down
		surfaceCellNodes[Orientation.Down].Neighbors[Orientation.Forward] = (
			surfaceCellNodes[Orientation.Forward],
			2
		);
		surfaceCellNodes[Orientation.Down].Neighbors[Orientation.Right] = (
			surfaceCellNodes[Orientation.Left],
			0
		);
		surfaceCellNodes[Orientation.Down].Neighbors[Orientation.Backward] = (
			surfaceCellNodes[Orientation.Backward],
			2
		);
		surfaceCellNodes[Orientation.Down].Neighbors[Orientation.Left] = (
			surfaceCellNodes[Orientation.Right],
			0
		);

		// Forward
		surfaceCellNodes[Orientation.Forward].Neighbors[Orientation.Up] = (
			surfaceCellNodes[Orientation.Up],
			0
		);
		surfaceCellNodes[Orientation.Forward].Neighbors[Orientation.Right] = (
			surfaceCellNodes[Orientation.Right],
			1
		);
		surfaceCellNodes[Orientation.Forward].Neighbors[Orientation.Down] = (
			surfaceCellNodes[Orientation.Down],
			2
		);
		surfaceCellNodes[Orientation.Forward].Neighbors[Orientation.Left] = (
			surfaceCellNodes[Orientation.Left],
			3
		);

		// Backward
		surfaceCellNodes[Orientation.Backward].Neighbors[Orientation.Up] = (
			surfaceCellNodes[Orientation.Up],
			2
		);
		surfaceCellNodes[Orientation.Backward].Neighbors[Orientation.Right] = (
			surfaceCellNodes[Orientation.Left],
			1
		);
		surfaceCellNodes[Orientation.Backward].Neighbors[Orientation.Down] = (
			surfaceCellNodes[Orientation.Down],
			0
		);
		surfaceCellNodes[Orientation.Backward].Neighbors[Orientation.Left] = (
			surfaceCellNodes[Orientation.Right],
			3
		);

		// Right
		surfaceCellNodes[Orientation.Right].Neighbors[Orientation.Up] = (
			surfaceCellNodes[Orientation.Up],
			1
		);
		surfaceCellNodes[Orientation.Right].Neighbors[Orientation.Forward] = (
			surfaceCellNodes[Orientation.Forward],
			3
		);
		surfaceCellNodes[Orientation.Right].Neighbors[Orientation.Down] = (
			surfaceCellNodes[Orientation.Down],
			1
		);
		surfaceCellNodes[Orientation.Right].Neighbors[Orientation.Backward] = (
			surfaceCellNodes[Orientation.Backward],
			1
		);

		// Left
		surfaceCellNodes[Orientation.Left].Neighbors[Orientation.Up] = (
			surfaceCellNodes[Orientation.Up],
			3
		);
		surfaceCellNodes[Orientation.Left].Neighbors[Orientation.Forward] = (
			surfaceCellNodes[Orientation.Forward],
			1
		);
		surfaceCellNodes[Orientation.Left].Neighbors[Orientation.Down] = (
			surfaceCellNodes[Orientation.Down],
			3
		);
		surfaceCellNodes[Orientation.Left].Neighbors[Orientation.Backward] = (
			surfaceCellNodes[Orientation.Backward],
			3
		);
	}
}
