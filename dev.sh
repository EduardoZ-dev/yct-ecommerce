#!/usr/bin/env bash
# ============================================
#  Distribuciones YCT - Dev Server Launcher
#  Ejecuta backend (.NET) y frontend (Angular)
#  para Git Bash / WSL / Linux / macOS.
# ============================================

set -e
ROOT="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "========================================"
echo "  YCT - Iniciando entorno de desarrollo"
echo "========================================"
echo ""
echo "  Backend  -> http://localhost:5088"
echo "  Frontend -> http://localhost:4200"
echo ""

# Cleanup on exit (Ctrl+C)
cleanup() {
  echo ""
  echo "Deteniendo servidores..."
  kill "$BACKEND_PID" "$FRONTEND_PID" 2>/dev/null || true
  exit 0
}
trap cleanup INT TERM

# Backend
echo "[BACKEND] Iniciando .NET API..."
(cd "$ROOT/Back/YCT.API" && dotnet run 2>&1 | sed 's/^/[BE] /') &
BACKEND_PID=$!

sleep 2

# Frontend
echo "[FRONTEND] Iniciando Angular..."
(cd "$ROOT/Front/yct-frontend" && npm start 2>&1 | sed 's/^/[FE] /') &
FRONTEND_PID=$!

echo ""
echo "Servidores corriendo. Ctrl+C para detener ambos."
echo ""

wait
