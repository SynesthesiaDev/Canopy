// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using System.Runtime.InteropServices;
using Canopy.Graphics;
using Silk.NET.OpenGL;

namespace Canopy.Dependency;

public static class Reflection
{
    public static void ResolveDependencies(object target)
    {
        var type = target.GetType();
        var currentType = type;

        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<ResolvedAttribute>() == null)
                    continue;

                var service = DependencyContainer.Get(field.FieldType);
                field.SetValue(target, service);
            }

            currentType = currentType.BaseType;
        }
    }

    public static unsafe void SetupVertexAttributes<T>(GL gl) where T : unmanaged
    {
        var type = typeof(T);
        var stride = (uint)sizeof(T);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var fieldInfo in fields)
        {
            var attributes = fieldInfo.GetCustomAttributes<VertexInfoAttribute>().ToArray();
            if (attributes.Length == 0) continue;

            var baseOffset = (int)Marshal.OffsetOf<T>(fieldInfo.Name);

            for (int i = 0; i < attributes.Length; i++)
            {
                var attr = attributes[i];

                int internalOffset = i * (attr.Count * getSizeOfAttributeType(attr.Type));

                var finalOffset = (void*)(baseOffset + internalOffset);

                gl.EnableVertexAttribArray((uint)attr.Index);
                gl.VertexAttribPointer(
                    (uint)attr.Index,
                    attr.Count,
                    attr.Type,
                    attr.Normalized,
                    stride,
                    finalOffset
                );
            }
        }
    }

    private static int getSizeOfAttributeType(VertexAttribPointerType type)
    {
        return type switch
        {
            VertexAttribPointerType.Float => sizeof(float),
            VertexAttribPointerType.UnsignedByte => sizeof(byte),
            VertexAttribPointerType.Int => sizeof(int),
            _ => sizeof(float)
        };
    }
}
