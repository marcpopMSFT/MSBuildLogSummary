extern alias StructuredLogger;
using System;
using System.IO;
using System.Linq;
using StructuredLogger.Microsoft.Build.Logging.StructuredLogger;

namespace MSBuildBinLogSummarizer
{
    class Program
    {
        static void Main(FileInfo logFile)
        {
            if (logFile == null)
            {
                Console.WriteLine("Filename missing");
                return;
            }

            if (!logFile.Extension.EndsWith(".binlog", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Input is not a .binlog");
                return;
            }

            ProcessBinaryLog(logFile.FullName);
        }

        private static void ProcessBinaryLog(string fullName)
        {
            var util = new StructuredLoggerUtil(fullName);

            if (!util.Succeeded)
            {
                Console.WriteLine("Build Failed");
            }
            else
            {
                Console.WriteLine("Build Succceeded", ConsoleColor.Green);
            }

            foreach (var (ProjectFile, Message, TargetNames) in util.Projects)
            {
                Console.WriteLine("Project " + ProjectFile + "," + Message + "," + TargetNames);
            }

            foreach (var (TargetName, ParentTarget, TargetFile) in util.Targets)
            {
                Console.WriteLine("Target " + TargetName + "," + ParentTarget + "," + TargetFile);
            }

            foreach (var (TaskName, TaskFile, SenderName) in util.Tasks)
            {
                Console.WriteLine("Task " + TaskName + "," + TaskFile + "," + SenderName);
            }

            foreach (var ( ItemName,  ItemSpec, MetadataCount) in util.Items)
            {
                Console.WriteLine("Item " + ItemName + "," + ItemSpec + "," + MetadataCount);
            }

            foreach (var (PropertyName, PropertyValue) in util.Properties)
            {
                Console.WriteLine("Property " + PropertyName + "," + PropertyValue);
            }
        }
    }
}
