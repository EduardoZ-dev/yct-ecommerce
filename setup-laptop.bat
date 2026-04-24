@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM  YCT - Setup automatico para portatil (SQL Server: JOSEDAZA)
REM ============================================================

set "SQL_SERVER=JOSEDAZA"
set "DB_NAME=YctDb"
set "ROOT=%~dp0"

echo.
echo ============================================================
echo  YCT - Setup en portatil
echo  SQL Server: %SQL_SERVER%
echo  Base de datos: %DB_NAME%
echo ============================================================
echo.

REM --- 1) Actualizar connection string ---
echo [1/5] Ajustando connection string en appsettings.json...
powershell -NoProfile -Command ^
  "$p = '%ROOT%Back\YCT.API\appsettings.json';" ^
  "$j = Get-Content $p -Raw | ConvertFrom-Json;" ^
  "$j.ConnectionStrings.DefaultConnection = 'Server=%SQL_SERVER%;Database=%DB_NAME%;Trusted_Connection=True;TrustServerCertificate=True';" ^
  "$j | ConvertTo-Json -Depth 10 | Set-Content $p -Encoding UTF8"
if errorlevel 1 goto :error

REM --- 2) Instalar dotnet-ef si hace falta ---
echo.
echo [2/5] Verificando dotnet-ef...
dotnet ef --version >nul 2>&1
if errorlevel 1 (
  echo   Instalando dotnet-ef global...
  dotnet tool install --global dotnet-ef
  if errorlevel 1 goto :error
) else (
  echo   Ya instalado.
)

REM --- 3) Aplicar migraciones (crea la BD + tablas) ---
echo.
echo [3/5] Creando base de datos y aplicando migraciones...
pushd "%ROOT%Back\YCT.API"
dotnet ef database update --project ..\YCT.Infrastructure
if errorlevel 1 (popd & goto :error)
popd

REM --- 4) Cargar datos semilla ---
echo.
echo [4/5] Cargando datos semilla (categorias, productos, admin)...
sqlcmd -S %SQL_SERVER% -d %DB_NAME% -E -i "%ROOT%Back\db-scripts\seed.sql"
if errorlevel 1 goto :error

REM --- 5) npm install del frontend ---
echo.
echo [5/5] Instalando dependencias del frontend...
pushd "%ROOT%Front\yct-frontend"
call npm install
if errorlevel 1 (popd & goto :error)
popd

echo.
echo ============================================================
echo  SETUP COMPLETO
echo ============================================================
echo.
echo  Para arrancar todo:
echo    - Backend:   cd Back\YCT.API ^&^& dotnet run
echo    - Frontend:  cd Front\yct-frontend ^&^& npm start
echo    - O usa:     dev.bat (arranca ambos)
echo.
echo  Credenciales admin: admin / admin123
echo.
pause
exit /b 0

:error
echo.
echo ============================================================
echo  ERROR - Revisa el mensaje arriba
echo ============================================================
pause
exit /b 1
