@echo off
title Manga Service (5010 only)
cd /d "%~dp0"

echo.
echo [1/3] Stopping Manga Service on port 5010...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr "LISTENING" ^| findstr /C:":5010 "') do taskkill /F /PID %%a >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/3] Build Manga Service...
dotnet build Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
if %ERRORLEVEL% neq 0 (
    echo.
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo [3/3] Starting Manga Service on http://localhost:5010 ...
echo Swagger: http://localhost:5010/swagger
echo.

dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
