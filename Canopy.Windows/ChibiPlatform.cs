// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Canopy.Dependency;
using Canopy.Rendering;
using Canopy.Rendering.Textures;
using Canopy.Storage;
using Serilog;
using SynesthesiaDev.Chibi.Core;
using SynesthesiaDev.Chibi.Core.Enums;

namespace Canopy.Windows;

public class ChibiPlatform(ICanopyPlatform platform)
{
    public readonly ICanopyPlatform Platform = platform;
    public ChibiWindow Window = null!;
    public OpenGLRenderer? OpenGL;

    private Texture furina = null!;

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

            DependencyContainer.AddSingleton(OpenGL);

            var storage = new DirectoryAssetStorage(@"C:\Users\Synesthesia\RiderProjects\Canopy\resources");
            foreach (var file in storage.GetAllFiles()) Log.Information("File - {file}", file);

            furina = storage.GetResolved("furina.png", DataParsers.LoadTexture);
            // Platform.InjectIntoDesktop(e.Handle);
        });

        Window.OnWindowResized.Subscribe(e =>
        {
            OpenGL?.Resize((int)e.Now.X, (int)e.Now.Y);
        });

        Window.OnFrame += () =>
        {
            var canvasSize = new Vector2(OpenGL!.BackBufferWidth, OpenGL!.BackBufferHeight);
            var currentSize = new Vector2(OpenGL!.BackBufferWidth, OpenGL!.BackBufferHeight);

            OpenGL?.BeginDrawing();
            // OpenGL?.BindTexture(furina);
            // OpenGL?.DrawQuad(Vector2.Zero, Window.WindowSize, 0xFFFFFFFF, 1f, 0f, furina, new RectangleF(1, 1, 1, 1));
            OpenGL?.DrawQuad(Vector2.Zero, currentSize, 0xFFFFFFFF, 1f, 0f, furina, null);
            OpenGL?.EndDrawing();
        };

        Window.Run((int)res.X, (int)res.Y, WindowFlags.Resizable | WindowFlags.HighPixelDensity);
    }
}
