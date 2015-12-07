using System.Diagnostics;
using System.Linq;
using System.Management;

namespace aQuery
{
    public static class ProcessHelpers
    {
        public static Process Run(string command)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C {command}"
            };
            process.StartInfo = startInfo;
            process.Start();

            return process;
        }

        public static Process RunProgram(string programPath, string arguments = null)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = programPath
            };
            if (arguments != null)
            {
                startInfo.Arguments = arguments;
            }

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForInputIdle();

            return process;
        }

        public static Process[] FindProcess(string programName)
        {
            return Process.GetProcessesByName(programName);
        }

        public static void KillProcess(string programName)
        {
            foreach (var process in FindProcess(programName))
            {
                process.Kill();
            }
        }
        public static string GetMainModuleFilepath(this Process process)
        {
            var wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + process.Id;

            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    var mo = results.Cast<ManagementObject>().FirstOrDefault();

                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }

            return null;
        }
    }
}