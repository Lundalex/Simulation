using Unity.Mathematics;
using UnityEngine;

// Import utils from Resources.cs
using Resources;
public static class ComputeHelper
{

// --- KERNEL DISPATCH ---

    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int)threadsNum, threadSize</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int threadsNum, int threadSize)
    {
        int threadGroupsNum = Utils.GetThreadGroupsNum(threadsNum, threadSize);
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupsNum, 1, 1);
    }
    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int2)threadsNum, threadSize</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int2 threadsNum, int threadSize)
    {
        int2 threadGroupsNums = Utils.GetThreadGroupsNum(threadsNum, threadSize);
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupsNums.x, threadGroupsNums.y, 1);
    }
    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int3)threadsNum, threadSize</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int3 threadsNum, int threadSize)
    {
        int3 threadGroupNums = Utils.GetThreadGroupsNum(threadsNum, threadSize);
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupNums.x, threadGroupNums.y, threadGroupNums.z);
    }
    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int)threadGroupsNum</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int threadGroupsNum)
    {
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupsNum, 1, 1);
    }
    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int2)threadGroupsNum</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int2 threadGroupsNums)
    {
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupsNums.x, threadGroupsNums.y, 1);
    }
    /// <summary>Dispatch a shader kernel</summary>
    /// <remarks>Uses (int3)threadGroupsNum</remarks>
    static public void DispatchKernel (ComputeShader cs, string kernelName, int3 threadGroupsNums)
    {
        cs.Dispatch(cs.FindKernel(kernelName), threadGroupsNums.x, threadGroupsNums.y, threadGroupsNums.z);
    }


// --- CREATE BUFFERS ---

    /// <summary>Create an append buffer</summary>
    /// <returns>Without ref</returns>
	public static ComputeBuffer CreateAppendBuffer<T>(int capacity) // T is the buffer struct
	{
		int stride = GetStride<T>();
		ComputeBuffer buffer = new ComputeBuffer(capacity, stride, ComputeBufferType.Append);
		buffer.SetCounterValue(0);
		return buffer;
	}
    /// <summary>Create an append buffer</summary>
    /// <returns>-> ref buffer</returns>
	public static void CreateAppendBuffer<T>(ref ComputeBuffer buffer, int capacity) // T is the buffer struct
	{
		int stride = GetStride<T>();
        buffer ??= new ComputeBuffer(capacity, stride, ComputeBufferType.Append);
		buffer.SetCounterValue(0);
	}
    /// <summary>Create a structured buffer</summary>
    /// <returns>Without ref</returns>
	public static ComputeBuffer CreateStructuredBuffer<T>(T[] data) // T is the buffer struct
	{
		var buffer = new ComputeBuffer(data.Length, GetStride<T>());
		buffer.SetData(data);
		return buffer;
	}
    /// <summary>Create a structured buffer</summary>
    /// <returns>-> ref buffer</returns>
	public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data) // T is the buffer struct
	{
		buffer ??= new ComputeBuffer(data.Length, GetStride<T>());
		buffer.SetData(data);
	}
    /// <summary>Create a structured buffer</summary>
    /// <returns>Without ref</returns>
	public static ComputeBuffer CreateStructuredBuffer<T>(int count) // T is the buffer struct
	{
		var buffer = new ComputeBuffer(count, GetStride<T>());
		return buffer;
	}
    /// <summary>Create a structured buffer</summary>
    /// <returns>-> ref buffer</returns>
	public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count) // T is the buffer struct
	{
		buffer = new ComputeBuffer(count, GetStride<T>());
	}
    /// <summary>Create a count buffer</summary>
    /// <returns>Without ref</returns>
    public static ComputeBuffer CreateCountBuffer()
    {
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        return countBuffer;
    }
    /// <summary>Create a count buffer</summary>
    /// <returns>-> ref countBuffer</returns>
    public static void CreateCountBuffer(ref ComputeBuffer countBuffer)
    {
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }


// --- GET APPEND BUFFER COUNT ---

    /// <summary>Get append buffer count</summary>
    /// <remarks>Uses an countBuffer</remarks>
	public static int GetAppendBufferCount(ComputeBuffer buffer, ComputeBuffer countBuffer)
	{
        ComputeBuffer.CopyCount(buffer, countBuffer, 0);
        int[] countArr = new int[1];
        countBuffer.GetData(countArr);
        int count = countArr[0];
        return count;
	}
    /// <summary>Get append buffer count</summary>
    /// <remarks>Does not use an countBuffer</remarks>
	public static int GetAppendBufferCount(ComputeBuffer buffer)
	{
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(buffer, countBuffer, 0);
        int[] countArr = new int[1];
        countBuffer.GetData(countArr);
        int count = countArr[0];
        return count;
	}


// --- RELEASE BUFFERS / TEXTURES ---

    /// <summary>Releases a single compute buffer</summary>
	public static void Release(ComputeBuffer buffer)
	{
		buffer?.Release(); // ComputeBuffer class passed by reference automatically
	}
    /// <summary>Releases multiple compute buffer</summary>
    public static void Release(params ComputeBuffer[] buffers)
	{
        for (int i = 0; i < buffers.Length; i++)
        {
            Release(buffers[i]);
        }
	}
    /// <summary>Releases a single render texture</summary>
	public static void Release(RenderTexture texture)
	{
		if (texture != null)
		{
			texture.Release(); // RenderTexture class passed by reference automatically
		}
	}


// --- CLASS ---

    /// <returns>The combined stride (size in bytes) of a struct/datatype</returns>
    public static int GetStride<T>() => System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
}