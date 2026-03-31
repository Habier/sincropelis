using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace SincroPelis
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SincroPelis",
            "logs",
            "app.log");

        public static void Initialize()
        {
            var logDir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    LogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Info("Logger initialized");
        }

        public static void Info(string message) => Log.Information(message);
        public static void Debug(string message) => Log.Debug(message);
        public static void Warning(string message) => Log.Warning(message);
        public static void Error(string message, Exception? ex = null)
        {
            if (ex != null)
                Log.Error(ex, message);
            else
                Log.Error(message);
        }

        public static void Shutdown()
        {
            Info("Application shutting down");
            Log.CloseAndFlush();
        }
    }
}
