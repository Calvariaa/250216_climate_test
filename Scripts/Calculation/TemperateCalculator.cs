using Godot;
using static Godot.GD;
namespace _Climate.Scripts;

public class TemperatureCalculator(int length, double alpha, SurfaceAreaCells surfaceAreaCells)
{
	public readonly int Length = length;
	public readonly double Alpha = alpha;

	public SurfaceAreaCells AreaCells = surfaceAreaCells;

	public SurfaceAreaCells.SurfaceCellNode CellsNodeLeft = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Left];
	public SurfaceAreaCells.SurfaceCellNode CellsNodeDown = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Down];
	public SurfaceAreaCells.SurfaceCellNode CellsNodeBackward = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Backward];
	public SurfaceAreaCells.SurfaceCellNode CellsNodeRight = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Right];
	public SurfaceAreaCells.SurfaceCellNode CellsNodeUp = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Up];
	public SurfaceAreaCells.SurfaceCellNode CellsNodeForward = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Forward];
	// public SurfaceCells Cells = surfaceAreaCells.surfaceCellNodes[AreaOrientation.Up].Surface;
	private uint _averageCount = 0;

	public void Calculate(double delta)
	{
		var dx2 = 1.0 / ((length - 1) << 1);

		// 辅助函数，用于计算温度分布的导数
		double[,] ComputeHeatEquation(SurfaceAreaCells.SurfaceCellNode cellsNode, double[,] uk, double uk_delta, int width, int height, double dx2, double alpha)
		{
			uk ??= new double[width, height];
			double[,] dTdt = new double[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double d2Tdx2 = 0;
					double d2Tdy2 = 0;

					if (x > 0) d2Tdx2 +=
						cellsNode.Surface.Cell(x - 1, y, 0).Temperature + uk[x - 1, y] * uk_delta;
					if (x < width - 1) d2Tdx2 +=
						cellsNode.Surface.Cell(x + 1, y, 0).Temperature + uk[x + 1, y] * uk_delta;
					if (y > 0) d2Tdy2 +=
						cellsNode.Surface.Cell(x, y - 1, 0).Temperature + uk[x, y - 1] * uk_delta;
					if (y < height - 1) d2Tdy2 +=
						cellsNode.Surface.Cell(x, y + 1, 0).Temperature + uk[x, y + 1] * uk_delta;

					if (x == 0) d2Tdx2 +=
						cellsNode.Neighbors[AreaDirection.Left].Node.Surface.
							Cell(width - 1, y, cellsNode.Neighbors[AreaDirection.Left].Rotation).
								Temperature + uk[width - 1, y] * uk_delta;
					if (x == width - 1) d2Tdx2 +=
						cellsNode.Neighbors[AreaDirection.Right].Node.Surface.
							Cell(0, y, cellsNode.Neighbors[AreaDirection.Right].Rotation).
								Temperature + uk[0, y] * uk_delta;
					if (y == 0) d2Tdy2 +=
						cellsNode.Neighbors[AreaDirection.Up].Node.Surface.
							Cell(x, height - 1, cellsNode.Neighbors[AreaDirection.Up].Rotation).
								Temperature + uk[x, height - 1] * uk_delta;
					if (y == height - 1) d2Tdy2 +=
						cellsNode.Neighbors[AreaDirection.Down].Node.Surface.
							Cell(x, 0, cellsNode.Neighbors[AreaDirection.Down].Rotation).
								Temperature + uk[x, 0] * uk_delta;

					d2Tdx2 -= 2 * (cellsNode.Surface.Cell(x, y, 0).Temperature + (uk[x, y] * uk_delta));
					d2Tdy2 -= 2 * (cellsNode.Surface.Cell(x, y, 0).Temperature + (uk[x, y] * uk_delta));

					dTdt[x, y] = alpha * (d2Tdx2 / (dx2) + d2Tdy2 / (dx2)); // 将矩阵展平成向量
				}
			}

			return dTdt;
		}

		// https://i.imgur.com/RIlJM32.png
		// https://zhuanlan.zhihu.com/p/8616433050

		// https://www.bilibili.com/opus/689834819427762213
		// 看看六阶, 要用
		void rk4(SurfaceAreaCells.SurfaceCellNode cellsNode, double dt, int width, int height, double dx2, double alpha)
		{
			// var T = cellsNode.Surface;

			// 时间积分：使用 Runge-Kutta 方法
			// 小朋友想一想你能不能用你妈隐式方法因为你他妈的算太慢了
			// 计算k1234
			var k1 = ComputeHeatEquation(cellsNode, null, 0, width, height, dx2, alpha);
			var k2 = ComputeHeatEquation(cellsNode, k1, delta / 2, width, height, dx2, alpha);
			var k3 = ComputeHeatEquation(cellsNode, k2, delta / 2, width, height, dx2, alpha);
			var k4 = ComputeHeatEquation(cellsNode, k3, delta, width, height, dx2, alpha);

			// 更新u_i^(n+1)
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					cellsNode.Surface.Cell(x, y, 0).Temperature += dt / 6 * (k1[x, y] + 2 * k2[x, y] + 2 * k3[x, y] + k4[x, y]);
				}
			}

			// return T;
		}

		// 数学逼提醒了我用龙格库塔法求偏微分，让我们赞美数学逼
		rk4(CellsNodeLeft, delta, Length, Length, dx2, Alpha);
		rk4(CellsNodeDown, delta, Length, Length, dx2, Alpha);
		rk4(CellsNodeBackward, delta, Length, Length, dx2, Alpha);
		rk4(CellsNodeRight, delta, Length, Length, dx2, Alpha);
		rk4(CellsNodeUp, delta, Length, Length, dx2, Alpha);
		rk4(CellsNodeForward, delta, Length, Length, dx2, Alpha);


		if (_averageCount < 1000)
			_averageCount++;
	}

	public void ClearCells()
	{
		for (var x = 0; x < Length; x++)
		{
			for (var y = 0; y < Length; y++)
			{
				CellsNodeLeft.Surface.Cell(x, y, 0).Temperature = 0;
				CellsNodeDown.Surface.Cell(x, y, 0).Temperature = 0;
				CellsNodeBackward.Surface.Cell(x, y, 0).Temperature = 0;
				CellsNodeRight.Surface.Cell(x, y, 0).Temperature = 0;
				CellsNodeUp.Surface.Cell(x, y, 0).Temperature = 0;
				CellsNodeForward.Surface.Cell(x, y, 0).Temperature = 0;
			}
		}
	}
}
