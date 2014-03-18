using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace DBMigration.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: build in command line parameter logic for enabling console options and runnin unattended
 
            string sourcePath = ConfigurationManager.AppSettings["SourcePath"];
            string upgradePath = "";
            string line;
            string[] split;
            bool stepSuccess = false;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Step 0: Read in the params.txt file
            Console.WriteLine("Step 0: Read in the params.txt file");

            // find the upgrade directory in the source directory
            try 
            {	        
                string[] folders = System.IO.Directory.GetDirectories(sourcePath, "E*", System.IO.SearchOption.TopDirectoryOnly);
                if (folders.Length == 1) // there should only be one
                {
                    upgradePath = folders[0]; 
                    StreamReader file = new StreamReader(upgradePath + "\\Config\\params.txt");
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

                }
                stepSuccess = true;	
                Console.WriteLine("Params read from file.");
            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("Step 0 Error: " + ex.Message);
                Console.WriteLine("Step 0 Error: " + ex.Message);
                stepSuccess = false;
            }

            // Step 1: Insert Upgrade Scripts from TFS
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success flag
                Console.WriteLine("Step 1: Insert Upgrade Scripts from TFS");
                try 
                {	        
                    // get the directories
                    string[] upgradeFolders = System.IO.Directory.GetDirectories(upgradePath, "*", System.IO.SearchOption.TopDirectoryOnly);
                    //load the directories into a sorted list
                    SortedDictionary<int, string> sortedFolders = new SortedDictionary<int,string>();
                    foreach (var dir in upgradeFolders)
                    {
                        int dirId;
                        // get just the last directory name
                        string folderName = dir.Substring(dir.Length - 3, 3);
                        if (Int32.TryParse(folderName, out dirId))
                        {
                            sortedFolders.Add(dirId, dir);
                        }

                    }

                    if (sortedFolders.Count > 0)
                    {
                        // 1- loop through each of the folders
                        // 2 - read the files
                        // 3 - insert into the upgrade_commands table
                        foreach (var folder in sortedFolders.Values)
                        {
                            

                            // get files from a directory
                            string[] files = Directory.GetFiles(folder);
                            // ensure the files are in sorted order
                            Array.Sort(files);
                            // sort the array to ensure this occurs in order
                            foreach (string file in files) 
                            {
                                Console.WriteLine(file);
                            }

                        }
                        stepSuccess = true;
                    }

                }
                catch (Exception ex)
                {
                    //TODO: implement desired error handling
                    Trace.WriteLine("Step 1 Error: " + ex.Message);
                    Console.WriteLine("Step 1 Error: " + ex.Message);
                    stepSuccess = false;
                }

            }

            
            // Step 2: Prepare for the Export
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
            }

            // Step 3: Create the Export
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
            }

            // Step 4: Importing the export
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            // Step 5: Running the upgrade
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            // Step 6: Running the finalize
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            Console.WriteLine("Hit enter key to end.");
            Console.ReadLine();
        }
    }
}
