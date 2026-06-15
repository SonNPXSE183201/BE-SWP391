@echo off
title Start SQL Server SQL2022
echo Starting SQL Server (SQL2022)...
net start "MSSQL$SQL2022"
if %ERRORLEVEL% neq 0 (
    echo.
    echo FAILED. Right-click this file and choose "Run as administrator".
    pause
    exit /b 1
)
echo.
echo SQL Server SQL2022 is running.
pause
