@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM  YCT - Restaurar la base de datos desde un .bak
REM
REM  Uso:  restore-db.bat ruta\al\backup.bak
REM        restore-db.bat                     (usa el mas reciente)
REM ============================================================

set "SQL_SERVER=JOSEDAZA"
set "DB_NAME=YctDb"
set "BACKUP_DIR=%~dp0backups"

if "%~1"=="" (
  echo Buscando el backup mas reciente en %BACKUP_DIR%...
  for /f "delims=" %%f in ('dir /b /o-d "%BACKUP_DIR%\%DB_NAME%_*.bak" 2^>nul') do (
    set "BACKUP_FILE=%BACKUP_DIR%\%%f"
    goto :found
  )
  echo No se encontraron backups en %BACKUP_DIR%.
  exit /b 1
) else (
  set "BACKUP_FILE=%~1"
)

:found
if not exist "%BACKUP_FILE%" (
  echo ERROR: no existe el archivo "%BACKUP_FILE%"
  exit /b 1
)

echo.
echo ============================================================
echo  ATENCION - Esto sobrescribira la base %DB_NAME%
echo  Archivo: %BACKUP_FILE%
echo ============================================================
echo.
set /p CONFIRM=Escribe SI para continuar:
if /i not "%CONFIRM%"=="SI" (
  echo Cancelado.
  exit /b 0
)

REM --- Forzar single-user mode, restaurar y volver a multi-user ---
sqlcmd -S %SQL_SERVER% -E -Q "ALTER DATABASE [%DB_NAME%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
sqlcmd -S %SQL_SERVER% -E -Q "RESTORE DATABASE [%DB_NAME%] FROM DISK = N'%BACKUP_FILE%' WITH REPLACE, STATS = 10"
sqlcmd -S %SQL_SERVER% -E -Q "ALTER DATABASE [%DB_NAME%] SET MULTI_USER"

if errorlevel 1 (
  echo.
  echo ERROR al restaurar. Revisa el mensaje arriba.
  pause
  exit /b 1
)

echo.
echo ============================================================
echo  Base de datos restaurada correctamente.
echo ============================================================
exit /b 0
