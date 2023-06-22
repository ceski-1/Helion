using Helion.Render.OpenGL.Context;
using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;

namespace Helion.Render.OpenGL.Buffer.Array;

public abstract class ArrayBufferObject<T> : BufferObject<T> where T : struct
{
    protected override BufferTarget Target => BufferTarget.ArrayBuffer;
    protected abstract BufferUsageHint Hint { get; }

    protected ArrayBufferObject(string objectLabel, int capacity = BufferObject<T>.DefaultCapacity) : base(objectLabel, capacity)
    {
    }

    protected override void PerformUpload()
    {
        GL.BufferData(Target, BytesPerElement * Data.Length, Data.Data, Hint);
    }

    protected override void BufferSubData(int index, int length)
    {
        IntPtr offset = new(BytesPerElement * index);
        int size = BytesPerElement * length;
        IntPtr ptr = GetVboArray() + (BytesPerElement * index);

        GL.BufferSubData(Target, offset, size, ptr);
    }
}
