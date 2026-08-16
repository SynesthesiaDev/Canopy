using Serilog;
using Serilog.Events;
using Serilog.Sinks.SpectreConsole;
using Velopack;

namespace Canopy.Windows;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        using var log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.SpectreConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}", minLevel: LogEventLevel.Verbose)
            .CreateLogger();

        Log.Logger = log;

        var canopy = new Canopy(new CanopyPlatformWindows());
        canopy.Initialize();
    }
}
