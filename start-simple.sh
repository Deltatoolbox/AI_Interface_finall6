#!/bin/bash
# Einfacher Nginx-freier Deployment für 0.0.0.0
# Nutzt eine einfache Caddy-Konfiguration ohne SSL

echo "🚀 Starting AIGS on 0.0.0.0:8080 (HTTP)"

# Logs-Verzeichnis erstellen
mkdir -p logs

# Backend starten
echo "📡 Starting Backend (API) on http://0.0.0.0:5058..."
cd src/SimpleGateway
dotnet run --urls="http://0.0.0.0:5058" &
BACKEND_PID=$!
cd ../..

# Warten bis Backend bereit ist
echo "⏳ Waiting for backend to start..."
sleep 5

# Frontend builden
echo "🏗️ Building Frontend..."
cd src/WebApp
npm run build
cd ../..

# Einfache Caddy-Konfiguration für HTTP
cat > Caddyfile.simple << 'EOF'
{
    auto_https off
    log {
        output file ./logs/access.log
        format json
        level INFO
    }
}

:8080 {
    header {
        Access-Control-Allow-Origin "*"
        Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS"
        Access-Control-Allow-Headers "Content-Type, Authorization, X-Requested-With"
        Access-Control-Allow-Credentials "true"
    }
    
    handle /api/* {
        reverse_proxy 127.0.0.1:5058
        
        @options {
            method OPTIONS
        }
        respond @options 200
    }
    
    handle /assets/* {
        root * ./src/WebApp/dist
        file_server
        header Cache-Control "public, max-age=31536000, immutable"
    }
    
    handle {
        root * ./src/WebApp/dist
        try_files {path} /index.html
        file_server
    }
}
EOF

# Caddy starten
echo "🔄 Starting Caddy reverse proxy..."
caddy run --config Caddyfile.simple &
CADDY_PID=$!

echo ""
echo "✅ AIGS is running!"
echo "📱 Frontend + API: http://0.0.0.0:8080"
echo "🔧 Direct API: http://0.0.0.0:5058"
echo ""
echo "Press Ctrl+C to stop all services"

# Cleanup function
cleanup() {
    echo ""
    echo "🛑 Stopping services..."
    kill $BACKEND_PID 2>/dev/null
    kill $CADDY_PID 2>/dev/null
    echo "✅ Stopped!"
    exit 0
}

# Trap cleanup on script exit
trap cleanup SIGINT SIGTERM

# Wait for processes
wait
