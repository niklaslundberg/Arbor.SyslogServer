@ECHO OFF

SET Arbor.X.Build.PublishRuntimeIdentifier=win10-x64

CALL dotnet arbor-build

IF "%ERRORLEVEL%" NEQ "0" (
    EXIT /B %ERRORLEVEL%
)

CD %~dp0

CD ..

CD docker

powershell build.ps1

EXIT /B %ERRORLEVEL%
