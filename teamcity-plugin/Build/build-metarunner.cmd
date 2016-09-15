RMDIR "%~dp0..\Drops" /S /Q
MKDIR "%~dp0..\Drops"
MKDIR "%~dp0..\Drops\Temp"
MKDIR "%~dp0..\Drops\Temp\agent"
MKDIR "%~dp0..\Drops\Temp\agent\bin"

XCOPY /E /Y "%~dp0..\Solutions\*.*" "%~dp0..\Drops\Temp"
XCOPY /E /Y  "%~dp0..\..\src\OctopusPuppet.Cmd\bin\Release\*.*" "%~dp0..\Drops\Temp\agent\bin"
DEL "%~dp0..\Drops\Temp\agent\bin\*.xml"
DEL "%~dp0..\Drops\Temp\agent\bin\*.pdb"

@PowerShell -File %~dp0Create-Zip.ps1 %~dp0..\Drops\Temp\agent\ %~dp0..\Drops\Temp\agent\octopuspuppet-metarunner.zip

DEL "%~dp0..\Drops\Temp\agent\*.xml"
RMDIR "%~dp0..\Drops\Temp\agent\bin" /S /Q

@PowerShell -File "%~dp0Create-Zip.ps1" -target %~dp0..\Drops\Temp\ %~dp0..\..\octopuspuppet-metarunner.zip

RMDIR "%~dp0..\Drops\Temp\" /S /Q