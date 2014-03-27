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
            Trace.WriteLine("Step 0: Read in the params.txt file");
            stepSuccess = LoadParameters();

            #region Prepare for Export

            // Step 1a: Upsert the version tables in the database
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success

                stepSuccess = true; // replace with method call
            }

            // Step 1b: Update the upgrade_host_script table
            // not sure how mant of these will be used with this new automation
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success

                stepSuccess = true; // replace with method call
            }

            // Step 1c: Update the upgrade db objects in the database
            if (stepSuccess == true)
            {
                Console.WriteLine("Step 1c: Update the upgrade db objects in the database");
                Trace.WriteLine("Step 1c: Update the upgrade db objects in the database");
                stepSuccess = false; // reset the step success
                stepSuccess = UpdateUpgradeObjects();
            }

            // Step 1d: Insert Upgrade Commands from TFS
            if (stepSuccess == true)
            {
                Console.WriteLine("Step 1d: Insert Upgrade Commands from TFS");
                Trace.WriteLine("Step 1d: Insert Upgrade Commands from TFS");
                stepSuccess = false; // reset the step success
                stepSuccess = InsertUpgradeCommands();
            }

            // Step 1e: Prepare the CLR assemblies
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success

                stepSuccess = true; // replace with method call
            }

            #endregion

            #region Create the Export
            
            // Step 2: Create the Export
            if (stepSuccess == true)
            {
                Console.WriteLine("Step 2: Create the export");
                Trace.WriteLine("Step 2: Create the export");
                //TODO: replace the arguments with the correct list for a migration export
                string expArgs = string.Format("upgtest/dang3r dumpfile=EXP_{0}_{1}.dmp logfile=EXP_{0}_{1}.log reuse_dimpfiles=y schemas=upgtest", parameters["VersionFrom"], parameters["VersionTo"]);
                string expResult = CmdLineWrapper.RunCmdLine("expdp.exe", expArgs);
                Console.WriteLine("Export Result: " + expResult);
                stepSuccess = false; // reset the step success
            }

            #endregion

            #region Run the Upgrade
            
            // Step 3: Importing the export
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            // Step 4: Running the upgrade
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            // Step 5: Running the finalize
            if (stepSuccess == true)
            {
                stepSuccess = false; // reset the step success
                // move the export file to the appropriate directory
            }

            #endregion

            Console.WriteLine("Hit enter key to end.");
            Console.ReadLine();
        }

        #region DB Migration Methods
        
        private static bool LoadParameters()
        {
            // find the upgrade directory in the source directory
            // Here is the list of parameters in the params.txt file:
            // VersionFrom
            // VersionTo

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
                Trace.WriteLine("Params read from file.");
                return true;
            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("LoadParameters Error: " + ex.Message);
                Console.WriteLine("LoadParameters Error: " + ex.Message);
                return false;
            }
        }

        private static bool UpdateUpgradeObjects()
        {
            bool result = false;
            OracleConnection conn = new OracleConnection(connString);

            // Update the packages and other db objects that are used by the upgrade process
            // NOTE: it is critical that the ddl scripts include an "ALTER PACKAGE ... COMPILE" 
            // statement at the end of each to avoid error in next step of process
            try
            {
                // open the db connection once for all inserts
                conn.Open();
                string[] files = GetSortedFilenamesFromDirectory(upgradePath + "\\Config\\upgrade-ddl");

                foreach (string fileName in files)
                {
                    // get the contents of the script 
                    StreamReader file = new StreamReader(fileName);
                    string ddl = file.ReadToEnd();
                    // execute the ddl
                    using (OracleCommand ddlCmd = new OracleCommand(ddl, conn))
                    {
                        ddlCmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("Upgrade objects updated");
                Trace.WriteLine("Upgrade objects updated");
                result = true;
            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("UpdateUpgradeObjects Error: " + ex.Message);
                Console.WriteLine("UpdateUpgradeObjects Error: " + ex.Message);
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

        private static bool InsertUpgradeCommands()
        {
            bool result = false;
            OracleConnection conn = new OracleConnection(connString);

            try
            {
                // prepare the upgrade command table
                //TODO: ensure it is correct to truncate these tables prior to inserting new commands
                //TODO: refactor executing a simple sql command
                conn.Open();
                using (OracleCommand truncateCmd = new OracleCommand("truncate table rt_upgrade_command", conn))
                {
                    truncateCmd.ExecuteNonQuery();
                }
                conn.Close();

                SortedDictionary<int, string> sortedFolders = GetSortedFolderNamesFromDirectory(upgradePath, "*", true);

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
                    Console.WriteLine("Upgrade commands inserted");
                    Trace.WriteLine("Upgrade commands inserted");
                    result = true;
                }

            }
            catch (Exception ex)
            {
                //TODO: implement desired error handling
                Trace.WriteLine("InsertUpgradeCommands Error: " + ex.Message);
                Console.WriteLine("InsertUpgradeCommands Error: " + ex.Message);
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

        #endregion

        #region Utilities

        private static SortedDictionary<int, string> GetSortedFolderNamesFromDirectory(string path, string wildcard, bool NumericOnly)
        {
            // get the directories
            // TODO: refactor getting sorted files from a directory and getting sorted numeric directories
            string[] folders = System.IO.Directory.GetDirectories(path, wildcard, System.IO.SearchOption.TopDirectoryOnly);
            //load the directories into a sorted list
            SortedDictionary<int, string> sortedFolders = new SortedDictionary<int, string>();
            foreach (var dir in folders)
            {
                int dirId;
                // get just the last directory name
                string folderName = dir.Substring(dir.Length - 3, 3);
                if (Int32.TryParse(folderName, out dirId))
                {
                    sortedFolders.Add(dirId, dir);
                }

            }
            return sortedFolders;
        }

        private static string[] GetSortedFilenamesFromDirectory(string path)
        {
            // get files from the directory
            string[] files = Directory.GetFiles(path);
            // ensure the files are in sorted order
            Array.Sort(files);
            return files;
        }

        #endregion
    }
}
