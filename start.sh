#!/bin/bash

# LM Gateway Start Script
# Startet Backend und Frontend gleichzeitig

echo "🚀 Starting LM Gateway Services..."
echo ""

# Funktion zum Beenden aller Prozesse
cleanup() {
    echo ""
    echo "🛑 Stopping services..."
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit 0
}

# Signal Handler für Ctrl+C
trap cleanup SIGINT SIGTERM

# Backend starten
echo "📡 Starting Backend (API) on http://localhost:5058..."
cd src/SimpleGateway
dotnet run &
BACKEND_PID=$!

# Kurz warten bis Backend gestartet ist
sleep 3

# Frontend starten
echo "🌐 Starting Frontend (Web) on http://localhost:5173..."
cd ../WebApp
npm run dev &
FRONTEND_PID=$!

echo ""
echo "✅ Services started successfully!"
echo ""
echo "📡 Backend API:  http://localhost:5058"
echo "🌐 Frontend Web: http://localhost:5173"
echo ""
echo "🔑 Login: admin / admin"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Warten bis einer der Prozesse beendet wird
wait
