// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Dependency;
using Canopy.Extensions;
using Silk.NET.OpenGL;

namespace Canopy.Graphics;

public class VertexBatch<T> : IDisposable where T : unmanaged
{
    private readonly GL gl;
    private readonly uint vao, vbo, ebo;
    private readonly T[] vertexArray;
    private readonly int maxVertices;
    private int vertexIndex;

    public VertexBatch(GL gl, int maxQuads = 1000)
    {
        this.gl = gl;
        maxVertices = maxQuads * 4;
        vertexArray = new T[maxVertices];

        gl.GenVertexArrays(1, out vao);
        gl.BindVertexArray(vao);
        gl.CheckError("Generate and bind vao array");

        gl.GenBuffers(1, out vbo);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.CheckError("Generate and bind vbo buffer");

        unsafe
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(maxVertices * sizeof(T)), null, BufferUsageARB.StreamDraw);
        }

        gl.GenBuffers(1, out ebo);
        gl.CheckError("Generate ebo buffer");

        initializeEbo(maxQuads);
        gl.CheckError("Initialize ebo");

        Reflection.SetupVertexAttributes<T>(gl);
        gl.CheckError("Setup vertex attributes");

        gl.BindVertexArray(0);
        gl.CheckError("unbind");
    }

    private void initializeEbo(int maxQuads)
    {
        int indexCount = maxQuads * 6;
        uint[] indices = new uint[indexCount];

        for (int i = 0; i < maxQuads; i++)
        {
            var vertRoot = (uint)(i * 4);
            int iRoot = i * 6;

            indices[iRoot + 0] = vertRoot + 0;
            indices[iRoot + 1] = vertRoot + 1;
            indices[iRoot + 2] = vertRoot + 2;

            indices[iRoot + 3] = vertRoot + 2;
            indices[iRoot + 4] = vertRoot + 3;
            indices[iRoot + 5] = vertRoot + 0;
        }

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe
        {
            fixed (uint* i = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indexCount * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }
    }

    public void PushVertex(T vertex)
    {
        if (vertexIndex >= maxVertices)
        {
            Flush();
        }

        vertexArray[vertexIndex++] = vertex;
    }

    public void Flush()
    {
        if (vertexIndex == 0) return;

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        unsafe
        {
            fixed (T* pointer = vertexArray)
            {
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertexIndex * sizeof(T)), pointer);
            }

            gl.BindVertexArray(vao);
            gl.DrawElements(PrimitiveType.Triangles, (uint)(vertexIndex / 4 * 6), DrawElementsType.UnsignedInt, (void*)0);

            vertexIndex = 0;
        }

        gl.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        gl.DeleteVertexArray(vao);
        gl.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);
    }
}
