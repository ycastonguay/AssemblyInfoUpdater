// Copyright © 2011-2014 Yanick Castonguay
//
// This file is part of AssemblyInfoUpdater.
//
// AssemblyInfoUpdater is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// AssemblyInfoUpdater is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with AssemblyInfoUpdater. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace AssemblyInfoUpdater
{
    class Program
    {
        private static Timer _timer = null;
        private static DateTime _dateTimeRefresh = DateTime.Now;
        private static List<Tuple<string, string>> _options = new List<Tuple<string, string>>();

        static void Main(string[] args)
        {
            // Validate params
            if (args.Length == 0)
            {
                LogWithoutTimestamp("Error: The first parameter must be the folder path!");
                LogWithoutTimestamp("");
                PrintHelp();
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                LogWithoutTimestamp("Error: The folder path doesn't exist (" + args[0] + ")");
                LogWithoutTimestamp("");
                PrintHelp();
                return;
            }

            // Check for options
            for (int index = 1; index < args.Length; index += 2) // skip first argument which is the target path
            {
                string t = args[index];
                if (t.StartsWith("--"))
                    _options.Add(new Tuple<string, string>(t, args[index+1]));
            }

            string[] assemblyInfoFilePaths;
            try
            {
                Log("Finding AssemblyInfo.cs files...");
                assemblyInfoFilePaths = FindAssemblyInfoFiles(args[0], true);
            }
            catch (Exception ex)
            {
                Log(string.Format("An error occured while trying to find AssemblyInfo.cs files: {0}", ex));
                return;
            }

            // Check for options
            var setVersion = _options.FirstOrDefault(x => x.Item1.ToUpper() == "--SETVERSION");            
            if(setVersion != null)
            {
                try
                {
                    foreach (string assemblyInfoFilePath in assemblyInfoFilePaths)
                    {
                        Log(string.Format("Updating {0}...", assemblyInfoFilePath));
                        SetAssemblyInfoVersion(assemblyInfoFilePath, setVersion.Item2);
                    }
                }
                catch (Exception ex)
                {
                    Log(string.Format("An error occured while trying to update version in AssemblyInfo.cs files: {0}", ex));
                    return;
                }
            }

            Log("AssemblyInfo.cs files updated successfully.");
        }

        public static void PrintHelp()
        {
            LogWithoutTimestamp("");
            LogWithoutTimestamp("Assembly Info Updater Tool v1.0 (C) 2014 Yanick Castonguay");
            LogWithoutTimestamp("\n");
            LogWithoutTimestamp("This tool updates the AssemblyInfo.cs files located in the specified directory and its children.");
            LogWithoutTimestamp("\n");
            LogWithoutTimestamp("Option list:");
            LogWithoutTimestamp("--setversion: Sets assembly version.");
            LogWithoutTimestamp("\n");
            LogWithoutTimestamp("Usage:     AssemblyInfoUpdater.exe [FolderPath]");
            LogWithoutTimestamp("Examples:  AssemblyInfoUpdater.exe C:\\Code\\MPfm\\MPfm --setversion 0.7.0.0");
            LogWithoutTimestamp("           AssemblyInfoUpdater.exe ~/Projects/MPfm/MPfm --setversion 0.7.0.0");
        }

        private static void Log(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] " + message);
        }

        private static void LogWithoutTimestamp(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        public static string[] FindAssemblyInfoFiles(string folderPath, bool recursive)
        {
            return Directory.GetFiles(folderPath, "AssemblyInfo.cs", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static void SetAssemblyInfoVersion(string filePath, string version)
        {
            // Read all lines of the file
            var lines = File.ReadLines(filePath).ToList();

            // Try to find which line contains the version (we expect this line to exist!)
            var newLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("[assembly: AssemblyVersion("))
                    newLines.Add(string.Format("[assembly: AssemblyVersion(\"{0}\")]", version));
                else if (line.StartsWith("[assembly: AssemblyFileVersion("))
                    newLines.Add(string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version));
                else
                    newLines.Add(line);
            }

            // Rewrite the file
            using (var textWriter = new StreamWriter(filePath))
                foreach (string line in newLines)
                    textWriter.WriteLine(line);
        }
    }
}
