using Serilog;
using Serilog.Events;
using Serilog.Sinks.SpectreConsole;

namespace Canopy.Windows;

internal class Program
{
    private static void Main(string[] args)
    {
        using var log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.SpectreConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}", minLevel: LogEventLevel.Verbose)
            .CreateLogger();

        Log.Logger = log;

        var canopy = new CanopyPlatformWindows();
        canopy.Initialize();
    }
}
