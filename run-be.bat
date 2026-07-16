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

:: 4. Initialize Database (disabled by default to avoid losing local test data)
:: echo Initializing database schema and seed data...
:: sqlcmd -S localhost -d master -Q "IF DB_ID('MangaPublishing') IS NULL CREATE DATABASE MangaPublishing"
:: sqlcmd -S localhost -d MangaPublishing -f 65001 -i Database\schema.sql
:: sqlcmd -S localhost -d MangaPublishing -f 65001 -i Database\seed.sql

:: Check if Windows Terminal (wt.exe) is available
where wt >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo Launching services in Windows Terminal tabs...
    wt -w 0 nt -d "%cd%" cmd /k "title GatewayAPI && dotnet watch run --project GatewayAPI/GatewayAPI.csproj" ; nt -d "%cd%" cmd /k "title MangaPublishingSystem && dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj" ; nt -d "%cd%\Services\AiVisionService" cmd /k "title AiVisionService && .\venv\Scripts\python.exe -m uvicorn main:app --port 8000 --reload"
) else (
    echo Windows Terminal not found.
    echo Falling back to launching in separate command windows...

    echo Launching GatewayAPI...
    start "GatewayAPI - dotnet watch" cmd /k dotnet watch run --project GatewayAPI/GatewayAPI.csproj

    echo Launching MangaPublishingSystem...
    start "MangaPublishingSystem - dotnet watch" cmd /k dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj

    echo Launching AiVisionService...
    start "AiVisionService - uvicorn" cmd /k "cd Services\AiVisionService && .\venv\Scripts\python.exe -m uvicorn main:app --port 8000 --reload"
)
