using Godot;
using static Godot.GD;
namespace _Climate.Scripts;
using System;
using System.Collections.Generic;

public class SpaceCells
{
	private enum Direction
	{
		Up = 0,
		Down,
		Left,
		Right,
		Forward,
		Backward
	}


	// 名字太长了但是不这样写enum就得强制转换为int做数组下标我讨厌你
	private class SurfaceCellsDirectionArray<TEnum, TValue> where TEnum : Enum
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
		public SurfaceCellsDirectionArray<Direction, SurfaceCells> Surface { get; set; }
		public Dictionary<Direction, (SurfaceCellNode Node, int Rotation)> Neighbors { get; set; }

		public SurfaceCellNode(SurfaceCellsDirectionArray<Direction, SurfaceCells> surface)
		{
			Surface = surface;
			Neighbors = new Dictionary<Direction, (SurfaceCellNode, int)>();
		}
	}

	
	private int Length;
	private int Depth;
	
	private SurfaceCellsDirectionArray<Direction, SurfaceCells> 
		surfaceCells = new SurfaceCellsDirectionArray<Direction, SurfaceCells>();
	private SurfaceCellsDirectionArray<Direction, SurfaceCellNode> 
		surfaceCellNodes = new SurfaceCellsDirectionArray<Direction, SurfaceCellNode>();

	public SpaceCells(int length, int depth)
	{
		Length = length;
		Depth = depth;

		foreach (var dir in Enum.GetValues(typeof(Direction)))
		{
			surfaceCellNodes[dir] = new SurfaceCellNode(new SurfaceCellsDirectionArray<Direction, SurfaceCells>());
		}
	
		SetNeighbors();
	}


	private void SetNeighbors()
	{
		// Up
		surfaceCellNodes[Direction.Up].Neighbors[Direction.Forward] = (surfaceCellNodes[Direction.Forward], 0);
		surfaceCellNodes[Direction.Up].Neighbors[Direction.Right] = (surfaceCellNodes[Direction.Right], 0);
		surfaceCellNodes[Direction.Up].Neighbors[Direction.Backward] = (surfaceCellNodes[Direction.Backward], 0);
		surfaceCellNodes[Direction.Up].Neighbors[Direction.Left] = (surfaceCellNodes[Direction.Left], 0);

		// Down
		surfaceCellNodes[Direction.Down].Neighbors[Direction.Forward] = (surfaceCellNodes[Direction.Forward], 2);
		surfaceCellNodes[Direction.Down].Neighbors[Direction.Right] = (surfaceCellNodes[Direction.Left], 0);
		surfaceCellNodes[Direction.Down].Neighbors[Direction.Backward] = (surfaceCellNodes[Direction.Backward], 2);
		surfaceCellNodes[Direction.Down].Neighbors[Direction.Left] = (surfaceCellNodes[Direction.Right], 0);

		// Forward
		surfaceCellNodes[Direction.Forward].Neighbors[Direction.Up] = (surfaceCellNodes[Direction.Up], 0);
		surfaceCellNodes[Direction.Forward].Neighbors[Direction.Right] = (surfaceCellNodes[Direction.Right], 1);
		surfaceCellNodes[Direction.Forward].Neighbors[Direction.Down] = (surfaceCellNodes[Direction.Down], 2);
		surfaceCellNodes[Direction.Forward].Neighbors[Direction.Left] = (surfaceCellNodes[Direction.Left], 3);

		// Backward
		surfaceCellNodes[Direction.Backward].Neighbors[Direction.Up] = (surfaceCellNodes[Direction.Up], 2);
		surfaceCellNodes[Direction.Backward].Neighbors[Direction.Right] = (surfaceCellNodes[Direction.Left], 1);
		surfaceCellNodes[Direction.Backward].Neighbors[Direction.Down] = (surfaceCellNodes[Direction.Down], 0);
		surfaceCellNodes[Direction.Backward].Neighbors[Direction.Left] = (surfaceCellNodes[Direction.Right], 3);

		// Right
		surfaceCellNodes[Direction.Right].Neighbors[Direction.Up] = (surfaceCellNodes[Direction.Up], 1);
		surfaceCellNodes[Direction.Right].Neighbors[Direction.Forward] = (surfaceCellNodes[Direction.Forward], 3);
		surfaceCellNodes[Direction.Right].Neighbors[Direction.Down] = (surfaceCellNodes[Direction.Down], 1);
		surfaceCellNodes[Direction.Right].Neighbors[Direction.Backward] = (surfaceCellNodes[Direction.Backward], 1);

		// Left
		surfaceCellNodes[Direction.Left].Neighbors[Direction.Up] = (surfaceCellNodes[Direction.Up], 3);
		surfaceCellNodes[Direction.Left].Neighbors[Direction.Forward] = (surfaceCellNodes[Direction.Forward], 1);
		surfaceCellNodes[Direction.Left].Neighbors[Direction.Down] = (surfaceCellNodes[Direction.Down], 3);
		surfaceCellNodes[Direction.Left].Neighbors[Direction.Backward] = (surfaceCellNodes[Direction.Backward], 3);
	}

}
