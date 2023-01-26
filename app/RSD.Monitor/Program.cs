using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualBasic.Logging;
using RSD.Monitor.Controller;

namespace RSD.Monitor
{
    static class Program
    {
        public static ControllerCommunicator Comm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // - Setups the Logging
            String logPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "RSD.Monitor.log");
            FileLogTraceListener tl = new FileLogTraceListener();
            tl.BaseFileName = logPath;
            tl.AutoFlush = true;
            tl.MaxFileSize = (1024 * 1024);
            Trace.Listeners.Add(tl);

            // -
            Trace.TraceInformation("Start-up");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
            Application.Run(new Main());            
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
           if(Comm != null) {
               if (Comm.IsOpen())
               {
                   Comm.Clear();
               }
           }
        }
    }
}