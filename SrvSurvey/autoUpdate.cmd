@echo off

:: Prep folder for old build
if exist %1\old ( rd %1\old /s /q )
md %1\old

:: Backup old build
xcopy %2 %1\old /E /Q

:: Remove old build
rd %2 /s /q
md %2

:: Deploy the new build
xcopy %1\new %2 /E /Q

:: Restart SrvSurvey again
start "" %2\SrvSurvey.exe

