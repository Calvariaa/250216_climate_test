using Godot;
using System;
using Godot.Collections;
using static Godot.GD;
using System.Collections.Generic;


/// <summary>
/// 这应该是一个被封装好的计算着色器
/// 使用构造函数生成一个实例
/// </summary>
public class ComputeShaderInstance
{

	private RenderingDevice RD;
	private Rid ComputeShader;
	private Rid ComputePipeline;

	private List<Rid> Buffers = [];
	private Rid UniformSet;

	/// <summary>
	/// 构造函数,用于生成实例
	/// </summary>
	/// <param name="path">计算着色器的路径</param>
	/// <param name="inputs">输入的参数,使用元组,List[(数据类型,数据),xxx],目前支持的类型:int,float,int[],float[],vector4i[]</param>
	/// <returns></returns>
	public ComputeShaderInstance(string path, List<(Type, object)> inputs)
	{
		if (inputs is null)
			Print("ComputeShaderInstance/ctor/inputs:卧槽为啥这个你还能输进来个空值你是人类吗");

		InitializeShader(path);
		InitializeBuffer(inputs);
	}

	private void InitializeBuffer(List<(Type, object)> inputs)
	{
		Array<RDUniform> uniforms = [];
		for (int i = 0; i < inputs.Count; i++)
		{
			switch (inputs[i].Item1)
			{
				case Type t when t == typeof(float):
					Buffers.Add(FloatToBuffer((float)inputs[i].Item2));
					break;
				case Type t when t == typeof(float[]):
					Buffers.Add(FloatArrayToBuffer((float[])inputs[i].Item2));
					break;
				case Type t when t == typeof(int):
					Buffers.Add(IntToBuffer((int)inputs[i].Item2));
					break;
				case Type t when t == typeof(int[]):
					Buffers.Add(IntArrayToBuffer((int[])inputs[i].Item2));
					break;
				case Type t when t == typeof(uint):
					Buffers.Add(UIntToBuffer((uint)inputs[i].Item2));
					break;
				case Type t when t == typeof(Vector4I[]):
					Buffers.Add(Vector4IArrayToBuffer((Vector4I[])inputs[i].Item2));
					break;
				default:
					throw new ArgumentException($"ComputeShaderInstance/InitializeBuffer:未定义的类型,请写入解释函数喵,类型: {inputs[i].Item1}");
			}

			uniforms.Add(new RDUniform
			{
				UniformType = RenderingDevice.UniformType.StorageBuffer,
				Binding = i,
			});
			uniforms[i].AddId(Buffers[i]);

		}

		UniformSet = RD.UniformSetCreate(uniforms, ComputeShader, 0);
	}

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

	// 更新现有缓冲区内容
	public void UpdateBuffer<T>(int bufferIndex, T data)
	{
		if (bufferIndex < 0 || bufferIndex >= Buffers.Count)
			throw new IndexOutOfRangeException($"无效的缓冲区索引: {bufferIndex}");

		Rid buffer = Buffers[bufferIndex];
		byte[] bytes = SerializeData(data);

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

	// 通用数据序列化方法
	private byte[] SerializeData<T>(T data)
	{
		if (data is float f)
			return BitConverter.GetBytes(f);
		if (data is float[] fArr)
			return FloatArrayToBytes(fArr);
		if (data is int i)
			return BitConverter.GetBytes(i);
		if (data is int[] iArr)
			return IntArrayToBytes(iArr);
		if (data is Vector4I vec4i)
			return Vector4IToBytes(vec4i);
		if (data is Vector4I[] vec4iArr)
			return Vector4IArrayToBytes(vec4iArr);

		throw new NotSupportedException($"不支持的类型: {typeof(T).Name}");
	}

	// 各种类型序列化方法
	private byte[] FloatArrayToBytes(float[] data)
	{
		byte[] bytes = new byte[data.Length * sizeof(float)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private byte[] IntArrayToBytes(int[] data)
	{
		byte[] bytes = new byte[data.Length * sizeof(int)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private byte[] Vector4IToBytes(Vector4I data)
	{
		byte[] bytes = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(data.X), 0, bytes, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(data.Y), 0, bytes, 4, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(data.Z), 0, bytes, 8, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(data.W), 0, bytes, 12, 4);
		return bytes;
	}

	private byte[] Vector4IArrayToBytes(Vector4I[] data)
	{
		byte[] bytes = new byte[data.Length * 16];
		for (int i = 0; i < data.Length; i++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(data[i].X), 0, bytes, i * 16, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(data[i].Y), 0, bytes, i * 16 + 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(data[i].Z), 0, bytes, i * 16 + 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(data[i].W), 0, bytes, i * 16 + 12, 4);
		}
		return bytes;
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
		RD.ComputeListBindUniformSet(computeList, UniformSet, 0);
		RD.ComputeListDispatch(computeList, GroupSizeX, GroupSizeY, GroupSizeZ);
		RD.ComputeListEnd();
		RD.Submit();
		RD.Sync();
	}

	//这里写的有问题,请更改
	//更改了,但是对吗?

	/// <summary>
	/// 用于获取float数组输出
	/// </summary>
	/// <param name="id">id为输入列表的序号</param>
	/// <returns></returns>
	public float[] GetFloatArrayResult(int id)
	{
		var outputBytes = RD.BufferGetData(Buffers[id]);
		float[] result = new float[outputBytes.Length / sizeof(float)];
		Buffer.BlockCopy(outputBytes, 0, result, 0, outputBytes.Length);
		return result;
	}


	/// <summary>
	/// 用于获取int数组输出
	/// </summary>
	/// <param name="id">id为输入列表的序号</param>
	/// <returns></returns>
	public int[] GetIntArrayResult(int id)
	{
		var outputBytes = RD.BufferGetData(Buffers[id]);
		int[] result = new int[outputBytes.Length / sizeof(int)];
		Buffer.BlockCopy(outputBytes, 0, result, 0, outputBytes.Length);
		return result;
	}



	private Rid FloatToBuffer(float data)
	{
		byte[] floatBytes = BitConverter.GetBytes(data);
		return RD.StorageBufferCreate((uint)floatBytes.Length, floatBytes);
	}

	private Rid FloatArrayToBuffer(float[] data)
	{
		byte[] bytes = new byte[data.Length * sizeof(float)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
		return RD.StorageBufferCreate((uint)bytes.Length, bytes);
	}

	private Rid IntToBuffer(int data)
	{
		byte[] bytes = new byte[sizeof(int)];
		byte[] intBytes = BitConverter.GetBytes(data);
		Buffer.BlockCopy(intBytes, 0, bytes, 0, intBytes.Length);
		return RD.StorageBufferCreate((uint)bytes.Length, bytes);
	}

	private Rid UIntToBuffer(uint data)
	{
		byte[] bytes = new byte[sizeof(uint)];
		byte[] uintBytes = BitConverter.GetBytes(data);
		Buffer.BlockCopy(uintBytes, 0, bytes, 0, uintBytes.Length);
		return RD.StorageBufferCreate((uint)bytes.Length, bytes);
	}

	private Rid IntArrayToBuffer(int[] data)
	{
		byte[] bytes = new byte[data.Length * sizeof(int)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
		return RD.StorageBufferCreate((uint)bytes.Length, bytes);
	}

	private Rid Vector4IArrayToBuffer(Vector4I[] data)
	{
		// 注意：确保Vector4I的内存布局与shader的uvec4一致
		// 每个Vector4I占16字节
		byte[] bytes = new byte[data.Length * 16];
		for (int i = 0; i < data.Length; i++)
		{
			byte[] x = BitConverter.GetBytes(data[i].X);
			byte[] y = BitConverter.GetBytes(data[i].Y);
			byte[] z = BitConverter.GetBytes(data[i].Z);
			byte[] w = BitConverter.GetBytes(data[i].W);

			Buffer.BlockCopy(x, 0, bytes, i * 16 + 0, 4);
			Buffer.BlockCopy(y, 0, bytes, i * 16 + 4, 4);
			Buffer.BlockCopy(z, 0, bytes, i * 16 + 8, 4);
			Buffer.BlockCopy(w, 0, bytes, i * 16 + 12, 4);
		}

		return RD.StorageBufferCreate((uint)bytes.Length, bytes);
	}

	public Rid GetBuffer(int index)
	{
		return Buffers[index];
	}
}
