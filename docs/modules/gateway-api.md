# Gateway.Api – Minimal APIs

HTTP-Einstiegsschicht. Stellt Endpunkte bereit, verdrahtet DI, Auth, Health, Metrics.

## Aufgaben
- Routing/Endpoints (Auth, Conversations, Chat, Health)
- DI-Konfiguration (Services, Repositories, DbContext)
- Middleware (AuthN/AuthZ, Logging, CORS)

## Wichtige Konfiguration
- `appsettings*.json`: Ports, Verbindungen, LM Studio BaseUrl, JWT, CORS
- Health: `/health/live`, `/health/ready`
- Metrics: `/metrics`

## Datenfluss
- HTTP Request → Validatoren (Application) → Services (Application) → Repositories (Infrastructure) → DB

## Erweiterung
- Neue Endpunkte: Minimal API Handler hinzufügen und Services/DTOs im `Gateway.Application` definieren
- Neue Policies/Rollen: AuthZ-Setup erweitern

