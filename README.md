# AIGS – AI Gateway Suite

Ein vollständiges, lokal lauffähiges AI-Gateway mit React-Frontend, ASP.NET Core Backend und Caddy/Nginx als Reverse Proxy. Unterstützt LM Studio (OpenAI-kompatibel), Konversationen, Auth, Metriken, Backups und mehr.

## Inhalt
- Überblick und Architektur
- Voraussetzungen
- Schnellstart (2 Wege)
- Konfiguration und Umgebungsvariablen
- Entwicklung (Frontend/Backend/Migrationen/Tests)
- Datenbank & Backups
- Deployment (Caddy, Nginx, systemd)
- API-Dokumentation
- Monitoring & Security
- Fehlerbehebung (Troubleshooting/FAQ)

## Überblick
- Frontend: React + TypeScript (Vite, Tailwind) unter `src/WebApp`
- Backend: ASP.NET Core (.NET 8)
  - Voller Schichtenaufbau: `Gateway.Api`, `Gateway.Application`, `Gateway.Domain`, `Gateway.Infrastructure`
  - Alternative „All-in-One“: `SimpleGateway` (SQLite, integrierte Migrations und Admin-Flows)
- Reverse Proxy: Caddy oder Nginx
- Datenbank: SQLite (lokal) mit Migrations (EF Core)

### Projektstruktur
```text
src/
├─ Gateway.Api/             # Minimal APIs, Composition Root
├─ Gateway.Application/     # Use-Cases, DTOs, Services, Validatoren
├─ Gateway.Domain/          # Entities, Interfaces
├─ Gateway.Infrastructure/  # EF Core, Repositories, externe Services
├─ SimpleGateway/           # Alternative monolithische Variante
└─ WebApp/                  # React-Frontend

tests/
├─ Gateway.UnitTests/
└─ Gateway.IntegrationTests/

deploy/
├─ caddy/
├─ nginx/
└─ systemd/
```

## Voraussetzungen
- .NET 8 SDK: `https://dotnet.microsoft.com/download`
- Node.js 18+: `https://nodejs.org/`
- LM Studio lokal: Standard `http://127.0.0.1:1234`
- Caddy (optional; wird per Skript installiert) oder Nginx

## Schnellstart

### Weg A: Einfache Variante (Caddy + SimpleGateway)
1) Repository klonen
```bash
git clone <your-repo-url>
cd AI_Interface
```
2) Setup-Skript ausführen (installiert/konfiguriert Caddy, erstellt Ordner etc.)
```bash
./setup-local.sh
```
3) Starten
```bash
./start-simple.sh
```
4) Aufrufen
- Frontend: `http://localhost`
- API via Proxy: `http://localhost/api`

Stoppen:
```bash
./stop-simple.sh
```

### Weg B: Voller Schichtenaufbau (Gateway.Api + WebApp)
Backend starten:
```bash
dotnet run --project src/Gateway.Api
```
Frontend im Dev-Modus:
```bash
cd src/WebApp
npm install
npm run dev
```
Standard-URLs:
- Frontend Dev: `http://localhost:5173`
- API: `http://localhost:5000` (laut DEVELOPMENT.md)

Hinweis: Für einen gebündelten Betrieb mit Reverse Proxy siehe Abschnitt „Deployment“.

## Konfiguration und Umgebungsvariablen
- AppSettings: `src/Gateway.Api/appsettings*.json`, `src/SimpleGateway/appsettings*.json`
- Deployment-Variablen: `deploy/aigs.env`
- Caddy-Konfiguration: `Caddyfile`, `deploy/caddy/Caddyfile`
- Nginx (Alternative): `deploy/nginx/nginx.conf`

Wichtige Parameter (Beispiele – passen Sie diese an):
- LM Studio Base URL: `http://127.0.0.1:1234`
- JWT Key/Secrets: in AppSettings oder als Umgebungsvariable
- Datenbankpfad (SQLite): in der jeweiligen `appsettings.json`

## Entwicklung

### Backend (Gateway.Api)
```bash
dotnet restore
dotnet run --project src/Gateway.Api
```
Health/Metrics (laut Docs):
- Health: `http://localhost:5000/health/live` und `/health/ready`
- Metrics: `http://localhost:5000/metrics`

### Frontend (WebApp)
```bash
cd src/WebApp
npm install
npm run dev
```

### Datenbank-Migrationen (Voll-Stack Variante)
```bash
# Migration erstellen
dotnet ef migrations add <Name> \
  --project src/Gateway.Infrastructure \
  --startup-project src/Gateway.Api

# Migration anwenden
dotnet ef database update \
  --project src/Gateway.Infrastructure \
  --startup-project src/Gateway.Api
```

### Datenbank-Migrationen (SimpleGateway)
```bash
cd src/SimpleGateway
dotnet ef migrations add <Name>
dotnet ef database update
```

### Tests
```bash
dotnet test

# nur Unit Tests
dotnet test tests/Gateway.UnitTests

# nur Integrationstests
dotnet test tests/Gateway.IntegrationTests
```

### Standard-Login
- Benutzer: `admin`
- Passwort: `admin`

## Datenbank & Backups
- SQLite-Dateien befinden sich in `src/SimpleGateway` bzw. im Ausgabeordner des Backends
- SimpleGateway enthält Backup/Restore-Logik und legt Snapshots in `src/SimpleGateway/backups/` ab
- Für manuelle Backups: Kopie der `.db`-Datei im ausgeschalteten Zustand erstellen

## Deployment

### Caddy (empfohlen für lokal)
Start (lokal):
```bash
caddy run --config Caddyfile
```
Eigenschaften:
- Statisches Frontend-Hosting
- Reverse Proxy auf Backend (`/api` → Backend-Port)
- Sicherheits-Header/CORS konfigurierbar

### Nginx (Alternative)
Beispiel-Konfiguration unter `deploy/nginx/nginx.conf`. Passen Sie Servername/Upstreams an.

### Systemd (Linux)
Eine Service-Definition ist unter `deploy/systemd/aigs.service` enthalten. Installation siehe `deploy/install.sh`.

## API-Dokumentation
Ausführliche API-Referenz in `docs/API.md` (Auth, Modelle, Chat, Konversationen, Health, Metrics, Rate Limits, WebSocket, CORS). Nutzen Sie die Proxy-Route `/api` (z. B. `POST /api/chat`).

## Monitoring & Security
- Monitoring/Observability: siehe `docs/MONITORING.md` (Prometheus, Grafana, OpenTelemetry, Logs, Tracing, Alerting)
- Security-Guidelines: siehe `docs/SECURITY.md` (JWT, Passwörter, CORS, Reverse Proxy, Hardening, Compliance)

## Fehlerbehebung (Troubleshooting)
Port-Konflikte:
- Passen Sie Ports in `Caddyfile` bzw. `launchSettings.json`/`appsettings.json` an
- Stoppen Sie kollidierende Dienste

Caddy-Probleme:
```bash
caddy validate --config Caddyfile
caddy run --config Caddyfile --watch
caddy reload --config Caddyfile
```

Backend:
- `.NET 8` installiert? `dotnet --version`
- Pakete erneuern: `dotnet restore`
- Datenbank vorhanden/Berechtigungen prüfen (SQLite-Datei)

Frontend:
```bash
node --version
npm cache clean --force
rm -rf node_modules && npm install
```

LM Studio:
- Läuft auf `http://127.0.0.1:1234`?
- Modelle geladen?
- BaseUrl in `appsettings.json` korrekt?

## FAQ
- Kann ich ohne Caddy entwickeln? Ja. Starten Sie `Gateway.Api` und `WebApp` im Dev-Modus (Ports 5000/5173). Passen Sie CORS an.
- Production-DB? Nutzen Sie Postgres/SQL Server. SQLite ist für lokal/Tests.
- SSL lokal? Caddy kann selbstsignierte Zertifikate ausliefern. Für Prod echte Zertifikate (Let’s Encrypt).

## Lizenz
[Ihre Lizenz hier]