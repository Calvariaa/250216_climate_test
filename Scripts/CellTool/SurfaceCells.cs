using Godot;
namespace _Climate.Scripts;

public class SurfaceCells
{
	private readonly uint Length;
	private Cell[,] cells;
	public AreaOrientation Orientation;

	public SurfaceCells(uint length, AreaOrientation orientation)
	{
		Length = length;
		Orientation = orientation;
		cells = new Cell[length, length];
		InitializeCells();
	}

	private void InitializeCells()
	{
		for (int i = 0; i < Length; i++)
		{
			for (int j = 0; j < Length; j++)
			{
				cells[i, j] = new Cell();
			}
		}
	}

	public Cell Cell(int i, int j)
	{
		return cells[i, j];
	}

	public uint GetLength()
	{
		return Length;
	}

	// i
	public static int GetRotatedI(int i, int j, uint length, int rotation)
	{
		switch (rotation)
		{
			case 0:
				return i;
			case 1:
				return (int)length - j - 1;
			case 2:
				return (int)length - i - 1;
			case 3:
				return j;
			default:
				return -1;
		}
	}

	// j
	public static int GetRotatedJ(int i, int j, uint length, int rotation)
	{
		switch (rotation)
		{
			case 0:
				return j;
			case 1:
				return i;
			case 2:
				return (int)length - j - 1;
			case 3:
				return (int)length - i - 1;
			default:
				return -1;
		}
	}
	public Cell Cell(int i, int j, int rotation)
	{
		return cells[GetRotatedI(i, j, Length, rotation), GetRotatedJ(i, j, Length, rotation)];
	}


	public SurfaceCells Clone()
	{
		SurfaceCells clone = new SurfaceCells(Length, Orientation);
		for (int i = 0; i < Length; i++)
		{
			for (int j = 0; j < Length; j++)
			{
				clone.cells[i, j] = new Cell
				{
					Temperature = cells[i, j].Temperature,
					Position = cells[i, j].Position
				};
			}
		}
		return clone;
	}
}
