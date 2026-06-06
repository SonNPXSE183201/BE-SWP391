@echo off
title MCWPMS Dev Launcher
cd /d "%~dp0"

:: Check if Windows Terminal (wt.exe) is available
where wt >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo Launching services in Windows Terminal tabs...
    wt -d "%cd%" cmd /k dotnet watch run --project GatewayAPI/GatewayAPI.csproj ^; new-tab -d "%cd%" cmd /k dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
) else (
    echo Windows Terminal not found.
    echo Falling back to launching in 2 separate command windows...
    
    echo Launching GatewayAPI...
    start "GatewayAPI - dotnet watch" cmd /k dotnet watch run --project GatewayAPI/GatewayAPI.csproj
    
    echo Launching MangaPublishingSystem...
    start "MangaPublishingSystem - dotnet watch" cmd /k dotnet watch run --project Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj
)
