using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NitroDock
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Force DPI awareness
            SetProcessDPIAware();

            // Global exception handling
            Application.ThreadException += (sender, e) =>
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logPath = System.IO.Path.Combine(appPath, "NitroDockX_Crash.log");
                try
                {
                    System.IO.File.AppendAllText(logPath, $"ThreadException: {e.Exception}\n");
                }
                catch { }
                MessageBox.Show($"Fatal error: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logPath = System.IO.Path.Combine(appPath, "NitroDockX_Crash.log");
                try
                {
                    System.IO.File.AppendAllText(logPath, $"UnhandledException: {e.ExceptionObject}\n");
                }
                catch { }
                MessageBox.Show($"Fatal error: {e.ExceptionObject}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NitroDockMain());
        }
    }
}
