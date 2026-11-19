@echo off
rem Ejecuta las 37 pruebas de la logica de negocio (no necesita base de datos)
cd /d "%~dp0"
dotnet run --project pruebas\eVidencia.Pruebas
pause
