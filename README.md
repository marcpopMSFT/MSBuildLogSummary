# MSBuildLogSummary

This utility is for listing out the Projects, Targets, Tasks, and Properties for a binlog.  It uses the structured log viewer package. There code to try to remove all version specific values so that they can be compared.

# Scripts
Prerequesities: Have git installed
1. dotnet publish -c release -r win-x64 
2. Copy result into a MSBuildLogSummary folder
3. From Powershell one level up from the LogSummary folder: .\DiffVersions.ps1 "5.0.202" "6.0.100-preview.3.21174.8" console

# Saved Diffs
I saved diffs for console, wpf, and blazorserver comparing 5.0.2xx with 5.0.1xx and comparing 5.0.2xx with 6.0.1xx-preview3.
