using Godot;
using static Godot.GD;
namespace _Climate.Scripts;

public class TemperatureCalculator(int length, double alpha, SurfaceCells cells)
{
	public readonly int Length = length;
	public readonly double Alpha = alpha;
	public SurfaceCells Cells = cells;
	private uint _averageCount = 0;

	public void Calculate(double delta)
	{
		var dx2 = 1.0 / ((length - 1) << 1);

		// 辅助函数，用于计算温度分布的导数
		double[,] ComputeHeatEquation(SurfaceCells cells, double[,] uk, double uk_delta, int width, int height, double dx2, double alpha)
		{
			uk ??= new double[width, height];
			double[,] dTdt = new double[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double d2Tdx2 = 0;
					double d2Tdy2 = 0;

					if (x > 0) d2Tdx2 += cells.Cell(x - 1, y, 0).Temperature + uk[x - 1, y] * uk_delta;
					if (x < width - 1) d2Tdx2 += cells.Cell(x + 1, y, 0).Temperature + uk[x + 1, y] * uk_delta;
					if (y > 0) d2Tdy2 += cells.Cell(x, y - 1, 0).Temperature + uk[x, y - 1] * uk_delta;
					if (y < height - 1) d2Tdy2 += cells.Cell(x, y + 1, 0).Temperature + uk[x, y + 1] * uk_delta;

					if (x == 0) d2Tdx2 += cells.Cell(width - 1, y, 0).Temperature + uk[width - 1, y] * uk_delta;
					if (x == width - 1) d2Tdx2 += cells.Cell(0, y, 0).Temperature + uk[0, y] * uk_delta;
					if (y == 0) d2Tdy2 += cells.Cell(x, height - 1, 0).Temperature + uk[x, height - 1] * uk_delta;
					if (y == height - 1) d2Tdy2 += cells.Cell(x, 0, 0).Temperature + uk[x, 0] * uk_delta;

					d2Tdx2 -= 2 * (cells.Cell(x, y, 0).Temperature + (uk[x, y] * uk_delta));
					d2Tdy2 -= 2 * (cells.Cell(x, y, 0).Temperature + (uk[x, y] * uk_delta));

					dTdt[x, y] = alpha * (d2Tdx2 / (dx2) + d2Tdy2 / (dx2)); // 将矩阵展平成向量
				}
			}

			return dTdt;
		}

		// https://i.imgur.com/RIlJM32.png
		// https://zhuanlan.zhihu.com/p/8616433050
		SurfaceCells rk4(SurfaceCells cells, double dt, int width, int height, double dx2, double alpha)
		{
			var T = cells;

			// 时间积分：使用 Runge-Kutta 方法
			// 小朋友想一想你能不能用你妈隐式方法因为你他妈的算太慢了
			// 计算k1234
			var k1 = ComputeHeatEquation(cells, null, 0, width, height, dx2, alpha);
			var k2 = ComputeHeatEquation(cells, k1, delta / 2, width, height, dx2, alpha);
			var k3 = ComputeHeatEquation(cells, k2, delta / 2, width, height, dx2, alpha);
			var k4 = ComputeHeatEquation(cells, k3, delta, width, height, dx2, alpha);

			// 更新u_i^(n+1)
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					T.Cell(x, y, 0).Temperature += dt / 6 * (k1[x, y] + 2 * k2[x, y] + 2 * k3[x, y] + k4[x, y]);
				}
			}

			return T;
		}

		// 数学逼提醒了我用龙格库塔法求偏微分，让我们赞美数学逼
		Cells = rk4(Cells, delta, Length, Length, dx2, Alpha);


		if (_averageCount < 1000)
			_averageCount++;
	}

	public void ClearCells()
	{
		for (var x = 0; x < Length; x++)
		{
			for (var y = 0; y < Length; y++)
			{
				Cells.Cell(x, y, 0).Temperature = 0;
			}
		}
	}
}
