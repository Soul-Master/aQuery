using System.Diagnostics;

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

        public static void KillProcess(string programName)
        {
            foreach (var process in Process.GetProcessesByName(programName))
            {
                process.Kill();
            }
        }
    }
}