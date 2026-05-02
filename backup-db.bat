@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM  YCT - Backup automatico de la base de datos
REM
REM  Genera un .bak con la fecha y hora en %BACKUP_DIR%.
REM  Mantiene los ultimos 14 backups y borra los mas antiguos.
REM
REM  Para ejecutarlo cada noche:
REM    1. Programador de tareas (taskschd.msc)
REM    2. Crear tarea basica -> diaria a las 23:00
REM    3. Accion: iniciar programa -> ruta a este .bat
REM ============================================================

set "SQL_SERVER=JOSEDAZA"
set "DB_NAME=YctDb"
set "BACKUP_DIR=%~dp0backups"
set "RETENTION_DAYS=14"

REM --- Asegurar carpeta de backups ---
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

REM --- Construir nombre con timestamp YYYY-MM-DD_HH-mm ---
for /f "tokens=2 delims==" %%a in ('wmic os get localdatetime /value ^| find "="') do set "DT=%%a"
set "STAMP=%DT:~0,4%-%DT:~4,2%-%DT:~6,2%_%DT:~8,2%-%DT:~10,2%"
set "BACKUP_FILE=%BACKUP_DIR%\%DB_NAME%_%STAMP%.bak"

echo.
echo ============================================================
echo  YCT - Backup de %DB_NAME%
echo  Servidor: %SQL_SERVER%
echo  Destino : %BACKUP_FILE%
echo ============================================================
echo.

REM --- Ejecutar backup via T-SQL ---
sqlcmd -S %SQL_SERVER% -E -Q "BACKUP DATABASE [%DB_NAME%] TO DISK = N'%BACKUP_FILE%' WITH NOFORMAT, INIT, NAME = N'YctDb-Full', SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10"

if errorlevel 1 (
  echo.
  echo ERROR al generar el backup. Revisa que SQL Server este corriendo
  echo y que el usuario tenga permisos sobre %DB_NAME%.
  pause
  exit /b 1
)

echo.
echo Backup creado correctamente.
echo.

REM --- Limpiar backups mas antiguos que %RETENTION_DAYS% dias ---
echo Eliminando backups con mas de %RETENTION_DAYS% dias en %BACKUP_DIR%...
forfiles /p "%BACKUP_DIR%" /m "%DB_NAME%_*.bak" /d -%RETENTION_DAYS% /c "cmd /c del @path" 2>nul

echo.
echo Backups actuales:
dir /b /o-d "%BACKUP_DIR%\%DB_NAME%_*.bak" 2>nul

echo.
echo ============================================================
echo  LISTO - Conserva una copia en OneDrive/Drive si puedes.
echo ============================================================
exit /b 0
