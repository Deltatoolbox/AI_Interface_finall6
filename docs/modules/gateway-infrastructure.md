# Gateway.Infrastructure – Adapter/Implementierungen

Technische Implementierungen der Domain-Ports.

## Aufgaben
- EF Core: `Data/GatewayDbContext.cs`
- Repositories: `ConversationRepository`, `MessageRepository`, `UserRepository`, `UsageLogRepository`
- Services: `ConcurrencyManager`, `LmStudioClient`, `TokenService`

## Datenfluss
- Services/Repos werden per DI in `Gateway.Api` injiziert
- DB-Zugriff ausschließlich über Repositories

## Erweiterung
- Neue Datenquelle: neues Repository + DbContext-Anpassung
- Externe APIs: neuer Service (HTTP-Client, Resilienz, Logging)

