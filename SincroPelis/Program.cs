using System;
using System.Windows.Forms;

namespace SincroPelis
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Logger.Initialize();
            Logger.Info("Application starting");

            try
            {
                ApplicationConfiguration.Initialize();
                KeyController.Start();
                var server = new Server();
                var client = new Client();
                using var mainForm = new MainForm(client, server);
                Application.Run(mainForm);
                KeyController.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception in main", ex);
                throw;
            }
            finally
            {
                Logger.Shutdown();
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Error("FATAL: Unhandled exception", ex);
            Logger.Shutdown();
            Environment.Exit(1);
        }
    }
}
