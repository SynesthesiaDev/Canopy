// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;
using H.NotifyIcon.Core;
using Serilog;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace Canopy.Windows;

public class BackgroundTrayService(Canopy canopy)
{
    public Thread? TrayThread;
    private TrayIconWithContextMenu trayIcon = null!;
    private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

    public void Start()
    {

        TrayThread = new Thread(() =>
        {
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
            // menu.Items.Add(new PopupMenuItem("Test popup", (_, _) => canopy.Platform.ShowNotification("fuccckkk", "my peniiiiiiiiiis", ICanopyPlatform.NotificationLevel.Warning)));
            menu.Items.Add(new PopupMenuItem("Dark Theme", (_, _) => canopy.Platform.SetTheme(ICanopyPlatform.Theme.Dark)));
            menu.Items.Add(new PopupMenuItem("Light Theme", (_, _) => canopy.Platform.SetTheme(ICanopyPlatform.Theme.Light)));
            menu.Items.Add(new PopupMenuItem("Exit", (_, _) => Environment.Exit(0)));
            trayIcon.ContextMenu = menu;

            trayIcon.Create();

            while (GetMessage(out MSG msg, IntPtr.Zero) == 1)
            {
                while (queue.TryDequeue(out var task))
                {
                    Log.Information("dequeued");
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
            Log.Warning("{title}, {message}, {icon}", title, message, icon);
            trayIcon.ShowNotification(
                title: title,
                message: message,
                icon: icon
            );
        });
    }
}
