namespace _Climate.Scripts;

using System;
using System.Collections.Generic;

public class SurfaceAreaCells
{
	private int Length;

	// 体表包含6个面
	public enum Orientation : int
	{
		Up = 0,
		Down,
		Left,
		Right,
		Forward,
		Backward
	}

	// 每个面有4个邻接的面
	public enum Direction : int
	{
		Up = 0,
		Down,
		Left,
		Right
	}

	// 名字太长了但是不这样写enum就得强制转换为int做数组下标我讨厌你
	public class SurfaceCellsOrientationArray<TEnum, TValue>
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

	// 链表节点的类定义，他被用来连接六个面和他们的旋转对应关系，链表包含四个属性分别是上下左右四个方向的邻居节点和其为了对齐当前节点所需的旋转角度
	public class SurfaceCellNode
	{
		// 6个表面
		public SurfaceCells Surface;

		// 4个邻接面
		public Dictionary<Direction, (SurfaceCellNode Node, int Rotation)> Neighbors { get; set; }

		public SurfaceCellNode(SurfaceCells surface)
		{
			Surface = surface;
			Neighbors = new Dictionary<Direction, (SurfaceCellNode, int)>();
		}

		// 深拷贝
		public SurfaceCellNode Clone()
		{
			SurfaceCells clonedSurface = Surface.Clone();

			SurfaceCellNode clone = new SurfaceCellNode(clonedSurface);

			foreach (var entry in Neighbors)
			{
				Direction direction = entry.Key;
				SurfaceCellNode neighborNode = entry.Value.Node;
				int rotation = entry.Value.Rotation;

				clone.Neighbors[direction] = (neighborNode.Clone(), rotation);
			}

			return clone;
		}
	}

	// 6个面
	public SurfaceCellsOrientationArray<Orientation, SurfaceCellNode> surfaceCellNodes =
		new SurfaceCellsOrientationArray<Orientation, SurfaceCellNode>();

	public SurfaceAreaCells(int length)
	{
		Length = length;

		foreach (Orientation dir in Enum.GetValues(typeof(Orientation)))
		{
			surfaceCellNodes[dir] = new SurfaceCellNode(
				new SurfaceCells(Length)
			);
		}

		SetNeighbors();
	}

	private void SetNeighbors()
	{
		// 邻接表
		var neighborMap = new Dictionary<Orientation, Dictionary<Direction, (Orientation, int)>>()
		{
			[Orientation.Up] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Backward, 0) },
				{ Direction.Down,  (Orientation.Forward, 2) },
				{ Direction.Left,  (Orientation.Left,     3) },
				{ Direction.Right, (Orientation.Right,    1) }
			},
			[Orientation.Down] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Forward,  0) },
				{ Direction.Down,  (Orientation.Backward, 2) },
				{ Direction.Left,  (Orientation.Left,     1) },
				{ Direction.Right, (Orientation.Right,    3) }
			},
			[Orientation.Left] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Up,      1) },
				{ Direction.Down,  (Orientation.Down,    3) },
				{ Direction.Left,  (Orientation.Backward, 2) },
				{ Direction.Right, (Orientation.Forward,   0) }
			},
			[Orientation.Right] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Up,     3) },
				{ Direction.Down,  (Orientation.Down,    1) },
				{ Direction.Left,  (Orientation.Forward,  0) },
				{ Direction.Right, (Orientation.Backward,2) }
			},
			[Orientation.Forward] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Up,     2) },
				{ Direction.Down,  (Orientation.Down,     0) },
				{ Direction.Left,  (Orientation.Left,     0) },
				{ Direction.Right, (Orientation.Right,    0) }
			},
			[Orientation.Backward] = new Dictionary<Direction, (Orientation, int)>
			{
				{ Direction.Up,    (Orientation.Up,       0) },
				{ Direction.Down,  (Orientation.Down,    2) },
				{ Direction.Left,  (Orientation.Right,    0) },
				{ Direction.Right, (Orientation.Left,     0) }
			}
		};

		// 对6面的遍历, 去赋予他的4方向
		foreach (Orientation face in Enum.GetValues(typeof(Orientation)))
		{
			SurfaceCellNode currentNode = surfaceCellNodes[face];

			foreach (KeyValuePair<Direction, (Orientation, int)> entry in neighborMap[face])
			{
				Direction dir = entry.Key;
				Orientation neighborFace = entry.Value.Item1;
				int rotation = entry.Value.Item2;

				currentNode.Neighbors[dir] = (surfaceCellNodes[neighborFace], rotation);
			}
		}
	}

	public SurfaceAreaCells Clone()
	{
		SurfaceAreaCells clone = new SurfaceAreaCells(Length);

		clone.Length = this.Length;

		// 复制每个面的Surface数据
		foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
		{
			clone.surfaceCellNodes[orientation] = surfaceCellNodes[orientation].Clone();
		}


		return clone;
	}
}
