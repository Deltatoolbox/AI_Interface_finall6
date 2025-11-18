# Gateway.Domain – Kernmodelle und Ports

Domain-Modelle und Interfaces (Ports), ohne technische Details.

## Aufgaben
- Entities: `Conversation`, `Message`, `UsageLog`, `User`
- Interfaces: `IConversationRepository`, `IMessageRepository`, `IUserRepository`, `IUsageLogRepository`, `ITokenService`, `IConcurrencyManager`, `ILmStudioClient`

## Prinzipien
- Keine Abhängigkeit zu EF Core/HTTP/Serilog
- Reine Geschäftsobjekte und Verträge

## Erweiterung
- Neue Entity: in Domain definieren, Repositories in Infrastructure implementieren
- Neue Schnittstelle (Port): hier definieren, Adapter in Infrastructure

