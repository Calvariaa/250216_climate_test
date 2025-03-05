using Godot;
namespace _Climate.Scripts;

public class SpaceCells
{
	private int Length;
	private string Name;
	private Cell[,] cells;

	public SpaceCells(int length, string name)
	{
		Length = length;
		Name = name;
		cells = new Cell[Length, Length];
		InitializeCells();
	}

	private void InitializeCells()
	{
		for (int i = 0; i < Length; i++)
		{
			for (int j = 0; j < Length; j++)
			{
				cells[i, j] = new Cell
				{
					Temperature = 0,
					Position = new Vector3(i, j, 0),
					Name = $"Cell_{Name}_{i}_{j}"
				};
			}
		}
	}

	public Cell Cell(int i, int j, int rotation)
	{
		switch (rotation)
		{
			case 0:
				return cells[i, j];
			case 1:
				return cells[Length - j - 1, i];
			case 2:
				return cells[Length - i - 1, Length - j - 1];
			case 3:
				return cells[j, Length - i - 1];
			default:
				return null;
		}
	}


	public SpaceCells Clone()
	{
		SpaceCells clone = new SpaceCells(Length, Name);
		for (int i = 0; i < Length; i++)
		{
			for (int j = 0; j < Length; j++)
			{
				clone.cells[i, j] = new Cell
				{
					Temperature = cells[i, j].Temperature,
					Position = cells[i, j].Position,
					Name = cells[i, j].Name
				};
			}
		}
		return clone;
	}
}
