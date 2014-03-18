using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace DBMigration.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: build in command line parameter logic for enabling console options and runnin unattended
 
            // Read in the params.txt file
            string sourcePath = ConfigurationManager.AppSettings["SourcePath"];
            string line;
            string[] split;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            StreamReader file = new StreamReader(sourcePath + "params.txt");
            while ((line = file.ReadLine()) != null)
            {
                // process the params file and store in a dictionary
                //TODO: should we make sure that all of the expected parameters are in the params file?
                split = line.Split(new Char[] {'='});
                if (split.Length == 2)
                {
                    parameters.Add(split[0].Trim(), split[1].Trim());
                }
                else
                {
                    //write the line to debug
                    //TODO: write this to a log file
                    Trace.WriteLine("Invalid param line value: " + line);
                }
            }

            // Insert the sql commands into the database
        }
    }
}
