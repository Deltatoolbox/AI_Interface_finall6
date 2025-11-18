# Gateway.Application – Use Cases, DTOs, Validatoren

Enthält Anwendungslogik ohne Infrastrukturbindung. Orchestriert Repositories/Services über Interfaces.

## Aufgaben
- DTOs: Ein-/Ausgaben für APIs
- Services: AuthService, ChatService, ConversationService
- Validators: Eingabevalidierung (z. B. Titel, Nachrichten, Limits)

## Datenfluss
1. API schickt DTO an Service
2. Service validiert (Validatoren)
3. Service nutzt Interfaces (Domain) → konkrete Implementierung (Infrastructure)

## Erweiterung
- Neuen Use Case: Service + DTO(s) + Validator hinzufügen
- Cross-Cutting: Logging/Telemetry im Service (ohne Infrastruktur-Details)

