#!/bin/bash

# AIGS Local Stop Script
# This script stops all local services

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_info "Stopping AIGS services..."

# Stop backend if running
if [ -f .backend.pid ]; then
    BACKEND_PID=$(cat .backend.pid)
    if kill -0 $BACKEND_PID 2>/dev/null; then
        log_info "Stopping backend (PID: $BACKEND_PID)..."
        kill $BACKEND_PID
        rm .backend.pid
        log_success "Backend stopped"
    else
        log_warning "Backend process not running"
        rm .backend.pid
    fi
else
    log_warning "No backend PID file found"
fi

# Stop Caddy if running
if [ -f .caddy.pid ]; then
    CADDY_PID=$(cat .caddy.pid)
    if kill -0 $CADDY_PID 2>/dev/null; then
        log_info "Stopping Caddy (PID: $CADDY_PID)..."
        kill $CADDY_PID
        rm .caddy.pid
        log_success "Caddy stopped"
    else
        log_warning "Caddy process not running"
        rm .caddy.pid
    fi
else
    log_warning "No Caddy PID file found"
fi

# Kill any remaining dotnet processes (backend)
if pgrep -f "dotnet.*SimpleGateway" > /dev/null; then
    log_info "Killing remaining dotnet processes..."
    pkill -f "dotnet.*SimpleGateway" || true
fi

# Kill any remaining caddy processes
if pgrep -f "caddy" > /dev/null; then
    log_info "Killing remaining Caddy processes..."
    pkill -f "caddy" || true
fi

log_success "All services stopped!"
echo ""
echo "🛑 Services stopped:"
echo "   Backend: Stopped"
echo "   Caddy: Stopped"
echo ""
echo "🚀 To start again: ./setup-local.sh"
