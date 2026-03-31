using System;
using System.Windows.Forms;

namespace SincroPelis
{
    internal static class Program
    {
        public static MainForm myForm = null!;
        public static Server server = new Server();
        public static Client client = new Client();
        public static Task serverTask = null!;

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
                myForm = new MainForm();
                Application.Run(myForm);
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