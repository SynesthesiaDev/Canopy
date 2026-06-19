// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Graphics;
using Canopy.Rendering;
using Canopy.Utils;
using Synesthesia.Utils;

namespace Canopy.Configuration;

public class CanopyUserlandScope
{
    public required RuntimeInfo.Platform Platform;
    public required string Version;
    public required bool IsDevelopmentBuild;
    public required string OpenGLVersion;
    public required string ShaderVersion;

    /*
     * Registers a clock with specified interval. Can be listened to via
     */
    public void RegisterClock(TimeSpan interval)
    {
        // internals later, just api layer now
    }

    /*
     * You should only really do this if you know what you are doing.
     * Calling this will disable built-in frame optimizations (running at 0fps until needed)
     * And leave this up to you
     */
    public void RegisterDrawHook()
    {
    }

    public class Configuration
    {
        /*
         * Set the underlying windows wallpaper to any newly pushed wallpaper
         * In case the program crashes, and before it start up (windows can be slow on startup when running auto-run programs)
         */
        public bool SetUnderlyingWindowsWallpaper = true;

        /*
         * Initializes OpenGL with 8 stencil bits.
         * Not needed for normal use, but if you run custom shaders or
         * more complex rendering logic with masking, you may need this.
         */
        public bool UseStencil = false;

        /*
         * Show the default wallpaper if no user-selected one is currently showing.
         * This is mainly for first-run wizard kinda thing since the default wallpaper has config instructions in it (im too lazy to render text)
         */
        public bool ShowDefaultWallpaper = true;

        /*
         * Release stream for an automatic update to pull from.
         * WARNING: Development release stream may be unstable and introduce breaking api changes often
         */
        public ReleaseStream ReleaseStream = ReleaseStream.Release;

        /*
         * Done on startup using Velopack
         */
        public bool AutoUpdate = true;

        /*
         * for development purposes only
         */
        public bool DebugRenderVisualizer = false;

        /*
         * For video wallpapers
         */
        public HardwareDecoder HardwareVideoDecoder = HardwareDecoder.NVDEC;
    }
}

// Entry main Lua file
public interface IConfigurationScriptApi
{
    string Author { get; }
    string Name { get; }

    void OnInitialize(CanopyUserlandScope scope);
    void OnClockTick();
    void OnFrame(OpenGLRenderer renderer);

    void PushWallpaper(Wallpaper wallpaper, Transition transition);

    record Transition(long Time, Easing Easing, Action? Callback = null)
    {
        public static readonly Transition DEFAULT = new Transition(5000, Easing.In);
    }
}
