# Tests – Unit & Integration

Testlandschaft und Ausführung.

## Pfade
- `tests/Gateway.UnitTests`: Unittests
- `tests/Gateway.IntegrationTests`: Integrationstests

## Ausführung
```bash
dotnet test
```

## Tipps
- Unit: Services/Validatoren isoliert testen (Mocks für Repositories)
- Integration: Gegen echte API/DB, Health/Metrics prüfen

