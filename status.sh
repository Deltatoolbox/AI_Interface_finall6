#!/bin/bash

# LM Gateway Status Script
# Zeigt den Status aller Services

echo "📊 LM Gateway Services Status"
echo "=============================="
echo ""

# Backend Status prüfen
if pgrep -f "dotnet.*SimpleGateway" > /dev/null; then
    echo "📡 Backend (API):  ✅ RUNNING on http://localhost:5058"
else
    echo "📡 Backend (API):  ❌ STOPPED"
fi

# Frontend Status prüfen
if pgrep -f "npm.*dev" > /dev/null || pgrep -f "vite" > /dev/null; then
    echo "🌐 Frontend (Web): ✅ RUNNING on http://localhost:5173"
else
    echo "🌐 Frontend (Web): ❌ STOPPED"
fi

# LM Studio Status prüfen
if curl -s http://localhost:1234/v1/models > /dev/null 2>&1; then
    echo "🤖 LM Studio:      ✅ RUNNING on http://localhost:1234"
else
    echo "🤖 LM Studio:      ❌ STOPPED"
fi

echo ""
echo "🔑 Login: admin / admin"
echo ""
echo "Commands:"
echo "  ./start.sh  - Start all services"
echo "  ./stop.sh   - Stop all services"
echo "  ./status.sh - Show this status"
