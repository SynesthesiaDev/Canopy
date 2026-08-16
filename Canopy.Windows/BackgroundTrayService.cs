// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;
using H.NotifyIcon.Core;
using Serilog;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Kernel32;

namespace Canopy.Windows;

public class BackgroundTrayService(Canopy canopy)
{
    public Thread? TrayThread;
    private TrayIconWithContextMenu trayIcon = null!;
    private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
    private uint trayThreadId;

    public void Start()
    {

        TrayThread = new Thread(() =>
        {
            trayThreadId = GetCurrentThreadId();
            trayIcon = new TrayIconWithContextMenu();
            trayIcon.ToolTip = "🌿 Canopy";

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Canopy.Windows.canopy.ico");

            if (stream != null)
            {
                var icon = new Icon(stream);
                trayIcon.UpdateIcon(icon.Handle);
            }
            else
            {
                Log.Warning("Icon not found");
            }

            var menu = new PopupMenu();
            menu.Items.Add(new PopupMenuItem("Open Config Folder", (_, _) => Utils.OpenFolder(Canopy.CANOPY_FOLDER_PATH)));
            menu.Items.Add(new PopupMenuItem("Reload Config", (_, _) => canopy.LoadRefreshable()));
            menu.Items.Add(new PopupMenuItem("Dark Theme", (_, _) => canopy.Platform.SetTheme(ICanopyPlatform.Theme.Dark)));
            menu.Items.Add(new PopupMenuItem("Light Theme", (_, _) => canopy.Platform.SetTheme(ICanopyPlatform.Theme.Light)));
            menu.Items.Add(new PopupMenuItem("Check for Updates", (_, _) => _ = Updater.CheckForUpdates(canopy)));
            menu.Items.Add(new PopupMenuItem("Exit", (_, _) => Environment.Exit(0)));
            trayIcon.ContextMenu = menu;

            trayIcon.Create();

            while (GetMessage(out MSG msg, IntPtr.Zero) == 1)
            {
                while (queue.TryDequeue(out var task))
                {
                    task.Invoke();
                }
                TranslateMessage(in msg);
                DispatchMessage(in msg);
            }
        });

        TrayThread.SetApartmentState(ApartmentState.STA);
        TrayThread.IsBackground = true;
        TrayThread.Start();
    }

    public void ShowBalloon(string title, string message, NotificationIcon icon)
    {
        queue.Enqueue(() =>
        {
            trayIcon.ShowNotification(
                title: title,
                message: message,
                icon: icon
            );
        });

        PostThreadMessage(trayThreadId, (uint)WindowMessage.WM_NULL, IntPtr.Zero, IntPtr.Zero);
    }
}
