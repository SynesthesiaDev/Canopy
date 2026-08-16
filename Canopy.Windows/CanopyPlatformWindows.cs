// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Canopy.Windows.Extensions;
using H.NotifyIcon.Core;
using Microsoft.Win32;
using Serilog;
using Synesthesia.Utils;
using Synesthesia.Utils.Extensions;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace Canopy.Windows;

public class CanopyPlatformWindows : ICanopyPlatform
{
    RuntimeInfo.Platform ICanopyPlatform.Platform => RuntimeInfo.Platform.Windows;
    private const string registry_key_path = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private BackgroundTrayService backgroundTrayService = null!;

    public void SetDesktop(string path)
    {
        var useLegacyMethod = Canopy.CurrentConfig.System.UseLegacyWindowsApi;

        if (useLegacyMethod)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"canopy-wallpaper-{Guid.NewGuid().ToString()}.bpm");
            File.WriteAllBytes(tempPath, convertToBmp(File.ReadAllBytes(path)));

            var result = SystemParametersInfo
            (
                SPI.SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDWININICHANGE
            );

            if (!result)
            {
                Log.Error("Failed to set wallpaper via legacy windows SystemParametersInfo api");
            }

            File.Delete(tempPath);

            return;
        }

        var wallpaper = new Shell32.IDesktopWallpaper();
        try
        {
            wallpaper.SetWallpaper(null, path);
            wallpaper.SetPosition(Canopy.CurrentConfig.General.FitMode.ToShellPos());
        }
        finally
        {
            Marshal.ReleaseComObject(wallpaper);
        }
    }


    public void SetTheme(ICanopyPlatform.Theme theme)
    {
        var useLightTheme = (theme == ICanopyPlatform.Theme.Light).ToInt();
        using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registry_key_path, true))
        {
            if (key != null)
            {
                key.SetValue("AppsUseLightTheme", useLightTheme, RegistryValueKind.DWord);
                key.SetValue("SystemUsesLightTheme", useLightTheme, RegistryValueKind.DWord);
            }
        }

        unsafe
        {
            nint result = 0;
            fixed (char* pString = "ImmersiveColorSet")
            {
                SendMessageTimeout(
                    HWND.HWND_BROADCAST,
                    (uint)WindowMessage.WM_SETTINGCHANGE,
                    IntPtr.Zero,
                    (IntPtr)pString,
                    SMTO.SMTO_ABORTIFHUNG,
                    5000,
                    ref result
                );
            }

            fixed (char* pPolicy = "Policy")
            {
                SendMessageTimeout(
                    HWND.HWND_BROADCAST,
                    (uint)WindowMessage.WM_SETTINGCHANGE,
                    IntPtr.Zero,
                    (IntPtr)pPolicy,
                    SMTO.SMTO_ABORTIFHUNG,
                    5000,
                    ref result
                );
            }
        }
    }

    public void InitializeTray(Canopy canopy)
    {
        backgroundTrayService = new BackgroundTrayService(canopy);
        backgroundTrayService.Start();
    }

    public void ShowNotification(string title, string message, ICanopyPlatform.NotificationLevel level = ICanopyPlatform.NotificationLevel.Info)
    {
        var icon = level switch
        {
            ICanopyPlatform.NotificationLevel.Info => NotificationIcon.Info,
            ICanopyPlatform.NotificationLevel.Warning => NotificationIcon.Warning,
            ICanopyPlatform.NotificationLevel.Error => NotificationIcon.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        backgroundTrayService.ShowBalloon(title, message, icon);
    }

    public void BlockThread()
    {
        backgroundTrayService.TrayThread?.Join();
    }

    private static byte[] convertToBmp(byte[] sourceImageBytes)
    {
        using var inputStream = new MemoryStream(sourceImageBytes);
        using var bitmap = new Bitmap(inputStream);
        using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, ImageFormat.Bmp);
        return outputStream.ToArray();
    }
}
