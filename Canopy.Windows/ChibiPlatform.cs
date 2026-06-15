// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Rendering;
using SynesthesiaDev.Chibi.Core;
using SynesthesiaDev.Chibi.Core.Enums;

namespace Canopy.Windows;

public class ChibiPlatform(ICanopyPlatform platform)
{
    public readonly ICanopyPlatform Platform = platform;
    public ChibiWindow Window = null!;
    public OpenGLRenderer? OpenGL;

    public void Run()
    {
        Window = new ChibiWindow();

        var res = Window.ScreenResolution;

        Window.OnWindowCreated.Subscribe(_ =>
        {
            var surface = new Gdi32Surface
            {
                Window = Window,
                Handle = Window.WindowHandle,
                NativeContext = new WindowsGLContext()
            };

            surface.InitializeGraphicsContext();

            OpenGL = new OpenGLRenderer
            {
                Surface = surface
            };

            OpenGL.Initialize();
            OpenGL.CompileDefaultShaders();

            // Platform.InjectIntoDesktop(e.Handle);
        });

        Window.OnWindowResized.Subscribe(e =>
        {
            if (OpenGL != null)
            {
                OpenGL.Resize((int)e.Now.X, (int)e.Now.Y);
            }
        });

        Window.OnFrame += () =>
        {
            OpenGL?.BeginDrawing();
            OpenGL?.OpenGL.ClearColor(1f, 0f, 0f, 1f);
            OpenGL?.EndDrawing();
        };

        Window.Run((int)res.X, (int)res.Y, WindowFlags.Resizable | WindowFlags.HighPixelDensity);
    }
}
