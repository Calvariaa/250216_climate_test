namespace _Climate.Scripts;

using System;
using System.Collections.Generic;
using Godot;

public enum AreaOrientation : int
{
	Up = 0,
	Down,
	Left,
	Right,
	Forward,
	Backward
}

// 每个面有4个邻接的面
public enum AreaDirection : int
{
	Up = 0,
	Down,
	Left,
	Right
}


public class SurfaceAreaCells
{
	private uint Length;

	// 体表包含6个面
	// 名字太长了但是不这样写enum就得强制转换为int做数组下标我讨厌你
	public class SurfaceCellsAreaOrientationArray<TEnum, TValue>
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
		public Dictionary<AreaDirection, (SurfaceCellNode Node, int Rotation)> Neighbors { get; set; }

		public SurfaceCellNode(SurfaceCells surface)
		{
			Surface = surface;
			Neighbors = new Dictionary<AreaDirection, (SurfaceCellNode, int)>();
		}

		// 深拷贝
		public SurfaceCellNode Clone()
		{
			SurfaceCells clonedSurface = Surface.Clone();

			SurfaceCellNode clone = new SurfaceCellNode(clonedSurface);

			foreach (var entry in Neighbors)
			{
				AreaDirection direction = entry.Key;
				SurfaceCellNode neighborNode = entry.Value.Node;
				int rotation = entry.Value.Rotation;

				clone.Neighbors[direction] = (neighborNode.Clone(), rotation);
			}

			return clone;
		}
	}

	// 6个面
	public SurfaceCellsAreaOrientationArray<AreaOrientation, SurfaceCellNode> surfaceCellNodes =
		new SurfaceCellsAreaOrientationArray<AreaOrientation, SurfaceCellNode>();

	public SurfaceAreaCells(uint length)
	{
		Length = length;

		foreach (AreaOrientation dir in Enum.GetValues(typeof(AreaOrientation)))
		{
			surfaceCellNodes[dir] = new SurfaceCellNode(
				new SurfaceCells(Length, dir)
			);
		}

		SetNeighbors();
	}

	private void SetNeighbors()
	{
		// 邻接表
		var neighborMap = new Dictionary<AreaOrientation, Dictionary<AreaDirection, (AreaOrientation, int)>>()
		{
			[AreaOrientation.Up] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Backward, 0) },
				{ AreaDirection.Down,  (AreaOrientation.Forward,  0) },
				{ AreaDirection.Left,  (AreaOrientation.Left,     0) },
				{ AreaDirection.Right, (AreaOrientation.Right,    0) }
			},
			[AreaOrientation.Down] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Backward, 	 2) },
				{ AreaDirection.Down,  (AreaOrientation.Forward, 	2) },
				{ AreaDirection.Left,  (AreaOrientation.Right,   	  0) },
				{ AreaDirection.Right, (AreaOrientation.Left,   	 0) }
			},
			[AreaOrientation.Left] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Backward,      3) },
				{ AreaDirection.Down,  (AreaOrientation.Forward,    1) },
				{ AreaDirection.Left,  (AreaOrientation.Down, 		0) },
				{ AreaDirection.Right, (AreaOrientation.Up,  		 0) }
			},
			[AreaOrientation.Right] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Backward,     1) },
				{ AreaDirection.Down,  (AreaOrientation.Forward,  		  3) },
				{ AreaDirection.Left,  (AreaOrientation.Up, 		 0) },
				{ AreaDirection.Right, (AreaOrientation.Down,		0) }
			},
			[AreaOrientation.Forward] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Up,     0) },
				{ AreaDirection.Down,  (AreaOrientation.Down,     2) },
				{ AreaDirection.Left,  (AreaOrientation.Left,     3) },
				{ AreaDirection.Right, (AreaOrientation.Right,    1) }
			},
			[AreaOrientation.Backward] = new Dictionary<AreaDirection, (AreaOrientation, int)>
			{
				{ AreaDirection.Up,    (AreaOrientation.Down,       2) },
				{ AreaDirection.Down,  (AreaOrientation.Up,         0) },
				{ AreaDirection.Left,  (AreaOrientation.Left,       1) },
				{ AreaDirection.Right, (AreaOrientation.Right,      3) }
			}
		};

		// 对6面的遍历, 去赋予他的4方向
		foreach (AreaOrientation face in Enum.GetValues(typeof(AreaOrientation)))
		{
			SurfaceCellNode currentNode = surfaceCellNodes[face];

			foreach (KeyValuePair<AreaDirection, (AreaOrientation, int)> entry in neighborMap[face])
			{
				AreaDirection dir = entry.Key;
				AreaOrientation neighborFace = entry.Value.Item1;
				int rotation = entry.Value.Item2;

				GD.Print($"face: {face}, dir: {dir}, neighborFace: {neighborFace}, rotation: {rotation}");

				currentNode.Neighbors[dir] = (surfaceCellNodes[neighborFace], rotation);
			}
		}
	}

	public SurfaceAreaCells Clone()
	{
		SurfaceAreaCells clone = new SurfaceAreaCells(Length);

		clone.Length = this.Length;

		// 复制每个面的Surface数据
		foreach (AreaOrientation AreaOrientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			clone.surfaceCellNodes[AreaOrientation] = surfaceCellNodes[AreaOrientation].Clone();
		}


		return clone;
	}
}
