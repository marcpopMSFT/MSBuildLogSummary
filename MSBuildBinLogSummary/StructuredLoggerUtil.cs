
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;

namespace MSBuildBinLogSummarizer
{
    /// <summary>
    /// 
    /// </summary>
    public class StructuredLoggerUtil
    {
        private readonly List<(string Code, string Text, DateTime Timestamp)> errors =
            new List<(string Code, string Text, DateTime Timestamp)>();

        private readonly List<(string Code, string Text, DateTime Timestamp)> warnings =
            new List<(string Code, string Text, DateTime Timestamp)>();


        private readonly List<(string TaskName, string TaskFile, string SenderName)> tasks =
            new List<(string TaskName, string TaskFile, string SenderName)>();


        private readonly List<(string TargetName, string TargetFile, string ParentTarget)> targets =
            new List<(string TargetName, string TargetFile, string ParentTarget)>();

        private readonly List<(string Targets, string ProjectFile, string Message)> projects =
            new List<(string Targets, string ProjectFile, string Message)>();

        private readonly List<(string ItemName, string ItemSpec, int MetadataCount)> items =
            new List<(string ItemName, string ItemSpec, int MetadataCount)>();

        private readonly List<(string PropertyName, string PropertyValue)> properties =
            new List<(string PropertyName, string PropertyValue)>();

        private readonly HashSet<string> listToRemove = new HashSet<string>() { "KnownFrameworkReference", "KnownAppHostPack", "KnownCrossgen2Pack", "_AllDirectoriesAbove", "_OutputPathItem", 
                                                                                "MSBuildToolsPath", "MSBuildStartupDirectory", "MSBuildProjectDirectory", "MSBuildProjectDirectoryNoRoot", 
                                                                                "DOTNET_CLI_TELEMETRY_SESSIONID", "DefineConstants", "ImplicitFrameworkDefine",};

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName"></param>
        public StructuredLoggerUtil(string fileName)
        {
            var reader = new BinLogReader();

            reader.AnyEventRaised += (_, e) => AnyEvent(_, e);
            reader.WarningRaised += (_, e) => Trackwarnings(e);
            reader.ErrorRaised += (_, e) => TrackErrors(e);
            reader.BuildFinished += (_, e) => Succeeded = GetSucceeded(e);
            reader.TargetStarted += (_, e) => TargetEvent(_, e);
            reader.TaskStarted += (_, e) => TaskEvent(_, e);
            reader.ProjectStarted += (_, e) => ProjectEvent(_, e);
            reader.Replay(fileName);

        }

        private void AnyEvent(object _, BuildEventArgs e)
        {

        }

        private void ProjectEvent(object _, ProjectStartedEventArgs e)
        {
            this.projects.Add((e.TargetNames,RemovePath(e.ProjectFile), e.Message ));
            if (e.TargetNames == "" && e.Message.Contains("default"))
            {
                RecordItems(e.Items);
                RecordProperties(e.Properties);
            }
        }

        private void RecordProperties(IEnumerable properties)
        {
            foreach (KeyValuePair<string,string> property in properties)
            {
                if (!RemoveVersionSpecificNames(property.Key.ToString()) && !RemoveVersionSpecificValues(property.Value.ToString()))
                {
                    this.properties.Add((property.Key.ToString(), RemovePath(property.Value.ToString())));
                }
            }
        }

        private void RecordItems(IEnumerable items)
        {
            foreach (DictionaryEntry item in items)
            {
                if (!RemoveVersionSpecificNames(item.Key.ToString()))
                {
                    Microsoft.Build.Framework.ITaskItem taskItem = (Microsoft.Build.Framework.ITaskItem)item.Value;
                    if (RemoveVersionSpecificValues(taskItem.ItemSpec))
                        continue;
                    this.items.Add((item.Key.ToString(), RemovePath(taskItem.ItemSpec), taskItem.CloneCustomMetadata().Count));
                }
            }
        }

        private bool RemoveVersionSpecificNames(string v)
        {
            if (v.Contains("Version"))
                return true;

            if (listToRemove.Contains(v))
                return true;

            return false;
        }

        private bool RemoveVersionSpecificValues(string v)
        {
            if (v.Contains("5.0") || v.Contains("6.0") || v.Contains("3.1") || v.Contains("3.0"))
                return true;

            return false;
        }

        private void TargetEvent(object _, TargetStartedEventArgs e)
        {
            this.targets.Add((e.TargetName, RemovePath(e.TargetFile), e.ParentTarget ));
        }
        private void TaskEvent(object _, TaskStartedEventArgs e)
        {
            this.tasks.Add((e.TaskName, RemovePath(e.TaskFile), e.SenderName));
        }

        private static bool GetSucceeded(BuildFinishedEventArgs e)
        {
            return e.Succeeded;
        }

        private static string RemovePath(string File)
        {
            var splitString = File.Split('\\');
            string result = "";
                
            if (splitString.Length> 0)
                result = splitString[splitString.Length - 1];

            if (string.IsNullOrEmpty(result) && splitString.Length > 1)
                result = splitString[splitString.Length - 2];

            return result;

        }

        private void TrackErrors(BuildErrorEventArgs e)
        {
            this.errors.Add((e.Code, e.Message, e.Timestamp));
        }

        private void Trackwarnings(BuildWarningEventArgs e)
        {
            this.warnings.Add((e.Code, e.Message, e.Timestamp));
        }



        /// <summary>
        /// Success vs. Failure
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Errors
        /// </summary>
        public IEnumerable<(string Code, string Text, DateTime Timestamp)> Errors => this.errors.AsReadOnly();

        /// <summary>
        /// Warnings
        /// </summary>
        public IEnumerable<(string Code, string Text, DateTime Timestamp)> Warnings => this.warnings.AsReadOnly();


        /// <summary>
        /// Targets
        /// </summary>
        public IEnumerable<(string TargetName, string TargetFile,string ParentTarget )> Targets => this.targets.AsReadOnly();


        /// <summary>
        /// Tasks
        /// </summary>
        public IEnumerable<(string TaskName, string TaskFile, string SenderName)> Tasks => this.tasks.AsReadOnly();

        /// <summary>
        /// Projects
        /// </summary>
        public IEnumerable<(string Targets, string ProjectFile, string Message)> Projects => this.projects.AsReadOnly();


        /// <summary>
        /// Items
        /// </summary>
        public IEnumerable<(string ItemName, string ItemSpec, int MetadataCount)> Items => this.items.AsReadOnly();

        /// <summary>
        /// Properties
        /// </summary>
        public IEnumerable<(string PropertyName, string PropertyValue)> Properties => this.properties.AsReadOnly();
    }
}
