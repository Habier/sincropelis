using System.Windows.Forms;

namespace SincroPelis
{
    internal static class Program
    {
        public static MainForm myForm;
        public static Server server = new Server();
        public static Client client = new Client();
        public static Task serverTask;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            myForm = new MainForm();
            Application.Run(myForm);
        }
    }
}