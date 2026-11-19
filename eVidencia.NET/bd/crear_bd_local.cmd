@echo off
rem Crea la base de datos local eVidenciaDemo en LocalDB (se instala con Visual Studio)
cd /d "%~dp0"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i 01_crear_bd_local.sql
pause
