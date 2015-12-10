using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using static aQuery.aQuery;
using System.IO;
using System.Linq;

namespace aQuery.Amibroker
{
    public class AmibrokerApp
    {
        // ReSharper disable InconsistentNaming
        private const string fileMenu = "'xtpBarTop' pane > 'Menu Bar' menu bar > 'File' menu item";
        private const string file_NewMenu = "'File' menu > 'New' menu item";
        private const string file_New_AnalysisMenu = "'New' menu > 'Analysis' menu item";
        // ReSharper restore InconsistentNaming

        public string BrokerPath { get; }
        public Process Broker { get; }
        public aQuery Window { get; private set; }
        public DirectoryInfo ReportsDir { get; private set; }

        public AmibrokerApp(string path = null)
        {
            BrokerPath = path == null ? GetBrokerPath() : null;

            if (BrokerPath == null)
            {
                throw new Exception("Unable to find Amibroker installation path");
            }

            Broker = ProcessHelpers.RunProgram(BrokerPath);
            Setup();

            Thread.Sleep(250);
        }

        public AmibrokerApp(Process process)
        {
            Broker = process;
            BrokerPath = process.GetMainModuleFilepath();
            Setup();

            if (BrokerPath == null)
            {
                throw new Exception("Unable to find Amibroker installation path");
            }
        }

        protected void Setup()
        {
            Window = a($"window[ClassName=AmiBrokerMainFrameClass][ProcessId={Broker.Id}]");
            
            // ReSharper disable once AssignNullToNotNullAttribute
            var reportsDirPath = Path.Combine(Path.GetDirectoryName(BrokerPath), "Reports");
            ReportsDir = new DirectoryInfo(reportsDirPath);
        }

        public static void KillAllInstances()
        {
            ProcessHelpers.KillProcess("broker");
        }

        public static AmibrokerApp GetExistingInstance()
        {
            var process = ProcessHelpers.FindProcess("broker").FirstOrDefault();

            return process != null ? new AmibrokerApp(process) : null;
        }

        public void HideAbout()
        {
            a("Dialog > 'OK' button", Window).Click();
        }

        public void ClearReportsFolder()
        {
            foreach (FileInfo file in ReportsDir.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in ReportsDir.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static string GetBrokerPath()
        {
            var localKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            var subkey = localKey.OpenSubKey(@"CLSID\{2DCDD562-9CC9-11D3-BF72-00C0DFE30718}\LocalServer32");

            return (string)subkey?.GetValue(null);
        }

        public AnalysisWindow NewAnalysis()
        {
            a(fileMenu, Window).Click();
            a(file_NewMenu, Window).Click();
            a(file_New_AnalysisMenu, Window).Click();
            
            return new AnalysisWindow(this);
        }

        public void OpenFile(string path, aQuery context = null)
        {
            context = context ?? Window;

            var openDialog = a("'Open' Dialog", context);
            a("'File name:' combo box > 'File name:' edit", openDialog)
                .Text(path);
            a("'Open' button", openDialog).Click();
        }
    }
}
