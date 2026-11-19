@echo off
rem Arranca la demo y abre el navegador en http://localhost:5080
cd /d "%~dp0"
dotnet run --project src\eVidencia.Web --launch-profile eVidencia.Web
pause
