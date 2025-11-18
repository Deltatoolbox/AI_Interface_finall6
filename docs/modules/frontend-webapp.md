# Frontend – WebApp (React + TypeScript)

Dieser Leitfaden beschreibt Aufbau, Verantwortungen und Erweiterung des Frontends in `src/WebApp`.

## Aufgaben und Ziele
- UI für Login, Konversationsverwaltung und Chat
- Rendering von Markdown, Code und LaTeX
- Stabiles Scrolling (Seite fix, Inhalte scrollen intern)

## Struktur (wichtigste Orte)
- `src/WebApp/src/pages/ChatPage.tsx`: Hauptseite mit Layout, Laden von Konversationen/Nachrichten, Senden
- `src/WebApp/src/components/MessageList.tsx`: Nachrichtenliste, Markdown/Code/LaTeX-Rendering, Attachments
- `src/WebApp/src/components/ConversationList.tsx`: Sidebar-Konversationen, Auswahl/Umbenennen/Löschen
- `src/WebApp/src/index.css`: Tailwind-Basis, Layout- und Message-Styles (Seiten-Overflow aus)

## Datenfluss (vereinfacht)
1. ChatPage lädt Konversationen (`GET /api/conversations`)
2. Auswahl setzt `currentConversation` → lädt Messages (`GET /api/conversations/{id}`)
3. `sendMessage` → `POST /api/chat` → Antwort als neue Assistant-Message

## Scrolling-Prinzip
- `html/body/#root`: `overflow: hidden` (kein Seitenscroll)
- Sidebar (`ConversationList`) und `MessageList`: `overflow-y-auto` im jeweiligen Container
- Eingabefeld am Seitenboden fixiert (nicht scollend)

## Markdown/Code/Math
- Markdown via `react-markdown` + `remark-gfm`
- Code via `react-syntax-highlighter` (Themes: `tomorrow`/`vscDarkPlus`)
- LaTeX via `react-katex` mit defensiver Erkennung (`$`/`$$`)

## Erweiterung
- Neue Renderregeln: `MessageList` → `components`-Overrides in `ReactMarkdown`
- Neue Buttons/Flows: in `ChatPage` State/Handler anlegen und per Props an Komponenten geben
- Theming: über Tailwind-Klassen und `dark`-Mode auf `documentElement`

## Best Practices
- Keine Seitenscroller; immer interne Scroll-Container mit `min-h-0`
- Große Inhalte (Tables/Code) innerhalb `overflow-x-auto` kapseln
- Streaming-Status (`isStreaming`) nutzen, um doppelte Sends zu verhindern

