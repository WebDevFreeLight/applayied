// Licensed to the .NET Foundation under one or more agreements.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ZR.Pdf.Pdf
{

    public class ProcessManager
    {




        public static void CloseOutlookProcesses()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Get-Process *olk* | Stop-Process -Force\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit(150);  // Wait up to 5 seconds

                if (!string.IsNullOrEmpty(output))
                    Console.WriteLine($"PowerShell Output: {output}");

                if (!string.IsNullOrEmpty(errors))
                    Console.WriteLine($"?? PowerShell Errors: {errors}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Error: {ex.Message}");
            }
        }




        public static void CloseOutlookProcessesV2()
        {
            try
            {
                Console.WriteLine("Attempting to close Microsoft Outlook...");

                 
                // Method 1: WMI Query (most reliable)
                if (!TryCloseOutlookViaWmi())
                {
                    // Method 2: Traditional Process API
                    if (!TryCloseOutlookViaProcessApi())
                    {
                        // Method 3: Window Title detection
                        TryCloseOutlookViaWindowTitles();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Unexpected error closing Outlook: {ex.Message}");
            }
        }

        private static bool TryCloseOutlookViaWmi()
        {
            try
            {


                return false;
                //using (var searcher = new ManagementObjectSearcher(
                //    "SELECT ProcessId, Name FROM Win32_Process WHERE Name LIKE '%olk%'"))
                //{
                //    bool anyClosed = false;
                //    foreach (ManagementObject process in searcher.Get())
                //    {
                //        try
                //        {
                //            int processId = Convert.ToInt32(process["ProcessId"]);
                //            string processName = process["Name"].ToString();
                //            Console.WriteLine($"Found Outlook process via WMI: {processName} (PID: {processId})");

                //            if (TryCloseProcessById(processId))
                //            {
                //                anyClosed = true;
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"?? Error processing WMI result: {ex.Message}");
                //        }
                //    }
                //    return anyClosed;
                //}
         
            
            
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? WMI query failed: {ex.Message}");
                return false;
            }
        }

        private static bool TryCloseOutlookViaProcessApi()
        {
            try
            {
                bool anyClosed = false;
                foreach (var process in Process.GetProcessesByName("olk"))
                {
                    try
                    {
                        if (TryCloseProcess(process))
                        {
                            anyClosed = true;
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                return anyClosed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Process API method failed: {ex.Message}");
                return false;
            }
        }

        private static bool TryCloseOutlookViaWindowTitles()
        {
            try
            {
                bool anyClosed = false;
                foreach (var process in Process.GetProcesses()
                    .Where(p => p.MainWindowTitle.IndexOf("olk", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    try
                    {
                        if (TryCloseProcess(process))
                        {
                            anyClosed = true;
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                return anyClosed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Window title method failed: {ex.Message}");
                return false;
            }
        }

        private static bool TryCloseProcessById(int processId)
        {
            try
            {
                using (var process = Process.GetProcessById(processId))
                {
                    return TryCloseProcess(process);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Error closing process {processId}: {ex.Message}");
                return false;
            }
        }

        private static bool TryCloseProcess(Process process)
        {
            try
            {
                if (process.HasExited)
                {
                    return false;
                }

                Console.WriteLine($"Attempting to close process: {process.ProcessName} (PID: {process.Id})");

                // Try graceful shutdown first
                if (process.CloseMainWindow())
                {
                    if (process.WaitForExit(3000)) // Wait 3 seconds
                    {
                        Console.WriteLine($"? Process closed gracefully: {process.ProcessName} (PID: {process.Id})");
                        return true;
                    }
                }

                // Force kill if still running
                if (!process.HasExited)
                {
                    process.Kill();
                    if (process.WaitForExit(2000)) // Wait 2 seconds after kill
                    {
                        Console.WriteLine($"? Process force-killed: {process.ProcessName} (PID: {process.Id})");
                        return true;
                    }
                }

                return false;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"?? Access denied closing process {process.Id}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Error closing process {process.Id}: {ex.Message}");
                return false;
            }
        }
    }
}
