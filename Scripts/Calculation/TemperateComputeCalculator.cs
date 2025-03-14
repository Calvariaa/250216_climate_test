using System;
using Godot;
using static Godot.GD;
using _Climate.Scripts;
using System.Diagnostics;


public class TemperatureComputeCalculator
{
	private readonly uint Length;
	private readonly double Alpha;


	public float[] LocalCellsList;
	// private uint[] LocalCellsNeighborsList;

	public Rid LocalCellsTextureIn, LocalCellsTextureOut;
	private Vector4I[] LocalCellsNeighborsVector;
	private float LocalDeltaTime;

	private uint GroupSize;

	public ComputeShaderInstance computeShaderInstance;

	public SurfaceAreaCells AreaCells;

	private Texture2Drd texture;

	public TemperatureComputeCalculator(string path, ShaderMaterial materialShader, uint length, double alpha, SurfaceAreaCells surfaceAreaCells)
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

		texture = (Texture2Drd)materialShader.GetShaderParameter("temperature_texture");
		texture.TextureRdRid = LocalCellsTextureOut;

		LocalCellsTextureIn = NewTextureRid();
		LocalCellsTextureOut = NewTextureRid();
		computeShaderInstance.SetTextureUniform(LocalCellsTextureIn, 0, 0);
		computeShaderInstance.SetTextureUniform(LocalCellsTextureOut, 1, 0);
		computeShaderInstance.SetBuffer(LocalCellsNeighborsVector, 0, 1);
		computeShaderInstance.SetBuffer(LocalDeltaTime, 0, 2);
		computeShaderInstance.SetBuffer(LocalCellsList, 0, 3);

		computeShaderInstance.SetPushConstant((int)Length);
		computeShaderInstance.SetPushConstant((float)Alpha);

		computeShaderInstance.InitializeComplete();



		Print("LocalCellsTextureIn: ", string.Join(", ", (computeShaderInstance.GetBufferRid(0, 0))));
		Print("LocalCellsTextureOut: ", string.Join(", ", (computeShaderInstance.GetBufferRid(1, 0))));
	}

	public Rid NewTextureRid()
	{
		var tf = new RDTextureFormat()
		{
			Format = RenderingDevice.DataFormat.R32Sfloat,
			TextureType = RenderingDevice.TextureType.Type2D,
			Width = Length * 6,
			Height = Length,
			Depth = 1,
			ArrayLayers = 1,
			Mipmaps = 1,
			UsageBits = RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.CanCopyToBit
		};

		var textureRid = computeShaderInstance.RD.TextureCreate(tf, new RDTextureView(), []);

		computeShaderInstance.RD.TextureClear(textureRid, new Color(0, 0, 0, 0), 0, 1, 0, 1);

		var textureUpdateState = computeShaderInstance.RD.TextureUpdate(textureRid, 0, Tool.ConvertToByteArray(LocalCellsList));

		Print("textureUpdateState: ", string.Join(", ", (textureRid, textureUpdateState)));


		// Make sure our textures are cleared.

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
					LocalCellsList[(int)orientation * Length * Length + i * Length + j] = AreaCells.surfaceCellNodes[orientation].Surface.Cell(i, j, 0).Temperature;

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
		computeShaderInstance.Calculate(GroupSize, GroupSize, 6);

		// float[] computeShaderResultArray = computeShaderInstance.GetFloatArrayResult(0, 3);
		// Print("Output: ", string.Join(",", computeShaderResultArray));

		// if (texture != null)
		// {
		// 	Print("texture: ", texture);
		// 	texture.TextureRdRid = LocalCellsTextureOut;
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
