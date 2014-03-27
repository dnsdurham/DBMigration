using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace DBMigration.ConsoleApp
{
    class CmdLineWrapper
    {
        internal static string RunCmdLine(string appName, string args)
        {
            // http://stackoverflow.com/questions/6817777/execute-a-command-line-utility-in-asp-net
            // http://support.microsoft.com/kb/305994

            StreamReader outputStream = StreamReader.Null;
            string output = "";
            //TODO: need to handle errors and timeouts

            var psi = new ProcessStartInfo(appName, args)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();
                outputStream = process.StandardOutput;
                output = outputStream.ReadToEnd();
                process.WaitForExit();
            }

            outputStream.Close();
            return output;
        }
    }
}
