#!/bin/bash
# Stop Script für AIGS Simple Deployment
# Stoppt alle Services und gibt Ports frei

echo "🛑 Stopping AIGS Services..."

# Funktion zum Stoppen von Prozessen auf einem Port
kill_port() {
    local port=$1
    local pids=$(lsof -ti:$port 2>/dev/null)
    
    if [ -n "$pids" ]; then
        echo "   Stopping processes on port $port (PIDs: $pids)..."
        kill -9 $pids 2>/dev/null
        sleep 1
        return 0
    else
        return 1
    fi
}

# 1. SimpleGateway Backend stoppen (Port 5058)
echo "📡 Stopping Backend (SimpleGateway)..."
if pkill -9 -f "SimpleGateway.*5058" 2>/dev/null; then
    echo "   ✅ SimpleGateway process stopped"
else
    echo "   ℹ️  No SimpleGateway process found"
fi

# Alle dotnet-Prozesse für SimpleGateway stoppen
if pkill -9 -f "dotnet.*SimpleGateway" 2>/dev/null; then
    echo "   ✅ Dotnet SimpleGateway processes stopped"
fi

# Port 5058 freigeben (falls noch belegt)
if kill_port 5058; then
    echo "   ✅ Port 5058 freed"
fi

# 2. Caddy stoppen (Port 8080)
echo "🔄 Stopping Caddy..."
if pkill -9 -f "caddy.*Caddyfile.simple" 2>/dev/null; then
    echo "   ✅ Caddy (Caddyfile.simple) stopped"
else
    echo "   ℹ️  No Caddy process with Caddyfile.simple found"
fi

# Alle Caddy-Prozesse stoppen (falls mehrere laufen)
if pkill -9 -f "caddy run" 2>/dev/null; then
    echo "   ✅ All Caddy processes stopped"
fi

# Port 8080 freigeben (falls noch belegt)
if kill_port 8080; then
    echo "   ✅ Port 8080 freed"
fi

# 3. Warten bis Ports wirklich frei sind
sleep 2

# 4. Ports verifizieren
echo ""
echo "🔍 Verifying ports..."

port_5058_status=$(ss -tuln | grep ':5058' 2>/dev/null)
port_8080_status=$(ss -tuln | grep ':8080' 2>/dev/null)

if [ -z "$port_5058_status" ]; then
    echo "   ✅ Port 5058: FREE"
else
    echo "   ⚠️  Port 5058: Still in use!"
    echo "      $port_5058_status"
fi

if [ -z "$port_8080_status" ]; then
    echo "   ✅ Port 8080: FREE"
else
    echo "   ⚠️  Port 8080: Still in use!"
    echo "      $port_8080_status"
fi

# 5. Temporäre Dateien aufräumen (optional)
if [ -f "startup.log" ]; then
    rm -f startup.log
    echo "   ✅ Cleanup: startup.log removed"
fi

echo ""
echo "✅ All AIGS services stopped!"
echo ""
echo "📊 Summary:"
echo "   🔴 Backend (Port 5058): Stopped"
echo "   🔴 Caddy (Port 8080): Stopped"
echo ""
echo "🚀 To start again: ./start-simple.sh"

