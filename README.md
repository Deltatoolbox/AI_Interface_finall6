# LM Studio Chat Gateway

Ein production-ready LAN Gateway für LM Studio mit .NET 8 Backend und React Frontend.

## 🚀 Features

### Backend (.NET 8)
- **ASP.NET Core Minimal APIs** mit Kestrel
- **JWT Authentication** mit konfigurierbaren Einstellungen
- **SQLite Database** mit Entity Framework Core
- **User Management** mit Admin/User Rollen
- **LM Studio Integration** über OpenAI-kompatible API
- **BCrypt Password Hashing** für sichere Passwörter
- **CORS Support** für Frontend-Integration

### Frontend (React + TypeScript)
- **React 18** mit Vite Build System
- **TypeScript** für Type Safety
- **TailwindCSS** für modernes Styling
- **JWT Token Management** mit Cookie-basierter Authentifizierung
- **Real-time Chat** mit Server-Sent Events (SSE)
- **Markdown Rendering** mit Syntax Highlighting
- **Responsive Design** für alle Geräte

### User Management
- **Konfigurierbare Registrierung** (Admin-only oder Self-Registration)
- **Rollen-basierte Zugriffskontrolle** (Admin/User)
- **Admin Dashboard** mit Statistiken
- **User Management UI** für Admin-Benutzer

## 🛠️ Tech Stack

### Backend
- .NET 8
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQLite Database
- JWT Bearer Authentication
- BCrypt.Net-Next
- Serilog

### Frontend
- React 18
- TypeScript
- Vite
- TailwindCSS
- React Router
- Lucide React Icons
- react-markdown
- react-syntax-highlighter

## 📁 Projektstruktur

```
AI_Interface/
├── src/
│   ├── SimpleGateway/          # .NET 8 Backend
│   │   ├── Program.cs          # Main entry point
│   │   ├── Models/             # Entity models
│   │   ├── Services/           # Business logic
│   │   ├── DTOs/               # Data Transfer Objects
│   │   └── Configuration/      # Settings classes
│   └── WebApp/                 # React Frontend
│       ├── src/
│       │   ├── components/     # React components
│       │   ├── pages/          # Page components
│       │   ├── contexts/      # React contexts
│       │   └── api.ts          # API client
│       └── package.json
├── start.sh                    # Start script
├── stop.sh                     # Stop script
└── status.sh                   # Status script
```

## 🚀 Quick Start

### Voraussetzungen
- .NET 8 SDK
- Node.js 18+
- LM Studio (mit heruntergeladenen Modellen)

### Installation

1. **Backend starten:**
```bash
cd src/SimpleGateway
dotnet run
```

2. **Frontend starten:**
```bash
cd src/WebApp
npm install
npm run dev
```

3. **Oder mit Scripts:**
```bash
./start.sh
```

### Standard-Login
- **Username:** admin
- **Password:** admin

## ⚙️ Konfiguration

### Backend (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "LM-Gateway",
    "Audience": "LM-Gateway-Users",
    "ExpirationMinutes": 60
  },
  "UserManagement": {
    "AllowSelfRegistration": true,
    "RequireEmailVerification": false,
    "DefaultRole": "User"
  }
}
```

### Frontend (vite.config.ts)
- Proxy-Konfiguration für API-Calls
- Development Server auf Port 5173
- Backend API auf Port 5000

## 🔐 Authentifizierung

Das System verwendet JWT-Token für die Authentifizierung:
- Token werden als HttpOnly Cookies gespeichert
- Automatische Token-Erneuerung
- Rollen-basierte Zugriffskontrolle

## 📊 Admin Features

- **Dashboard** mit System-Statistiken
- **User Management** für Benutzerverwaltung
- **Real-time Monitoring** der aktiven Verbindungen
- **Model Usage Tracking**

## 🎯 API Endpoints

### Authentication
- `POST /api/auth/login` - Benutzer-Login
- `POST /api/auth/register` - Benutzer-Registrierung

### Chat
- `GET /api/conversations` - Gespräche abrufen
- `POST /api/conversations` - Neues Gespräch erstellen
- `PUT /api/conversations/{id}` - Gespräch umbenennen
- `POST /api/chat` - Chat-Nachricht senden

### Admin
- `GET /api/admin/stats` - System-Statistiken
- `GET /api/admin/users` - Benutzer auflisten
- `POST /api/admin/users` - Benutzer erstellen
- `PUT /api/admin/users/{id}` - Benutzer aktualisieren
- `DELETE /api/admin/users/{id}` - Benutzer löschen

## 🚀 Deployment

### Production Build
```bash
# Backend
cd src/SimpleGateway
dotnet publish -c Release

# Frontend
cd src/WebApp
npm run build
```

### Docker (Optional)
Das Projekt kann mit Docker containerisiert werden.

## 📝 Entwicklung

### Debugging
- Backend: `dotnet run` mit Debug-Logs
- Frontend: `npm run dev` mit Hot Reload
- Debug-Seite: `/debug` für Auth-Status

### Testing
- API-Tests mit curl oder Postman
- Frontend-Tests mit Browser DevTools

## 🤝 Contributing

1. Fork das Repository
2. Erstelle einen Feature-Branch
3. Committe deine Änderungen
4. Push zum Branch
5. Erstelle einen Pull Request

## 📄 Lizenz

Dieses Projekt steht unter der MIT-Lizenz.

## 🎉 Meilensteine

- ✅ **MVP Backend** mit LM Studio Integration
- ✅ **React Frontend** mit Chat-Interface
- ✅ **SQLite Database** mit Persistierung
- ✅ **JWT Authentication** mit User Management
- ✅ **Admin Dashboard** mit Statistiken
- ✅ **User Management UI** für Admin-Benutzer
- ✅ **Navigation** zwischen allen Seiten
- ✅ **Production-ready** Architektur

---

**Entwickelt mit ❤️ für die LM Studio Community**