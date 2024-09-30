using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Serilog;
using System;
using Velopack;
using Velopack.Sources;

namespace WorldClockStatusWindow;

internal class Program
{
    private static UpdateManager _updateManager;

    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

        Log.Logger.Information("Start");

        var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

        VelopackApp.Build().Run(loggerFactory.CreateLogger("Install"));

        var app = new App();
        app.InitializeComponent();
        app.Run();

        Log.Logger.Information("End");
    }

    public static UpdateManager UpdateManager => _updateManager ??= new UpdateManager(new GithubSource("https://github.com/ckaczor/WorldClockStatusWindow", null, false));

    public static string LocalVersion => (UpdateManager.CurrentVersion ?? new SemanticVersion(0, 0, 0)).ToString();
}