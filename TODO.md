# Audit Findings & TODOs

This document lists bugs, security vulnerabilities, and missing features identified during the code audit.

## 🐛 Bugs

- [x] **Chat Persistence Failure**: In `Program.cs`, the `/api/chat` endpoint generates a new random `conversationId` for the chat stream instead of using the ID of the conversation created immediately before. This causes `ChatService.SaveMessagesAsync` to fail (FK violation) when trying to save messages to a non-existent conversation.
  - *Fix*: Use the `Id` from the `ConversationResponse` returned by `conversationService.CreateConversationAsync` in `Program.cs`.

## 🔒 Security Issues

- [x] **Hardcoded JWT Key**: `appsettings.json` contains a placeholder JWT key (`your-super-secret-jwt-key...`). This is a risk if not changed in production.
  - *Fix*: Ensure this is overridden by environment variables and document the requirement.
- [x] **Weak Default Admin Password**: `Program.cs` defaults the admin password to "admin".
  - *Fix*: Require `ADMIN_PASSWORD` env var or generate a strong random password on startup if not set, and log it.
- [ ] **Missing CSRF Protection**: While a CSRF token endpoint exists (`/api/auth/csrf`), there is no middleware validating the token on state-changing requests (POST/PUT/DELETE).
  - *Fix*: Add Antiforgery middleware or manual token validation.
- [x] **Permissive CORS**: `Program.cs` allows any method and header (`AllowAnyMethod`, `AllowAnyHeader`) which might be too permissive compared to `appsettings.json` configuration.
  - *Fix*: Restrict to configured origins and headers.

## 🚀 Missing Features

- [ ] **User Management**: No endpoints exist to create, list, or delete users. Only the default admin user exists.
  - *Feature*: Add `POST /api/users`, `GET /api/users`, `DELETE /api/users/{id}` (Admin only).
- [ ] **Password Management**: No mechanism for users to change their password or reset it.
  - *Feature*: Add `POST /api/auth/change-password`.
- [ ] **Rate Limiting**: Rate limiting configuration exists in `appsettings.json` but the middleware is not registered in `Program.cs`.
  - *Fix*: Add `app.UseRateLimiter()` and configure policies.
- [ ] **Registration**: No public user registration flow.
  - *Feature*: Add `POST /api/auth/register` (optional, depending on requirements).
