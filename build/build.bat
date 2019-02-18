@ECHO OFF

SET Arbor.X.Build.PublishRuntimeIdentifier=win10-x64

CALL dotnet arbor-build

EXIT /B %ERRORLEVEL%
