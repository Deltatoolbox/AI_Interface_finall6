#!/bin/bash

# LM Gateway Local Setup Script (without Docker)
# This script sets up Caddy for local development

set -e

echo "🚀 Setting up LM Gateway locally with Caddy..."

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

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   log_error "This script should not be run as root for security reasons"
   exit 1
fi

# Check dependencies
check_dependencies() {
    log_info "Checking dependencies..."
    
    if ! command -v caddy &> /dev/null; then
        log_warning "Caddy is not installed. Installing Caddy..."
        install_caddy
    else
        log_success "Caddy is already installed"
    fi
    
    if ! command -v dotnet &> /dev/null; then
        log_error "Dotnet is not installed. Please install .NET 8 SDK first."
        log_info "Visit: https://dotnet.microsoft.com/download"
        exit 1
    else
        log_success "Dotnet is installed"
    fi
    
    if ! command -v node &> /dev/null; then
        log_error "Node.js is not installed. Please install Node.js first."
        log_info "Visit: https://nodejs.org/"
        exit 1
    else
        log_success "Node.js is installed"
    fi
}

# Install Caddy
install_caddy() {
    log_info "Installing Caddy..."
    
    # Detect OS
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        if command -v apt-get &> /dev/null; then
            # Ubuntu/Debian
            sudo apt-get update
            sudo apt-get install -y debian-keyring debian-archive-keyring apt-transport-https
            curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
            curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
            sudo apt-get update
            sudo apt-get install -y caddy
        elif command -v yum &> /dev/null; then
            # CentOS/RHEL
            sudo yum install -y 'dnf-command(copr)'
            sudo dnf copr enable -y @caddy/caddy
            sudo dnf install -y caddy
        else
            log_error "Unsupported Linux distribution. Please install Caddy manually."
            log_info "Visit: https://caddyserver.com/docs/install"
            exit 1
        fi
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        if command -v brew &> /dev/null; then
            brew install caddy
        else
            log_error "Homebrew not found. Please install Caddy manually."
            log_info "Visit: https://caddyserver.com/docs/install"
            exit 1
        fi
    else
        log_error "Unsupported operating system. Please install Caddy manually."
        log_info "Visit: https://caddyserver.com/docs/install"
        exit 1
    fi
    
    log_success "Caddy installed successfully"
}

# Build frontend
build_frontend() {
    log_info "Building frontend..."
    
    cd src/WebApp
    
    # Install dependencies
    npm ci
    
    # Build application
    npm run build
    
    # Copy build to dist directory
    mkdir -p ../../dist
    cp -r dist/* ../../dist/
    
    cd ../..
    
    log_success "Frontend built successfully"
}

# Setup Caddy
setup_caddy() {
    log_info "Setting up Caddy..."
    
    # Create log directory
    mkdir -p logs/caddy
    
    # Test configuration
    caddy validate --config Caddyfile
    
    log_success "Caddy configuration is valid"
}

# Start services
start_services() {
    log_info "Starting services..."
    
    # Start backend in background
    log_info "Starting backend on port 5058..."
    cd src/SimpleGateway
    dotnet run --urls="http://localhost:5058" &
    BACKEND_PID=$!
    cd ../..
    
    # Wait for backend to start
    log_info "Waiting for backend to start..."
    sleep 5
    
    # Check if backend is running
    if curl -f http://localhost:5058/api/health > /dev/null 2>&1; then
        log_success "Backend is running on http://localhost:5058"
    else
        log_error "Backend failed to start"
        kill $BACKEND_PID 2>/dev/null || true
        exit 1
    fi
    
    # Start Caddy
    log_info "Starting Caddy..."
    caddy run --config Caddyfile &
    CADDY_PID=$!
    
    # Wait for Caddy to start
    sleep 2
    
    # Check if Caddy is running
    if curl -f http://localhost > /dev/null 2>&1; then
        log_success "Caddy is running on http://localhost"
    else
        log_error "Caddy failed to start"
        kill $BACKEND_PID $CADDY_PID 2>/dev/null || true
        exit 1
    fi
    
    # Save PIDs for cleanup
    echo $BACKEND_PID > .backend.pid
    echo $CADDY_PID > .caddy.pid
}

# Show setup info
show_info() {
    log_success "LM Gateway is now running locally!"
    echo ""
    echo "🌐 Access your application:"
    echo "   Frontend: http://localhost"
    echo "   Backend API: http://localhost:5058"
    echo "   API via Caddy: http://localhost/api"
    echo "   Health Check: http://localhost/api/health"
    echo ""
    echo "📊 Service Status:"
    echo "   Backend PID: $(cat .backend.pid 2>/dev/null || echo 'Not running')"
    echo "   Caddy PID: $(cat .caddy.pid 2>/dev/null || echo 'Not running')"
    echo ""
    echo "🛑 To stop services:"
    echo "   ./stop-local.sh"
    echo ""
    echo "📝 Logs:"
    echo "   Backend: Check terminal output"
    echo "   Caddy: logs/caddy/access.log"
    echo ""
    echo "🔧 Management:"
    echo "   Restart Caddy: caddy reload --config Caddyfile"
    echo "   Test config: caddy validate --config Caddyfile"
}

# Cleanup function
cleanup() {
    log_info "Stopping services..."
    
    if [ -f .backend.pid ]; then
        BACKEND_PID=$(cat .backend.pid)
        kill $BACKEND_PID 2>/dev/null || true
        rm .backend.pid
    fi
    
    if [ -f .caddy.pid ]; then
        CADDY_PID=$(cat .caddy.pid)
        kill $CADDY_PID 2>/dev/null || true
        rm .caddy.pid
    fi
    
    log_success "Services stopped"
}

# Handle script arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --help)
            echo "LM Gateway Local Setup Script"
            echo ""
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --help               Show this help message"
            echo ""
            echo "This script will:"
            echo "  1. Check and install dependencies (Caddy, .NET, Node.js)"
            echo "  2. Build the frontend"
            echo "  3. Start the backend on port 5058"
            echo "  4. Start Caddy on port 80"
            echo ""
            echo "Access:"
            echo "  Frontend: http://localhost"
            echo "  Backend: http://localhost:5058"
            echo "  API: http://localhost/api"
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Trap cleanup on exit
trap cleanup EXIT

# Run setup
main() {
    log_info "Starting LM Gateway local setup..."
    
    check_dependencies
    build_frontend
    setup_caddy
    start_services
    show_info
    
    # Keep script running
    log_info "Press Ctrl+C to stop all services..."
    wait
}

# Run main function
main
