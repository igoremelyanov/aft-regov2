@echo off
cls

echo starting build.bat ...
echo.

if not exist "libs\FSharp.Collections.ParallelSeq\lib\net40\FSharp.Collections.ParallelSeq.dll" (
	echo no FSharp.Collections.ParallelSeq package detected, updating from package repository ...
	".nuget\NuGet.exe" "Install" "FSharp.Collections.ParallelSeq" "-OutputDirectory" "libs" "-ExcludeVersion"
)

if not exist "libs\FAKE\tools\FAKE.exe" (
	echo no FAKE package detected, updating from package repository ...
	".nuget\NuGet.exe" "Install" "FAKE" -Version 4.5.3 "-OutputDirectory" "libs" "-ExcludeVersion"
)

echo starting Fake.exe build.fsx ...
echo.

libs\FAKE\tools\Fake.exe build.fsx buildTarget=%1 buildStepRepeatCount=%2 performanceTesting=%3 action=%4 dropDb=%5 pass=%6

pause