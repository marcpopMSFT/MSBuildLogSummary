# set the global.json, run the new project template, build
function CreateBinlog($Version, $ProjectType)
{
    Remove-Item $ProjectType -Recurse -Force
    mkdir $ProjectType
    cd $ProjectType
    "{""sdk"": { ""version"": ""$Version""    }  }" | Set-Content "global.json"
    $version = dotnet --version
    if ($version -ne $Version)
    {
        return 2
    }
    
    dotnet new $ProjectType
    dotnet build /bl
    copy "msbuild.binlog" "..\Result\msbuild.$ProjectType.$Version.binlog"
    cd ..
}

if($args.Length -gt 2)
{
    $SDKVersion1 = $args[0]
    $SDKVersion2 = $args[1]
    $ProjectType = $args[2]
}
else {
    return 1;
}

#Clean the results folder
Remove-Item "Result" -Recurse -Force
mkdir "Result"

# create binlogs for each project type
CreateBinlog $SDKVersion1 $ProjectType
CreateBinlog $SDKVersion2 $ProjectType

# Dump the summary of each binlog and diff them
Set-Location .\Result
..\MSBuildLogSummary\MSBuildBinLogSummary.exe --log-file msbuild.$ProjectType.$SDKVersion1.binlog | Set-Content Summary.$ProjectType.$SDKVersion1.txt 
..\MSBuildLogSummary\MSBuildBinLogSummary.exe --log-file msbuild.$ProjectType.$SDKVersion2.binlog | Set-Content Summary.$ProjectType.$SDKVersion2.txt
git diff --text --minimal Summary.$ProjectType.$SDKVersion1.txt Summary.$ProjectType.$SDKVersion2.txt | Set-Content Diff.$ProjectType.$SDKVersion1.$SDKVersion2.txt


Remove-Item ..\Diff.$ProjectType.$SDKVersion1.$SDKVersion2.txt

# Trim out the lines that aren't different
$file = Get-Content Diff.$ProjectType.$SDKVersion1.$SDKVersion2.txt
Foreach($line in $file)
{
if ($line -match "^[\-\+]")
{
$line | Add-Content ..\Diff.$ProjectType.$SDKVersion1.$SDKVersion2.txt
}
}

#copy the result here
Set-Location ..

return 0



