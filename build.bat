@echo off
title Restore + Build BE-SWP391
cd /d "%~dp0"

echo Restoring packages...
dotnet restore MangaPublishingSystem.sln
if %ERRORLEVEL% neq 0 goto :fail

echo Building...
dotnet build MangaPublishingSystem.sln
if %ERRORLEVEL% neq 0 goto :fail

echo.
echo OK - 0 errors. Neu IDE van do: dong Cursor mo lai 1 lan.
pause
exit /b 0

:fail
echo FAILED.
pause
exit /b 1
