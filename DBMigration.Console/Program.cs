using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using Oracle.DataAccess.Client;
using System.Data;

namespace DBMigration.ConsoleApp
{
    class Program
    {
        static string sourcePath = ConfigurationManager.AppSettings["SourcePath"];
        static string upgradePath = "";
        static Dictionary<string, string> parameters = new Dictionary<string, string>();
        static string connString = ConfigurationManager.ConnectionStrings["UPGTest"].ConnectionString;

        static void Main(string[] args)
        {
            //TODO: build in command line parameter logic for enabling console options and running unattended
            //TODO: refactor this into subroutines for ease of maintenance and readability
 
            bool stepSuccess = false;

            // Step 0: Read in the params.txt file
            Console.WriteLine("Step 0: Read in the params.txt file");
            stepSuccess = LoadParameters();

            // Step 1: Insert Upgrade Scripts from TFS
            if (stepSuccess == true)
            {
                Console.WriteLine("Step 1: Insert Upgrade Scripts from TFS");
                stepSuccess = InsertUpgradeScripts();
            }

            // Step 2: Prepare for the Export
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
            }

            // Step 3: Create the Export
            if (stepSuccess == true)
            {
                Console.WriteLine(CmdLineWrapper.RunCmdLine("blah=poop poop=blah"));

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

        private static bool LoadParameters()
        {
            // find the upgrade directory in the source directory
            try
            {
                string[] folders = System.IO.Directory.GetDirectories(sourcePath, "E*", System.IO.SearchOption.TopDirectoryOnly);
                string line;
                string[] split;

                if (folders.Length == 1) // there should only be one
                {
                    upgradePath = folders[0];
                    StreamReader file = new StreamReader(upgradePath + "\\Config\\params.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        // process the params file and store in a dictionary
                        //TODO: should we make sure that all of the expected parameters are in the params file?
                        split = line.Split(new Char[] { '=' });
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
                Console.WriteLine("Params read from file.");
                return true;
            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("Step 0 Error: " + ex.Message);
                Console.WriteLine("Step 0 Error: " + ex.Message);
                return false;
            }
        }

        private static bool InsertUpgradeScripts()
        {
            bool result = false;
            OracleConnection conn = new OracleConnection(connString);

            try
            {
                // update the utility package used for script inserts
                //TODO: add the logic to load this script from the config folder and execute on the target database

                // prepare the upgrade command table
                //TODO: ensure it is correct to truncate these tables prior to inserting new commands
                conn.Open();
                using (OracleCommand truncateCmd = new OracleCommand("truncate table rt_upgrade_command", conn))
                {
                    truncateCmd.ExecuteNonQuery();
                }
                conn.Close();

                // get the directories
                string[] upgradeFolders = System.IO.Directory.GetDirectories(upgradePath, "*", System.IO.SearchOption.TopDirectoryOnly);
                //load the directories into a sorted list
                SortedDictionary<int, string> sortedFolders = new SortedDictionary<int, string>();
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

                    // open the db connection once for all inserts
                    conn.Open();

                    foreach (var folder in sortedFolders.Values)
                    {
                        // get files from a directory
                        string[] files = Directory.GetFiles(folder);
                        // ensure the files are in sorted order
                        Array.Sort(files);
                        // sort the array to ensure this occurs in order
                        foreach (string fileName in files)
                        {
                            // get the contents of the script 
                            StreamReader file = new StreamReader(fileName);
                            string script = file.ReadToEnd();
                            // insert into the upgrade commands table
                            using (OracleCommand cmd = new OracleCommand("upgrade_exp_prep.add_upgrade_command", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                //procedure add_upgrade_command
                                //(i_upg_id          integer,
                                //i_execution_order  integer,
                                //i_final_ind        char,
                                //i_retry_type       integer,
                                //i_section          integer,
                                //i_preview          varchar2,
                                //i_upg_command      clob,
                                //i_success_ind      char);

                                // add the parameters
                                // TODO: will need to figure out what the appropriate settings are for each script in the upgrade_commands table
                                cmd.Parameters.Add("@i_upg_id", OracleDbType.Int16, 1, ParameterDirection.Input);
                                cmd.Parameters.Add("@i_execution_order", OracleDbType.Int16, 100, ParameterDirection.Input);
                                cmd.Parameters.Add("@i_final_ind", OracleDbType.Char, 1, "N", ParameterDirection.Input);
                                cmd.Parameters.Add("@i_retry_type", OracleDbType.Int16, 1, ParameterDirection.Input);
                                cmd.Parameters.Add("@i_section", OracleDbType.Int16, 1, ParameterDirection.Input);
                                cmd.Parameters.Add("@i_preview", OracleDbType.Varchar2, 90, script.Substring(0,80), ParameterDirection.Input);
                                cmd.Parameters.Add("@i_upg_command", OracleDbType.Clob, script, ParameterDirection.Input);
                                cmd.Parameters.Add("@i_success_ind", OracleDbType.Char, 1, "N", ParameterDirection.Input);

                                //execute
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    result = true;
                }

            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("Step 1 Error: " + ex.Message);
                Console.WriteLine("Step 1 Error: " + ex.Message);
                result = false;
            }
            finally
            {
                // close the connection
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return result;
        }

    }
}
