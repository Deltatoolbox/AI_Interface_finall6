# AIGS - Development Quick Start

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and pnpm
- LM Studio running on `http://127.0.0.1:1234`

## Quick Setup

### 1. Clone and Restore

```bash
git clone <repository-url>
cd AI_Interface
dotnet restore
```

### 2. Database Setup

```bash
# Create initial migration
dotnet ef migrations add Init --project src/Gateway.Infrastructure --startup-project src/Gateway.Api

# Apply migration
dotnet ef database update --project src/Gateway.Infrastructure --startup-project src/Gateway.Api
```

### 3. Start Backend

```bash
dotnet run --project src/Gateway.Api
```

The API will be available at `http://localhost:5000`

### 4. Start Frontend

```bash
cd src/WebApp
pnpm install
pnpm dev
```

The frontend will be available at `http://localhost:5173`

## Default Login

- **Username**: `admin`
- **Password**: `admin`

## Development URLs

- **Frontend**: http://localhost:5173
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health/live
- **Metrics**: http://localhost:5000/metrics

## Testing

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/Gateway.UnitTests

# Run integration tests only
dotnet test tests/Gateway.IntegrationTests
```

## Common Development Tasks

### Add New Migration

```bash
dotnet ef migrations add MigrationName --project src/Gateway.Infrastructure --startup-project src/Gateway.Api
```

### Update Database

```bash
dotnet ef database update --project src/Gateway.Infrastructure --startup-project src/Gateway.Api
```

### Reset Database

```bash
dotnet ef database drop --project src/Gateway.Infrastructure --startup-project src/Gateway.Api
dotnet ef database update --project src/Gateway.Infrastructure --startup-project src/Gateway.Api
```

### Build for Production

```bash
# Backend
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish

# Frontend
cd src/WebApp
pnpm build
```

## Troubleshooting

### LM Studio Connection Issues

1. Ensure LM Studio is running on port 1234
2. Check `LmStudio:BaseUrl` in `appsettings.json`
3. Verify LM Studio has models loaded

### Database Issues

1. Check file permissions for SQLite database
2. Ensure migration was applied successfully
3. Check connection string format

### Frontend Issues

1. Clear browser cache and cookies
2. Check console for JavaScript errors
3. Verify API endpoints are accessible

### Authentication Issues

1. Check JWT key configuration
2. Verify cookie settings
3. Clear browser cookies and try again

## Project Structure

```
src/
├── Gateway.Api/           # Minimal API endpoints
├── Gateway.Application/   # Use cases, DTOs, validators
├── Gateway.Domain/        # Entities, interfaces
├── Gateway.Infrastructure/ # EF Core, repositories, external services
└── WebApp/               # React frontend

tests/
├── Gateway.UnitTests/     # Unit tests
└── Gateway.IntegrationTests/ # Integration tests

deploy/
├── caddy/                # Caddy configuration
├── nginx/                # Nginx configuration
├── systemd/              # Systemd service
└── install.sh            # Deployment script
```

## Next Steps

1. **Customize Configuration**: Update `appsettings.json` for your environment
2. **Add Users**: Create additional users via database or admin interface
3. **Configure Models**: Ensure LM Studio has the models you want to use
4. **Set Up Monitoring**: Configure Prometheus and Grafana for metrics
5. **Deploy to Production**: Use the deployment scripts for production setup
