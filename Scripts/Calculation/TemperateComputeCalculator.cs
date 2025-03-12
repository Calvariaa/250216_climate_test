using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;
using System.Diagnostics;


public class TemperatureComputeCalculator
{
	private readonly uint Length;
	private readonly double Alpha;


	private float[] LocalCellsList;
	// private uint[] LocalCellsNeighborsList;
	private Vector4I[] LocalCellsNeighborsVector;
	private float LocalDeltaTime;

	private uint GroupSize;

	public ComputeShaderInstance computeShaderInstance;

	public SurfaceAreaCells AreaCells;

	public TemperatureComputeCalculator(string path, uint length, double alpha, SurfaceAreaCells surfaceAreaCells)
	{
		Length = length;
		Alpha = alpha;
		AreaCells = surfaceAreaCells;
		LocalCellsList = new float[length * length * 6];
		// LocalCellsNeighborsList = new uint[length * length * 6 * 4];
		LocalCellsNeighborsVector = new Vector4I[length * length * 6];
		LocalDeltaTime = 0.0f;


		GroupSize = length / 32;

		InitializingLocalList();

		computeShaderInstance = new(path);


		// texture.TextureRdRid = textureRid;


		computeShaderInstance.SetTextureUniform(NewTextureRid(), 0, 0);
		computeShaderInstance.SetTextureUniform(NewTextureRid(), 1, 0);
		computeShaderInstance.SetBuffer(LocalCellsNeighborsVector, 0, 1);
		computeShaderInstance.SetBuffer(LocalDeltaTime, 0, 2);

		computeShaderInstance.SetPushConstant((uint)Length, (float)Alpha);

		computeShaderInstance.InitializeComplete();
	}

	public Rid NewTextureRid()
	{
		var tf = new RDTextureFormat()
		{
			Format = RenderingDevice.DataFormat.R32Sfloat,
			TextureType = RenderingDevice.TextureType.Type2D,
			Width = 256 * 6,
			Height = 256,
			Depth = 1,
			ArrayLayers = 1,
			Mipmaps = 1,
			UsageBits = RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.CanCopyToBit
		};

		var textureRid = computeShaderInstance.RD.TextureCreate(tf, new RDTextureView(), []);


		// Make sure our textures are cleared.
		computeShaderInstance.RD.TextureClear(textureRid, new Color(0, 0, 0, 0), 0, 1, 0, 1);

		return textureRid;
	}



	private void InitializingLocalList()
	{
		// 六个方向的表，orientationTable[currentOrien, targetOrien] = {ioffset, rotation}
		// int[,] orientationTable = new int[6, 6]
		// {
		// 	{0, 1, 2, 3, 4, 5},
		// 	{-1, 0, 1, 2, 3, 4},
		// 	{-2, -1, 0, 1, 2, 3},
		// 	{-3, -2, -1, 0, 1, 2},
		// 	{-4, -3, -2, -1, 0, 1},
		// 	{-5, -4, -3, -2, -1, 0}
		// };

		// 四个方向的表, directionTable[direction, 0] = x, directionTable[direction, 1] = y
		int[,] directionTable = new int[4, 2]
		{
			{0, -1},
			{0, +1},
			{-1, 0},
			{+1, 0}
		};


		// (int)orientation * Length * Length + i * Length + j
		foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		{
			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < Length; j++)
				{
					// 这里应该直接写给纹理
					// LocalCellsList[(int)orientation * Length * Length + i * Length + j] = AreaCells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature;

					var currentVectorIndex = (int)orientation * Length * Length + i * Length + j;
					foreach (AreaDirection direction in Enum.GetValues(typeof(AreaDirection)))
					{
						var currentArrayIndex = (int)orientation * Length * Length * 4 + i * Length * 4 + j * 4 + (int)direction;

						var targetIMov = i + directionTable[(int)direction, 1];
						var targetJMov = j + directionTable[(int)direction, 0];

						var targetNeighbor = AreaCells.surfaceCellNodes[orientation].Neighbors[direction];
						var iOffset = (int)targetNeighbor.Node.Surface.Orientation - (int)orientation;

						long neighborsId;
						// 检测目标移动的位置（direction: 上下左右）有没有超过当前区块的边界
						if (targetIMov >= 0 && targetIMov < Length && targetJMov >= 0 && targetJMov < Length)
						{
							neighborsId = (int)orientation * Length * Length + targetIMov * Length + targetJMov;
							// LocalCellsNeighborsList[currentArrayIndex] = (uint)neighborsId;
							switch (direction)
							{
								case AreaDirection.Up:
									LocalCellsNeighborsVector[currentVectorIndex].X = (int)neighborsId;
									break;
								case AreaDirection.Down:
									LocalCellsNeighborsVector[currentVectorIndex].Y = (int)neighborsId;
									break;
								case AreaDirection.Left:
									LocalCellsNeighborsVector[currentVectorIndex].Z = (int)neighborsId;
									break;
								case AreaDirection.Right:
									LocalCellsNeighborsVector[currentVectorIndex].W = (int)neighborsId;
									break;
							}
						}
						else
						{
							var localTargetI = SurfaceCells.GetRotatedI(i % (int)Length, j, Length, targetNeighbor.Rotation);
							var localTargetJ = SurfaceCells.GetRotatedI(i % (int)Length, j, Length, targetNeighbor.Rotation);

							neighborsId = (int)orientation * Length * Length
							+ (i + iOffset * Length + localTargetI + directionTable[(int)direction, 1] * (1 - Length)) * Length
							+ (j + localTargetJ + directionTable[(int)direction, 0] * (1 - Length));
							// LocalCellsNeighborsList[currentArrayIndex] = (uint)neighborsId;
						}

						switch (direction)
						{
							case AreaDirection.Up:
								LocalCellsNeighborsVector[currentVectorIndex].X = (int)neighborsId;
								break;
							case AreaDirection.Down:
								LocalCellsNeighborsVector[currentVectorIndex].Y = (int)neighborsId;
								break;
							case AreaDirection.Left:
								LocalCellsNeighborsVector[currentVectorIndex].Z = (int)neighborsId;
								break;
							case AreaDirection.Right:
								LocalCellsNeighborsVector[currentVectorIndex].W = (int)neighborsId;
								break;
						}
					}
				}
			}
		}

	}


	public void Calculate(double delta)
	{
		// LocalDeltaTime = (float)delta;

		// // (int)orientation * Length * Length + i * Length + j
		// foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		// {
		// 	for (int i = 0; i < Length; i++)
		// 	{
		// 		for (int j = 0; j < Length; j++)
		// 		{
		// 			LocalCellsList[(int)orientation * Length * Length + i * Length + j] = AreaCells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature;

		// 		}
		// 	}
		// }

		// computeShaderInstance.UpdateBuffer(0, LocalCellsList);
		// computeShaderInstance.UpdateBuffer(2, LocalDeltaTime);
		// computeShaderInstance.Calculate(GroupSize, GroupSize, 6);

		// float[] computeShaderResultArray = computeShaderInstance.GetFloatArrayResult(0);
		// // Print("Output: ", string.Join(",", computeShaderResultArray));
		// foreach (AreaOrientation orientation in Enum.GetValues(typeof(AreaOrientation)))
		// {
		// 	for (int i = 0; i < Length; i++)
		// 	{
		// 		for (int j = 0; j < Length; j++)
		// 		{
		// 			AreaCells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature = computeShaderResultArray[(int)orientation * Length * Length + i * Length + j];
		// 		}
		// 	}
		// }
	}

	public void UpdateCompute()
	{
		computeShaderInstance.Calculate(GroupSize, GroupSize, 6);
	}


	public void ClearCells()
	{

	}
}
