@echo off
REM ============================================
REM  Distribuciones YCT - Dev Server Launcher
REM  Ejecuta backend (.NET) y frontend (Angular)
REM  cada uno en su propia ventana de consola.
REM ============================================

echo.
echo ========================================
echo   YCT - Iniciando entorno de desarrollo
echo ========================================
echo.
echo  Backend  -^> http://localhost:5088
echo  Frontend -^> http://localhost:4200
echo.
echo  Cada servidor se abre en su propia ventana.
echo  Cierra cada ventana para detener el servidor.
echo.

REM Backend .NET (nueva ventana)
start "YCT Backend (.NET 9)" cmd /k "cd /d %~dp0Back\YCT.API && echo === BACKEND .NET === && dotnet run"

REM Pequena pausa para que el backend inicie primero
timeout /t 2 /nobreak >nul

REM Frontend Angular (nueva ventana)
start "YCT Frontend (Angular)" cmd /k "cd /d %~dp0Front\yct-frontend && echo === FRONTEND ANGULAR === && npm start"

echo.
echo  Listo. Servidores iniciandose en ventanas separadas.
echo  Esta ventana se puede cerrar.
echo.
pause
