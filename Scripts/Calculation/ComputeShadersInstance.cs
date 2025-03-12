using Godot;
using System;
using static Godot.GD;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 这应该是一个被封装好的计算着色器
/// 使用构造函数生成一个实例
/// 使用 SetBuffer 来输入数据
/// 使用 InitializeComplete 来告知初始化完成
/// 使用 Calculate 来进行计算
/// </summary>
public class ComputeShaderInstance
{

	public RenderingDevice RD;
	private Rid ComputeShader;
	private Rid ComputePipeline;

	private Dictionary<(uint set, int binding), Rid> Buffers = [];
	private Dictionary<Rid, RenderingDevice.UniformType> UniformType = [];
	private Dictionary<uint, Rid> UniformSet = [];

	private byte[] PushConstant = [];

	/// <summary>
	/// 构造函数,用于生成实例
	/// </summary>
	/// <param name="path">计算着色器的路径</param>
	/// <returns></returns>
	public ComputeShaderInstance(string path) => InitializeShader(path);

	/// <summary>
	/// 初始化计算着色器
	/// </summary>
	/// <param name="path">计算着色器的路径</param>
	private void InitializeShader(string path)
	{
		//加载着色器
		RD = RenderingServer.CreateLocalRenderingDevice();

		RDShaderFile ComputeShaderFile = Load<RDShaderFile>(path);
		if (ComputeShaderFile == null)
			throw new ArgumentException($"ComputeShaderInstance/InitializeShader: 无法加载 ComputeShader, 文件路径: {path}");

		RDShaderSpirV shaderBytecode = ComputeShaderFile.GetSpirV();
		ComputeShader = RD.ShaderCreateFromSpirV(shaderBytecode);

		ComputePipeline = RD.ComputePipelineCreate(ComputeShader);
	}

	/// <summary>
	/// 你应该通过调用多次这个函数来输入数据
	/// </summary>
	/// <param name="data">这就是数据本身了</param>
	/// <param name="set">Uniform Set</param>
	/// <param name="binding">Uniform Binding</param>
	/// <typeparam name="T"></typeparam>
	public void SetBuffer<T>(T data, uint set, int binding)
	{
		byte[] bytes = Tool.ConvertToByteArray(data);
		var rid = RD.StorageBufferCreate((uint)bytes.Length, bytes);
		// GD.Print("Output: ", string.Join(", ", rid));
		UniformType[rid] = RenderingDevice.UniformType.StorageBuffer;
		Buffers.Add((set, binding), rid);
		// Buffers[(set, binding)] = rid;
	}


	// 更新现有缓冲区内容
	public void UpdateBuffer<T>(T data, uint set, int binding)
	{
		if (binding < 0 || binding >= Buffers.Count)
			throw new IndexOutOfRangeException($"无效的缓冲区索引: {binding}");

		Rid buffer = Buffers[(set, binding)];
		byte[] bytes = Tool.ConvertToByteArray(data);

		// 确保GPU操作完成

		RD.BufferUpdate(
			buffer: buffer,
			offset: 0,
			sizeBytes: (uint)bytes.Length,
			data: bytes
		);
		RD.Submit();
		RD.Sync();
	}

	/// <summary>
	/// 你可以通过这个函数来输入推式常量
	/// 我还是没有测试过,嘻嘻
	/// 实际上我也不知道最大可以输入多少,但我觉得应该是128byte
	/// </summary>
	/// <param name="objects">输入的数据</param>
	/// <typeparam name="T">匹配: unmanaged</typeparam>
	public void SetPushConstant<T>(params T[] objects) where T : unmanaged
	{
		byte[] bytes = Tool.ConvertToByteArray(objects);
		if (bytes.Length >= 16 && bytes.Length <= 128)
		{
			PushConstant = bytes;
		}
		else if (bytes.Length > 0 && bytes.Length < 16)
		{
			byte[] completion = new byte[16];
			for (var i = 0; i < completion.Length; i++)
			{
				if (i < bytes.Length)
					completion[i] = bytes[i];
				else
					completion[i] = 0;
			}
			PushConstant = completion;
		}
		else
			Print($"ComputeShaderInstance/SetPushConstant:那你输进来的东西超过推式常量的限制,到底输入的是个啥呢: {objects} 还有转换后的东西: {bytes}");
	}

	/// <summary>
	/// 清空你输入的推式常量
	/// </summary>
	public void ClearPushConstant()
	{
		PushConstant = [];
	}

	public void SetUniform()
	{

	}

	/// <summary>
	/// 设置纹理 Uniform
	/// </summary>
	/// <param name="textureRid">纹理的 RID</param>
	/// <param name="set">Uniform Set</param>
	/// <param name="binding">Uniform Binding</param>
	public void SetTextureUniform(Rid textureRid, uint set, int binding)
	{
		UniformType[textureRid] = RenderingDevice.UniformType.Image;
		// Buffers.Add((set, binding), textureRid);
		Buffers[(set, binding)] = textureRid;
	}

	/// <summary>
	/// 当输入完所有输入数据后调用这个写入GPU
	/// </summary>
	public void InitializeComplete()
	{
		Dictionary<uint, Dictionary<int, RDUniform>> uniforms = [];
		foreach (var buffer in Buffers)
		{
			uint set = buffer.Key.set;
			int binding = buffer.Key.binding;

			if (!uniforms.ContainsKey(set))
				uniforms[set] = [];

			RDUniform rdUniform = new RDUniform
			{
				UniformType = UniformType[buffer.Value],
				Binding = binding,
			};

			uniforms[set][binding] = rdUniform;
			uniforms[set][binding].AddId(buffer.Value);

			// Print("uniforms: ", string.Join(",", uniforms));
		}

		foreach (var kvp in uniforms)
		{
			uint setKey = kvp.Key;
			// 排序后生成数组，确保 Binding 顺序一致
			Godot.Collections.Array<RDUniform> uniformArray = new Godot.Collections.Array<RDUniform>(
				kvp.Value.OrderBy(p => p.Key).Select(p => p.Value).ToArray()
			);

			UniformSet[setKey] = RD.UniformSetCreate(uniformArray, ComputeShader, setKey);
			// Print(UniformSet[setKey]);
		}

	}

	/// <summary>
	/// 运行计算,请确保工作组与线程组符合数据规模
	/// </summary>
	/// <param name="GroupSizeX">工作组X</param>
	/// <param name="GroupSizeY">工作组Y</param>
	/// <param name="GroupSizeZ">工作组Z</param>
	public void Calculate(uint GroupSizeX, uint GroupSizeY, uint GroupSizeZ)
	{
		long computeList = RD.ComputeListBegin();
		RD.ComputeListBindComputePipeline(computeList, ComputePipeline);

		foreach (var item in UniformSet)
			RD.ComputeListBindUniformSet(computeList, item.Value, item.Key);

		if (PushConstant.Length > 0)
			RD.ComputeListSetPushConstant(computeList, PushConstant, (uint)PushConstant.Length);

		RD.ComputeListDispatch(computeList, GroupSizeX, GroupSizeY, GroupSizeZ);
		RD.ComputeListEnd();
		RD.Submit();
		RD.Sync();
	}

	public float[] GetFloatArrayResult(uint set, int binding)
	{
		var outputBytes = RD.BufferGetData(Buffers[(set, binding)]);
		float[] result = new float[outputBytes.Length / sizeof(float)];
		Buffer.BlockCopy(outputBytes, 0, result, 0, outputBytes.Length);
		return result;
	}

	public Rid GetBufferRid(uint set, int binding)
	{
		return Buffers[(set, binding)];
	}

	// public Rid GetBufferRid(uint set, int binding)
	// {
	// 	return UniformSet[(set, binding)];
	// }

}

public class Tool
{

	/// <summary>
	/// 将数据转换为字节数组
	/// </summary>
	/// <typeparam name="T">数据类型</typeparam>
	/// <param name="data">待转换的数据（不能为空）</param>
	/// <returns>转换后的字节数组</returns>
	public static byte[] ConvertToByteArray<T>(T data) => data switch
	{
		null => throw new ArgumentNullException(nameof(data)),

		int intByte => BitConverter.GetBytes(intByte),
		uint uintByte => BitConverter.GetBytes(uintByte),
		float floatByte => BitConverter.GetBytes(floatByte),
		double doubleByte => BitConverter.GetBytes(doubleByte),
		bool boolByte => BitConverter.GetBytes(boolByte),

		int[] intArray => IntArrayToBytes(intArray),
		uint[] uintArray => UintArrayToBytes(uintArray),
		float[] floatArray => FloatArrayToBytes(floatArray),

		Vector2 vector2Byte => Vector2ToBytes(vector2Byte),
		Vector2I vector2IByte => Vector2IToBytes(vector2IByte),
		Vector3 vector3Byte => Vector3ToBytes(vector3Byte),
		Vector3I vector3IByte => Vector3IToBytes(vector3IByte),
		Vector4 vector4Byte => Vector4ToBytes(vector4Byte),
		Vector4I vector4IByte => Vector4IToBytes(vector4IByte),

		Vector2[] vector2Array => Vector2ArrayToBytes(vector2Array),
		Vector2I[] vector2IArray => Vector2IArrayToBytes(vector2IArray),
		Vector3[] vector3Array => Vector3ArrayToBytes(vector3Array),
		Vector3I[] vector3IArray => Vector3IArrayToBytes(vector3IArray),
		Vector4[] vector4Array => Vector4ArrayToBytes(vector4Array),
		Vector4I[] vector4IArray => Vector4IArrayToBytes(vector4IArray),
		_ => HandleComplexType(data),
	};

	/// <summary>
	/// 将一组 float 数值转换为字节数组，每个 float 占 4 字节
	/// </summary>
	private static byte[] GetBytesFromFloats(params float[] values)
	{
		byte[] result = new byte[values.Length * 4];
		for (int i = 0; i < values.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(values[i]), 0, result, i * 4, 4);

			//我不确定这玩意是否能用,先注释了
			// Buffer.BlockCopy(values, 0, result, 0, result.Length);
		}
		return result;
	}

	/// <summary>
	/// 将一组 int 数值转换为字节数组，每个 int 占 4 字节
	/// </summary>
	private static byte[] GetBytesFromInts(params int[] values)
	{
		byte[] result = new byte[values.Length * 4];
		for (int i = 0; i < values.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(values[i]), 0, result, i * 4, 4);

			//我不确定这玩意是否能用,先注释了
			// Buffer.BlockCopy(values, 0, result, 0, result.Length);
		}
		return result;
	}

	private static byte[] Vector2ToBytes(Vector2 vector2Byte) => GetBytesFromFloats(vector2Byte.X, vector2Byte.Y);
	private static byte[] Vector2IToBytes(Vector2I vector2IByte) => GetBytesFromInts(vector2IByte.X, vector2IByte.Y);
	private static byte[] Vector3ToBytes(Vector3 vector3Byte) => GetBytesFromFloats(vector3Byte.X, vector3Byte.Y, vector3Byte.Z);
	private static byte[] Vector3IToBytes(Vector3I vector3IByte) => GetBytesFromInts(vector3IByte.X, vector3IByte.Y, vector3IByte.Z);
	private static byte[] Vector4ToBytes(Vector4 vector4Byte) => GetBytesFromFloats(vector4Byte.X, vector4Byte.Y, vector4Byte.Z, vector4Byte.W);
	private static byte[] Vector4IToBytes(Vector4I vector4IByte) => GetBytesFromInts(vector4IByte.X, vector4IByte.Y, vector4IByte.Z, vector4IByte.W);

	private static byte[] IntArrayToBytes(int[] intArray)
	{
		byte[] bytes = new byte[intArray.Length * sizeof(int)];
		Buffer.BlockCopy(intArray, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private static byte[] UintArrayToBytes(uint[] uintArray)
	{
		byte[] bytes = new byte[uintArray.Length * sizeof(uint)];
		Buffer.BlockCopy(uintArray, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private static byte[] FloatArrayToBytes(float[] floatArray)
	{
		byte[] bytes = new byte[floatArray.Length * sizeof(float)];
		Buffer.BlockCopy(floatArray, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private static byte[] Vector2ArrayToBytes(Vector2[] vector2Array)
	{
		byte[] bytes = new byte[vector2Array.Length * 8];
		for (int i = 0; i < vector2Array.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector2Array[i].X), 0, bytes, i * 8 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector2Array[i].Y), 0, bytes, i * 8 + 4, 4);
		}
		return bytes;
	}

	private static byte[] Vector2IArrayToBytes(Vector2I[] vector2IArray)
	{
		byte[] bytes = new byte[vector2IArray.Length * 8];
		for (int i = 0; i < vector2IArray.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector2IArray[i].X), 0, bytes, i * 8 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector2IArray[i].Y), 0, bytes, i * 8 + 4, 4);
		}
		return bytes;
	}

	private static byte[] Vector3ArrayToBytes(Vector3[] vector3Array)
	{
		byte[] bytes = new byte[vector3Array.Length * 12];
		for (int i = 0; i < vector3Array.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector3Array[i].X), 0, bytes, i * 12 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector3Array[i].Y), 0, bytes, i * 12 + 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector3Array[i].Z), 0, bytes, i * 12 + 8, 4);
		}
		return bytes;
	}

	private static byte[] Vector3IArrayToBytes(Vector3I[] vector3IArray)
	{
		byte[] bytes = new byte[vector3IArray.Length * 12];
		for (int i = 0; i < vector3IArray.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector3IArray[i].X), 0, bytes, i * 12 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector3IArray[i].Y), 0, bytes, i * 12 + 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector3IArray[i].Z), 0, bytes, i * 12 + 8, 4);
		}
		return bytes;
	}

	private static byte[] Vector4ArrayToBytes(Vector4[] vector4Array)
	{
		byte[] bytes = new byte[vector4Array.Length * 16];
		for (int i = 0; i < vector4Array.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector4Array[i].X), 0, bytes, i * 16 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4Array[i].Y), 0, bytes, i * 16 + 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4Array[i].Z), 0, bytes, i * 16 + 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4Array[i].W), 0, bytes, i * 16 + 12, 4);
		}
		return bytes;
	}

	private static byte[] Vector4IArrayToBytes(Vector4I[] vector4IArray)
	{
		byte[] bytes = new byte[vector4IArray.Length * 16];
		for (int i = 0; i < vector4IArray.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(vector4IArray[i].X), 0, bytes, i * 16 + 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4IArray[i].Y), 0, bytes, i * 16 + 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4IArray[i].Z), 0, bytes, i * 16 + 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(vector4IArray[i].W), 0, bytes, i * 16 + 12, 4);
		}
		return bytes;
	}

	/// <summary>
	/// 处理复杂类型的转换，目前未实现
	/// </summary>
	private static byte[] HandleComplexType<T>(T data)
	{
		Print("你知道吗?List应当被转换为Array!");
		Print("使用Linq的.Select(ci => ci.index).ToArray()功能!");
		throw new NotImplementedException(nameof(data));
	}

}
