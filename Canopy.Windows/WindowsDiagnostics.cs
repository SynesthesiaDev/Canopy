using System.Runtime.InteropServices;
using System.Text;
using Serilog;

namespace Canopy.Windows;

public static class WindowDiagnostics
{
    // Raw P/Invoke to avoid any Vanara wrapper uncertainty
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] private static extern int  GetWindowLong(IntPtr h, int idx);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr h, uint cmd);
    [DllImport("user32.dll")] private static extern IntPtr GetTopWindow(IntPtr h);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr h);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr h, StringBuilder sb, int max);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int L, T, R, B; }

    private const int  GWL_STYLE   = -16;
    private const int  GWL_EXSTYLE = -20;
    private const uint GW_HWNDNEXT = 2;

    private const uint WS_CHILD              = 0x40000000;
    private const uint WS_VISIBLE            = 0x10000000;
    private const uint WS_EX_LAYERED         = 0x00080000;
    private const uint WS_EX_NOREDIRBITMAP   = 0x00200000;  // Win11 HDR / layered shell flag
    private const uint WS_EX_TRANSPARENT     = 0x00000020;

    /// <summary>
    /// Dumps the full window subtree rooted at <paramref name="root"/>.
    /// Pass your SDL handle as <paramref name="highlight"/> to mark it in the output.
    /// </summary>
    public static void DumpTree(IntPtr root, string label, IntPtr highlight = default)
    {
        Log.Information("┌── {Label} ──────────────────────────────────────────", label);
        Walk(root, 0, highlight);
        Log.Information("└─────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Quick one-liner about a single window — useful for spot-checks.
    /// </summary>
    public static void DumpWindow(IntPtr hwnd, string label)
    {
        Log.Information("[Diag:{Label}] {Info}", label, Describe(hwnd, false));
    }

    // ─── internals ────────────────────────────────────────────────────────────

    private static void Walk(IntPtr hwnd, int depth, IntPtr highlight)
    {
        if (hwnd == IntPtr.Zero) return;

        var isSdl  = hwnd == highlight;
        var indent = new string(' ', depth * 3);
        var arrow  = isSdl ? "  ◄◄◄ SDL WINDOW" : "";

        Log.Information("{Indent}{Info}{Arrow}", indent, Describe(hwnd, isSdl), arrow);

        var child = GetTopWindow(hwnd);
        while (child != IntPtr.Zero)
        {
            Walk(child, depth + 1, highlight);
            child = GetWindow(child, GW_HWNDNEXT);
        }
    }

    private static string Describe(IntPtr hwnd, bool isSdl)
    {
        var sb = new StringBuilder(256);
        GetClassName(hwnd, sb, sb.Capacity);
        var cls = sb.ToString();

        GetWindowRect(hwnd, out var r);
        var w = r.R - r.L;
        var h = r.B - r.T;

        var style   = unchecked((uint)GetWindowLong(hwnd, GWL_STYLE));
        var exStyle = unchecked((uint)GetWindowLong(hwnd, GWL_EXSTYLE));

        var vis       = IsWindowVisible(hwnd);
        var isChild   = (style   & WS_CHILD)            != 0;
        var noRedir   = (exStyle & WS_EX_NOREDIRBITMAP) != 0;  // critical on Win11
        var layered   = (exStyle & WS_EX_LAYERED)       != 0;
        var transp    = (exStyle & WS_EX_TRANSPARENT)   != 0;

        var parent    = GetParent(hwnd);

        // Highlight any suspicious flags in the output
        var flags = new List<string>();
        if (!vis)     flags.Add("!HIDDEN");
        if (!isChild) flags.Add("TOP-LEVEL");
        if (noRedir)  flags.Add("NO-REDIR");   // present on Progman on Win11
        if (layered)  flags.Add("LAYERED");
        if (transp)   flags.Add("TRANSPARENT");
        if (w == 0 || h == 0) flags.Add("ZERO-SIZE");

        var flagStr = flags.Count > 0 ? $" [{string.Join(' ', flags)}]" : "";

        return $"[{hwnd:X8}] {cls} | {w}x{h} @{r.L},{r.T} | parent={parent:X8} | style=0x{style:X8}{flagStr}";
    }
}
