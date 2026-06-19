// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Rendering.Shaders;

public sealed class ShaderSources
{
    public const string DEFAULT_VERTEX = @"
        #version 330 core
        layout(location = 0) in vec2 a_pos;
        layout(location = 1) in vec2 a_texCoord;

        out vec2 v_texCoord;
        uniform mat4 u_projection;

        void main()
        {
            v_texCoord  = a_texCoord;
            gl_Position = u_projection * vec4(a_pos, 0.0, 1.0);
        }
    ";

    public const string DEFAULT_FRAGMENT = @"
        #version 330 core
        in vec2 v_texCoord;
        out vec4 FragColor;

        uniform sampler2D u_texture;
        uniform bool u_use_texture;
        uniform vec4 u_color;
        uniform float u_alpha;

        void main()
        {
            vec4 color = u_use_texture
                ? texture(u_texture, v_texCoord) * u_color
                : u_color;

            FragColor = vec4(color.rgb, color.a * u_alpha);
        }
    ";
}
