# LM Gateway System - Local Development

This repository contains a complete LM Gateway System with React frontend, ASP.NET Core backend, and Caddy reverse proxy for local development.

## Features

- **Frontend**: React with TypeScript, modern UI components
- **Backend**: ASP.NET Core Minimal APIs with SQLite database
- **Reverse Proxy**: Caddy for HTTPS termination and routing
- **Authentication**: JWT-based authentication with role-based access control
- **End-to-End Encryption**: AES-256-GCM encryption for chat messages
- **Webhooks**: Event-driven notifications to external systems
- **Integrations**: Slack and Discord integration
- **Admin Panel**: Complete administration interface
- **Backup System**: Automated backup and restore functionality

## Prerequisites

- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download)
- **Node.js 18+**: [Download here](https://nodejs.org/)
- **Caddy**: Will be installed automatically by the setup script

## Quick Start

1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd AI_Interface
   ```

2. **Run the setup script:**
   ```bash
   ./setup-local.sh
   ```

3. **Access your application:**
   - Frontend: http://localhost
   - Backend API: http://localhost:5058
   - API via Caddy: http://localhost/api

4. **Stop services:**
   ```bash
   ./stop-local.sh
   ```

## Manual Setup (Alternative)

If you prefer to set up manually:

1. **Install Caddy:**
   ```bash
   # Ubuntu/Debian
   sudo apt-get install -y debian-keyring debian-archive-keyring apt-transport-https
   curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
   curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
   sudo apt-get update
   sudo apt-get install -y caddy
   ```

2. **Build and start backend:**
   ```bash
   cd src/SimpleGateway
   dotnet run --urls="http://localhost:5058"
   ```

3. **Build frontend:**
   ```bash
   cd src/WebApp
   npm install
   npm run build
   cp -r dist/* ../../dist/
   ```

4. **Start Caddy:**
   ```bash
   caddy run --config Caddyfile
   ```

## Configuration

### Caddy Configuration

The `Caddyfile` is configured for localhost development with:
- HTTP on port 80
- HTTPS on port 443 (optional, with self-signed certificates)
- Reverse proxy to backend on port 5058
- Static file serving for frontend
- Security headers and CORS configuration

### Backend Configuration

The backend runs on port 5058 and includes:
- SQLite database (automatically created)
- JWT authentication
- Role-based access control
- Webhook system
- Encryption services

### Frontend Configuration

The React frontend is built and served as static files through Caddy.

## Default Login

- **Username**: admin
- **Password**: admin

## API Endpoints

- **Authentication**: `/api/auth/*`
- **Users**: `/api/users/*`
- **Conversations**: `/api/conversations/*`
- **Chat**: `/api/chat`
- **Admin**: `/api/admin/*`
- **Webhooks**: `/api/webhooks/*`
- **Integrations**: `/api/integrations/*`
- **Health**: `/api/health`

## Development

### Backend Development

```bash
cd src/SimpleGateway
dotnet watch run --urls="http://localhost:5058"
```

### Frontend Development

```bash
cd src/WebApp
npm run dev
```

### Database Migrations

```bash
cd src/SimpleGateway
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Troubleshooting

### Port Conflicts

If ports 80 or 5058 are in use:
- Change the backend port in `src/SimpleGateway/Properties/launchSettings.json`
- Update the Caddyfile to proxy to the new port
- Or stop conflicting services

### Caddy Issues

- Test configuration: `caddy validate --config Caddyfile`
- Check logs: `caddy run --config Caddyfile --watch`
- Reload configuration: `caddy reload --config Caddyfile`

### Backend Issues

- Check if .NET 8 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
- Check database: The SQLite file is created automatically

### Frontend Issues

- Check Node.js version: `node --version`
- Clear cache: `npm cache clean --force`
- Reinstall dependencies: `rm -rf node_modules && npm install`

## Security Notes

- This setup is for **local development only**
- Default admin credentials should be changed in production
- HTTPS is optional for localhost development
- Database is SQLite (file-based, not suitable for production)

## Production Deployment

For production deployment, consider:
- Using a real domain with Let's Encrypt certificates
- PostgreSQL or SQL Server instead of SQLite
- Proper secrets management
- Environment-specific configurations
- Load balancing and scaling

## License

[Your License Here]