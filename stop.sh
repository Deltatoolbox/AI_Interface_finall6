#!/bin/bash

# LM Gateway Stop Script
# Stoppt alle laufenden Services

echo "🛑 Stopping LM Gateway Services..."

# Alle dotnet Prozesse stoppen
pkill -f "dotnet.*SimpleGateway" 2>/dev/null
echo "✅ Backend stopped"

# Alle npm/vite Prozesse stoppen
pkill -f "npm.*dev" 2>/dev/null
pkill -f "vite" 2>/dev/null
echo "✅ Frontend stopped"

# Alle Python HTTP Server stoppen (falls vorhanden)
pkill -f "python.*http.server" 2>/dev/null
echo "✅ HTTP Server stopped"

echo ""
echo "🎯 All services stopped successfully!"
