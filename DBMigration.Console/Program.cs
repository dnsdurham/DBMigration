using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

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
            StreamReader file = new StreamReader(sourcePath + "params.txt");
            while ((line = file.ReadLine()) != null)
            {
                // process the params file
            }

            // Insert the sql commands into the database
        }
    }
}
