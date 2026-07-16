@echo off
title MCWPMS Dev Launcher
cd /d "%~dp0"

:: 1. Force kill existing dotnet processes to release locked dll files
echo Stopping existing processes...
taskkill /f /im dotnet.exe >nul 2>nul
taskkill /f /im VBCSCompiler.exe >nul 2>nul
taskkill /f /im python.exe >nul 2>nul

:: 2. Clean temporary build artifacts
echo Cleaning solution...
dotnet clean MangaPublishingSystem.sln -c Debug >nul 2>nul

:: 3. Build the solution sequentially to prevent parallel write conflicts on shared dependencies
echo Building solution...
dotnet build MangaPublishingSystem.sln -c Debug

:: 4. Initialize Database
echo Initializing database schema and seed data...
sqlcmd -S localhost -f 65001 -i Database\schema.sql
sqlcmd -S localhost -f 65001 -i Database\seed.sql
    echo Launching services in Windows Terminal tabs...
    wt -w 0 nt -d "%cd%" cmd /k "title GatewayAPI && dotnet watch run --project GatewayAPI/GatewayAPI.csproj" ; nt -d "%cd%" cmd /k "title MangaPublishingSystem && dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj" ; nt -d "%cd%\Services\AiVisionService" cmd /k "title AiVisionService && .\venv\Scripts\python.exe -m uvicorn main:app --port 8000 --reload"
