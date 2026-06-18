@echo off
echo Starting Ocelot Gateway (Port 5000)...
start "Ocelot Gateway" cmd /c "cd GatewayAPI && dotnet run"

echo Starting Presentation API Logic (Port 5010)...
start "Presentation API" cmd /c "cd Services\MangaPublishingSystem\MangaPublishingSystem.Presentation && dotnet run"

echo ========================================================
echo [OK] Both Backend services have been started in new windows!
echo - Gateway is running on: http://localhost:5000
echo - Presentation is running on: http://localhost:5010
echo ========================================================
pause
