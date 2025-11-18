# SimpleGateway – Monolithisches Backend

Einfacher Monolith für lokalen Betrieb mit integrierter DB/Migrations und Services.

## Aufgaben
- API, DB, Services in einem Projekt
- SQLite-Datenbank und Migrations unter `Migrations/`
- Backups unter `backups/`

## Start
```bash
cd src/SimpleGateway
dotnet run --urls="http://localhost:5058"
```

## Erweiterung
- Neue Endpunkte in `Program.cs`/Services anlegen
- EF Core Migrationen ergänzen

