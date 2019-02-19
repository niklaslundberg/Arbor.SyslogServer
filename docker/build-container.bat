CD %~dp0
RD /S /Q temp
MKDIR temp
CD ..
CD Artifacts
CD WebSites
CD Arbor.SyslogServer

DIR /B > files.user
SET /P Configuration=<files.user

CD %CONFIGURATION%

SET SourceFiles=%CD%

xcopy %SourceFiles% %~dp0temp

docker build -t arbor-syslogserver:latest %~dp0\docker\temp
