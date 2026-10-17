using System;
using System.Diagnostics;

namespace Infrastructure
{
    /// <summary>
    /// Shell command helper for system operations
    /// WARNING: Use with caution - validate all inputs
    /// </summary>
    public class ShellHelper
    {
        /// <summary>
        /// Execute bash command (Linux)
        /// </summary>
        public static string Bash(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();
            return result;
        }

        /// <summary>
        /// Execute command (Windows)
        /// </summary>
        public static string Cmd(string fileName, string args)
        {
            string output = string.Empty;
            var info = new ProcessStartInfo();
            info.FileName = fileName;
            info.Arguments = args;
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            return output;
        }
    }
}
