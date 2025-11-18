# Chat Templates Feature

## Übersicht

Das Template-System ermöglicht es, vordefinierte Chat-Templates zu verwenden, die System-Prompts, Beispielnachrichten und Kategorien enthalten. Templates werden automatisch beim Start der Anwendung geladen.

## Verfügbare Templates

### 1. General Assistant
- **Kategorie**: General
- **Beschreibung**: Ein hilfreicher AI-Assistent für allgemeine Fragen und Aufgaben
- **Verwendung**: Alltägliche Konversationen und allgemeine Anfragen

### 2. Creative Writer
- **Kategorie**: Creative
- **Beschreibung**: Spezialisiert auf kreatives Schreiben, Storytelling und Content-Erstellung
- **Verwendung**: Geschichten schreiben, Charaktere entwickeln, kreative Brainstorming-Sessions

### 3. Code Assistant
- **Kategorie**: Programming
- **Beschreibung**: Programmierung und Software-Entwicklung
- **Verwendung**: Code schreiben, Debugging, Programmierungskonzepte erklären

### 4. Language Tutor
- **Kategorie**: Education
- **Beschreibung**: Sprachlern- und Übungsassistent
- **Verwendung**: Sprachen üben, Grammatik erklären, Übersetzungen

### 5. Business Analyst
- **Kategorie**: Business
- **Beschreibung**: Business-Strategie, Analyse und Planung
- **Verwendung**: Geschäftsanalysen, Business-Pläne, strategische Entscheidungen

### 6. Technical Support
- **Kategorie**: Technical
- **Beschreibung**: Technischer Support für Troubleshooting und Problemlösung
- **Verwendung**: Technische Probleme lösen, Software-/Hardware-Support

### 7. Research Assistant
- **Kategorie**: Research
- **Beschreibung**: Recherche- und Informationssammlung
- **Verwendung**: Recherche durchführen, Fakten prüfen, Informationen organisieren

### 8. Code Reviewer
- **Kategorie**: Programming
- **Beschreibung**: Code-Review und Qualitätssicherung
- **Verwendung**: Code auf Bugs, Best Practices und Verbesserungen prüfen

### 9. Data Analyst
- **Kategorie**: Analytics
- **Beschreibung**: Datenanalyse und Statistik
- **Verwendung**: Daten interpretieren, statistische Analysen, Datenvisualisierung

### 10. Content Editor
- **Kategorie**: Writing
- **Beschreibung**: Textbearbeitung und Verbesserung
- **Verwendung**: Grammatik, Stil, Klarheit und Inhaltsverbesserung

## Verwendung

### Im Frontend

1. Klicken Sie auf den "Templates" Button in der Chat-Seite
2. Wählen Sie eine Kategorie aus (optional)
3. Klicken Sie auf ein Template, um es zu verwenden
4. Das Template erstellt eine neue Konversation mit:
   - System-Prompt als erste Nachricht
   - Beispielnachrichten (falls vorhanden)

### Template-Struktur

Jedes Template enthält:
- **ID**: Eindeutige Identifikation
- **Name**: Anzeigename
- **Description**: Kurzbeschreibung
- **Category**: Kategorie für Filterung
- **SystemPrompt**: System-Prompt für den AI-Assistenten
- **ExampleMessages**: Array von Beispielnachrichten
- **IsBuiltIn**: Flag für Built-In-Templates

## API-Endpunkte

### Templates abrufen
```
GET /api/templates
```

### Kategorien abrufen
```
GET /api/templates/categories
```

### Template nach ID abrufen
```
GET /api/templates/{templateId}
```

### Templates nach Kategorie
```
GET /api/templates/category/{category}
```

### Template erstellen
```
POST /api/templates
Body: {
  "name": string,
  "description": string,
  "category": string,
  "systemPrompt": string,
  "exampleMessages": string[]
}
```

### Template aktualisieren
```
PUT /api/templates/{templateId}
Body: {
  "name": string,
  "description": string,
  "category": string,
  "systemPrompt": string,
  "exampleMessages": string[]
}
```

### Template löschen
```
DELETE /api/templates/{templateId}
```

### Built-In-Templates seeden
```
POST /api/templates/seed
```

## Technische Details

### Backend-Implementierung

Templates werden in der `ChatTemplateService` Klasse verwaltet:
- `GetAllTemplatesAsync()`: Alle Templates abrufen
- `GetTemplatesByCategoryAsync(category)`: Templates nach Kategorie
- `GetTemplateByIdAsync(templateId)`: Einzelnes Template
- `CreateTemplateAsync(userId, request)`: Neues Template erstellen
- `UpdateTemplateAsync(userId, templateId, request)`: Template aktualisieren
- `DeleteTemplateAsync(userId, templateId)`: Template löschen
- `SeedBuiltInTemplatesAsync()`: Built-In-Templates laden

### Automatisches Laden

Templates werden automatisch beim Start der Anwendung geladen:
- Die `SeedBuiltInTemplatesAsync()` Methode wird beim Start aufgerufen
- Nur fehlende Templates werden hinzugefügt (keine Duplikate)
- Built-In-Templates können nicht bearbeitet oder gelöscht werden

### Datenbank

Templates werden in der `ChatTemplates` Tabelle gespeichert:
- `Id`: Eindeutige ID
- `Name`: Template-Name
- `Description`: Beschreibung
- `Category`: Kategorie
- `SystemPrompt`: System-Prompt
- `ExampleMessages`: JSON-Array von Beispielnachrichten
- `IsBuiltIn`: Boolean-Flag
- `CreatedByUserId`: Ersteller (optional)
- `CreatedAt`: Erstellungsdatum
- `UpdatedAt`: Aktualisierungsdatum

## Erweiterung

### Neues Template hinzufügen

1. Template in `SeedBuiltInTemplatesAsync()` hinzufügen
2. Eindeutige ID verwenden
3. System-Prompt und Beispielnachrichten definieren
4. Kategorie zuweisen
5. Beim nächsten Start wird das Template automatisch geladen

### Custom Templates

Benutzer können eigene Templates erstellen über die API oder das Frontend (falls implementiert). Custom Templates können bearbeitet und gelöscht werden, im Gegensatz zu Built-In-Templates.

## Best Practices

1. **System-Prompts**: Sollten klar, präzise und zielgerichtet sein
2. **Beispielnachrichten**: Sollten typische Verwendungsfälle abdecken
3. **Kategorien**: Sollten konsistent und aussagekräftig sein
4. **Beschreibungen**: Sollten kurz und informativ sein

## Troubleshooting

### Templates werden nicht angezeigt

1. Überprüfen Sie, ob die Datenbank initialisiert wurde
2. Überprüfen Sie die Logs auf Fehler beim Seeding
3. Rufen Sie manuell `/api/templates/seed` auf (mit Berechtigung)

### Template wird nicht angewendet

1. Überprüfen Sie, ob das Template korrekt geladen wurde
2. Überprüfen Sie die Konversationserstellung im Frontend
3. Überprüfen Sie die Browser-Konsole auf Fehler

