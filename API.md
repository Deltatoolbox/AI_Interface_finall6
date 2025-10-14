# AIGS API Documentation

## Overview

The AIGS provides a RESTful API for interacting with LM Studio's OpenAI-compatible interface. All API endpoints require authentication except for login and health checks.

## Authentication

The API uses JWT tokens for authentication. Tokens are issued upon successful login and should be included in subsequent requests.

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin"
}
```

**Response:**
- `200 OK`: Login successful, JWT token set as HTTP-only cookie
- `401 Unauthorized`: Invalid credentials

### Logout

```http
POST /api/auth/logout
```

**Response:**
- `204 No Content`: Logout successful

### CSRF Token

```http
GET /api/auth/csrf
```

**Response:**
```json
{
  "csrfToken": "guid-string"
}
```

## Models

### Get Available Models

```http
GET /api/models
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "id": "model-name",
    "object": "model",
    "created": 1234567890,
    "ownedBy": "lm-studio"
  }
]
```

## Chat

### Send Chat Message

```http
POST /api/chat
Content-Type: application/json
Authorization: Bearer <token>

{
  "model": "model-name",
  "messages": [
    {
      "role": "user",
      "content": "Hello, how are you?"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 1000,
  "top_p": 0.9,
  "stream": true
}
```

**Response:** Server-Sent Events stream

```
data: {"choices":[{"delta":{"content":"Hello"}}]}

data: {"choices":[{"delta":{"content":"!"}}]}

data: [DONE]
```

## Conversations

### Get Conversations

```http
GET /api/conversations
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "id": "conversation-id",
    "title": "Conversation Title",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

### Create Conversation

```http
POST /api/conversations
Content-Type: application/json
Authorization: Bearer <token>

{
  "title": "New Conversation"
}
```

**Response:**
```json
{
  "id": "conversation-id",
  "title": "New Conversation",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### Get Conversation with Messages

```http
GET /api/conversations/{id}
Authorization: Bearer <token>
```

**Response:**
```json
{
  "id": "conversation-id",
  "title": "Conversation Title",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "messages": [
    {
      "id": "message-id",
      "role": "user",
      "content": "Hello",
      "promptTokens": 5,
      "completionTokens": 0,
      "latencyMs": 0,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

## Health Checks

### Liveness Probe

```http
GET /health/live
```

**Response:**
- `200 OK`: Service is alive

### Readiness Probe

```http
GET /health/ready
```

**Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "description": "Database connection is healthy"
    },
    {
      "name": "LM Studio",
      "status": "Healthy",
      "description": "LM Studio connection is healthy"
    }
  ]
}
```

## Metrics

### Prometheus Metrics

```http
GET /metrics
```

**Response:** Prometheus-formatted metrics

```
# HELP http_requests_total Total number of HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="GET",endpoint="/api/models"} 10

# HELP active_streams Current number of active chat streams
# TYPE active_streams gauge
active_streams 2
```

## Error Responses

All error responses follow the Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request was invalid",
  "errors": [
    "Username is required",
    "Password must be at least 1 character"
  ]
}
```

## Rate Limiting

The API implements rate limiting per user:

- **Login endpoint**: 5 requests per minute
- **Other endpoints**: 60 requests per minute

Rate limit headers are included in responses:

```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 59
X-RateLimit-Reset: 1640995200
```

## WebSocket Support

The chat endpoint supports WebSocket connections for real-time streaming. Use the same authentication headers and send the chat request as the initial message.

## CORS

CORS is configured to allow requests from:
- `https://gateway.local`
- `http://localhost:5173` (development)

Credentials are supported for authenticated requests.
