# Code-Kommentare und Architektur-Guide

Dieser Leitfaden kommentiert den vorhandenen Code und erklärt Aufbau, Verantwortung und Zusammenspiel der wichtigsten Module – ohne den Quellcode zu verändern. Ziel: Schneller Überblick für neue Entwickler, klare Einstiegspunkte zum Debuggen und Erweitern.

Hinweis: Detailbereiche, die hier nicht als Codeausschnitte zitiert sind, werden anhand der Dateinamen und Projektstruktur beschrieben.

## Übersicht – Lösungskomponenten
- Frontend: `src/WebApp` (React + TypeScript, Vite, Tailwind)
- Voll-Stack Backend (modular): `src/Gateway.*` (Api, Application, Domain, Infrastructure)
- Alternative Monolith: `src/SimpleGateway` (API + DB + Migrations in einem Projekt)
- Deployment/Infra: `Caddyfile`, `deploy/*` (Caddy, Nginx, systemd)
- Tests: `tests/*` (Unit + Integration)

---

## Frontend – React WebApp (`src/WebApp`)
Zentrale Aufgaben: Auth, Konversationen laden/anlegen, Nachrichten anzeigen/senden, UI/UX (Scrolling, Dark Mode, Code/Math-Rendering).

### Globale Styles: `src/WebApp/src/index.css`
- Setzt Basis-Reset und erzwingt `overflow: hidden` auf `html, body, #root`, damit das Seiten-Scrolling deaktiviert ist und nur Inhaltsbereiche intern scrollen.
- Tailwind-Komponenten-Klassen für Nachrichtenblöcke (`.message-user`, `.message-assistant`, `.message-system`).

### Seite: `src/WebApp/src/pages/ChatPage.tsx`
Verantwortungen:
- Layout der gesamten Chat-Seite (Header, Sidebar, Hauptbereich)
- Laden/Verwalten von Konversationen und Nachrichten
- Triggern von Sendevorgängen und Template-Workflows
- Korrekte Scroll-Container-Aufteilung (Seite: kein Scroll; Sidebar + Messages: intern scrollen)

Wichtige Punkte:
- State: `conversations`, `currentConversation`, `messages`, `isStreaming`.
- Lifecycle: `useEffect` lädt Konversationen und beim Wechsel `currentConversation` die Nachrichten.
- Actions:
  - `createConversation(title, model?, category?)`
  - `loadConversations()` / `loadMessages(conversationId)`
  - `sendMessage(content, files?)` inkl. File-Annotationen in `content`
  - `handleConversationRename`, `handleConversationDelete`
  - Template-Flow: `handleSelectTemplate(template)` erzeugt eine Konversation mit Systemprompt und optionalen Beispielnachrichten.
- Layout:
  - Header fest positioniert (kein Scroll)
  - Linke Sidebar: Button „New Conversation“ (fix), darunter `ConversationList` als eigener Scrollbereich
  - Hauptbereich: Nachrichtenliste als eigener Scrollbereich + Eingabe fix am unteren Rand

### Komponenten: `src/WebApp/src/components/MessageList.tsx`
Verantwortungen:
- Darstellung der Nachrichtenliste inkl. Markdown, Code-Highlighting und LaTeX (`react-markdown`, `react-syntax-highlighter`, `react-katex`).
- Scroll-Handling zum Ende mittels `messagesEndRef` bei Änderungen der `messages`.
- Rendering von Datei-Anhängen (Bilder, Text-Dateien) mit Icons und Größenangabe.

Wichtige Punkte:
- Der umschließende Div ist als interner Scroll-Container ausgelegt (`h-full overflow-y-auto`), damit die Eingabe im Hauptlayout fix bleiben kann.
- Dark-Mode-Erkennung für Code-Theme (`vscDarkPlus` vs. `tomorrow`).
- Selektives Rendern von LaTeX-Inhalten nur bei Bedarf (Performance und Robustheit bei fehlerhaften Ausdrücken).

### Komponenten: `src/WebApp/src/components/ConversationList.tsx`
Verantwortungen:
- Listet Konversationen, erlaubt Auswahl, Umbenennen, Löschen, Teilen.
- Eigener Scroll-Container in der Sidebar (`overflow-y-auto`).

Interaktionen:
- `onConversationSelect`, `onConversationRename`, `onConversationDelete` werden nach außen delegiert (Lifting State Up in `ChatPage`).
- Inline-Edit mit Enter/Escape sowie Blur-Save; einfache Bestätigungsdialoge für Delete.

---

## Backend – Modularer Aufbau (`src/Gateway.*`)
Ziel: Saubere Trennung von Schichten und Verantwortlichkeiten.

### `Gateway.Domain`
- Entities: `Conversation`, `Message`, `UsageLog`, `User`
- Interfaces: Repository-Verträge und technische Abstraktionen (`IConversationRepository`, `IMessageRepository`, `IUserRepository`, `IUsageLogRepository`, `ITokenService`, `IConcurrencyManager`, `ILmStudioClient`)

Kommentar: Die Domain-Schicht kennt keine Infrastrukturdetails (z. B. keine EF Core Klassen). Sie definiert Kernmodelle und Ports.

### `Gateway.Application`
- DTOs: Transportobjekte zwischen API/UI und Domain
- Services: Geschäftslogik (Auth, Chat, Conversation) orchestriert Domain-Objekte + Repositories
- Validators: Eingabevalidierung (z. B. FluentValidation)

Kommentar: Enthält Anwendungsfälle/Use-Cases. Keine Framework-spezifische Persistenz – nutzt Interface-gebundene Repositories/Services.

### `Gateway.Infrastructure`
- `Data/GatewayDbContext.cs`: EF Core DbContext, Mapping und DB-Zugriff
- `Repositories/*Repository.cs`: konkrete EF Core Implementierungen der Domain-Repositories
- `Services/*`: technische Dienste (ConcurrencyManager, LmStudioClient, TokenService)

Kommentar: Hier werden Infrastruktur-Details implementiert (DB, HTTP-Clients, Token, Concurrency).

### `Gateway.Api`
- .NET Minimal API Projekt
- Konfiguration von Routing, DI, AuthN/AuthZ, Health, Metrics etc.

Kommentar: Komponiert die Schichten zusammen, stellt HTTP-Endpunkte bereit, injiziert Services/Repos.

---

## Alternative Backend – `src/SimpleGateway`
Monolithische Variante mit folgenden Komponenten:
- `Data/GatewayDbContext.cs`: EF Core Kontext mit Migrations
- `Services/*`: Anwendungslogik in einem Projekt (anstelle separater Schichten)
- `Migrations/*`: EF Core Migrations für SQLite
- `Program.cs`: Startup und Endpunkte in einem Projekt

Einsatz: Schnelle lokale Inbetriebnahme, kleinere Deployments oder einfache Setups.

---

## Tests (`tests/*`)
- `Gateway.UnitTests`: Unittests für Services/Validatoren/Repos (isoliert)
- `Gateway.IntegrationTests`: End-to-End nahe Tests gegen API/DB/Health

Nutzen:
- Qualitätssicherung, Regressionserkennung, Dokumentation der erwarteten Verhaltensweisen.

---

## Deployment & Betrieb (`deploy/*`, `Caddyfile`, `nginx.conf`)
- Caddy: statisches Hosting des Frontends, Reverse Proxy auf API (`/api → Backend`), einfache lokale TLS-Optionen
- Nginx: alternative Proxy-Konfiguration
- systemd: Service-Datei für Linux-Deployment, Logrotation via Journald/Datei

Betriebshilfen:
- Skripte im Projekt-Root: `start*.sh`, `stop*.sh`, `status.sh`

---

## API & Integrationen
- OpenAI-kompatible Chat-API (`/api/chat`) für LM Studio
- Konversations-Endpunkte (`/api/conversations/*`) zum Persistieren und Laden
- Auth (`/api/auth/*`) mit JWT in HttpOnly-Cookies
- Health (`/health/live`, `/health/ready`), Metrics (`/metrics`)

Details in `docs/API.md`.

---

## Monitoring & Security
- Monitoring/Observability (Prometheus, OpenTelemetry, Logs, Traces) – siehe `docs/MONITORING.md`
- Security (JWT, Passwörter, CORS, Reverse Proxy Hardening, Compliance) – siehe `docs/SECURITY.md`

---

## Typische Fehlerbilder und Hinweise
- Scrolling: Seite selbst scrollt nicht; interne Bereiche `ConversationList`/`MessageList` scrollen. Achten Sie auf `min-h-0` und `overflow-y-auto` in Flex-Layouts.
- Streaming/Abbruch: `abortControllerRef` in `ChatPage` steuert laufende Requests; UI-Flag `isStreaming` für Eingabe-Disable.
- File Attachments: Bilder als `<img>` mit `maxHeight`, Textdateien in `<pre>` mit `overflow-x-auto`.
- Dark Mode: Code-Theme wechselt dynamisch nach `document.documentElement.classList`.

---

## Erweiterungspunkte
- Neue Message-Rendertypen: `MessageList` via `ReactMarkdown`-Components erweitern (z. B. Tabellen, Diagramme)
- Weitere Storage/DB: Repositories in `Gateway.Infrastructure` implementieren, Interfaces bleiben stabil
- Auth-Provider: `ITokenService`-Implementierung austauschen/erweitern
- Rate Limits/Concurrency: `IConcurrencyManager` und Metriken erweitern

---

## Glossar (kurz)
- Domain: Fachlogik, entkoppelt von technischen Details
- Application: Orchestriert Anwendungsfälle, nutzt Domain und Ports
- Infrastructure: Technische Adapter, konkretisieren Ports (DB, HTTP, Token)
- Minimal API: Leichtgewichtige HTTP-Endpunkte in ASP.NET Core
- LM Studio: Lokale LLM-Laufzeit mit OpenAI-kompatiblem Interface

---

## Nächste Schritte
- Lesen: `README.md` (Root) → `docs/API.md` → `docs/MONITORING.md` → `docs/SECURITY.md`
- Dev: Frontend `npm run dev`, Backend `dotnet run --project src/Gateway.Api`
- Tests: `dotnet test`

> Dieser Leitfaden ist bewusst code-nah, verändert aber keinen Quellcode. Für tiefergehende Reviews empfehlen sich Component- oder PR-spezifische Kommentare.

