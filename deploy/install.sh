#!/bin/bash

# LM Gateway Deployment Script
# This script sets up the LM Gateway service on Ubuntu

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SERVICE_NAME="lm-gateway"
SERVICE_USER="lm-gateway"
SERVICE_GROUP="lm-gateway"
INSTALL_DIR="/opt/lm-gateway"
DATA_DIR="/opt/lm-gateway/data"
LOGS_DIR="/opt/lm-gateway/logs"
CONFIG_DIR="/etc"
SERVICE_FILE="/etc/systemd/system/lm-gateway.service"
ENV_FILE="/etc/lm-gateway.env"

echo -e "${GREEN}LM Gateway Deployment Script${NC}"
echo "=================================="

# Check if running as root
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}This script must be run as root${NC}"
   exit 1
fi

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}.NET 8 is not installed. Installing...${NC}"
    
    # Install .NET 8
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    
    apt-get update
    apt-get install -y dotnet-sdk-8.0
    
    echo -e "${GREEN}.NET 8 installed successfully${NC}"
fi

# Create service user
if ! id "$SERVICE_USER" &>/dev/null; then
    echo -e "${YELLOW}Creating service user: $SERVICE_USER${NC}"
    useradd --system --no-create-home --shell /bin/false $SERVICE_USER
    echo -e "${GREEN}Service user created${NC}"
fi

# Create directories
echo -e "${YELLOW}Creating directories...${NC}"
mkdir -p $INSTALL_DIR
mkdir -p $DATA_DIR
mkdir -p $LOGS_DIR
mkdir -p $CONFIG_DIR

# Set permissions
chown -R $SERVICE_USER:$SERVICE_GROUP $INSTALL_DIR
chmod 755 $INSTALL_DIR
chmod 755 $DATA_DIR
chmod 755 $LOGS_DIR

echo -e "${GREEN}Directories created and permissions set${NC}"

# Copy service file
echo -e "${YELLOW}Installing systemd service...${NC}"
cp deploy/systemd/lm-gateway.service $SERVICE_FILE
chmod 644 $SERVICE_FILE

# Copy environment file
echo -e "${YELLOW}Installing environment configuration...${NC}"
cp deploy/lm-gateway.env $ENV_FILE
chmod 600 $ENV_FILE
chown root:root $ENV_FILE

# Generate secure JWT key if not set
if grep -q "CHANGE-THIS-TO-A-SECURE-RANDOM-KEY-IN-PRODUCTION" $ENV_FILE; then
    echo -e "${YELLOW}Generating secure JWT key...${NC}"
    JWT_KEY=$(openssl rand -base64 32)
    sed -i "s/CHANGE-THIS-TO-A-SECURE-RANDOM-KEY-IN-PRODUCTION/$JWT_KEY/" $ENV_FILE
    echo -e "${GREEN}JWT key generated${NC}"
fi

# Reload systemd
systemctl daemon-reload

echo -e "${GREEN}Service installed successfully${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Build and publish the application:"
echo "   dotnet publish -c Release -r linux-x64 --self-contained true -o $INSTALL_DIR"
echo ""
echo "2. Run database migrations:"
echo "   dotnet ef database update --project src/Gateway.Infrastructure --startup-project src/Gateway.Api"
echo ""
echo "3. Start the service:"
echo "   systemctl start $SERVICE_NAME"
echo "   systemctl enable $SERVICE_NAME"
echo ""
echo "4. Check service status:"
echo "   systemctl status $SERVICE_NAME"
echo ""
echo "5. View logs:"
echo "   journalctl -u $SERVICE_NAME -f"
echo ""
echo -e "${GREEN}Deployment completed!${NC}"
