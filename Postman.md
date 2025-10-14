# AIGS Postman Collection

This collection provides examples for testing the AIGS API endpoints.

## Setup

1. Import this collection into Postman
2. Set the `baseUrl` variable to your gateway URL (e.g., `http://localhost:5000`)
3. Run the "Login" request to get authenticated
4. Use other endpoints as needed

## Environment Variables

Create a Postman environment with these variables:

- `baseUrl`: `http://localhost:5000` (or your gateway URL)
- `token`: Will be set automatically after login
- `conversationId`: Will be set when creating conversations

## Collection Structure

### Authentication
- **Login**: Authenticate and get JWT token
- **Logout**: Clear authentication
- **Get CSRF Token**: Get CSRF token for state-changing requests

### Models
- **Get Models**: Retrieve available models from LM Studio

### Chat
- **Send Chat Message**: Send a chat message and receive streaming response

### Conversations
- **Get Conversations**: List user's conversations
- **Create Conversation**: Create a new conversation
- **Get Conversation**: Get conversation with messages

### Health & Monitoring
- **Health Live**: Liveness probe
- **Health Ready**: Readiness probe
- **Metrics**: Prometheus metrics

## Usage Examples

### 1. Basic Chat Flow

1. Run "Login" request
2. Run "Get Models" to see available models
3. Run "Create Conversation" to start a new conversation
4. Run "Send Chat Message" with your message
5. Observe the streaming response

### 2. Conversation Management

1. Run "Get Conversations" to see existing conversations
2. Run "Get Conversation" with a specific conversation ID
3. Continue chatting in existing conversations

### 3. Health Monitoring

1. Run "Health Live" to check basic health
2. Run "Health Ready" to check readiness
3. Run "Metrics" to see Prometheus metrics

## Notes

- The chat endpoint returns Server-Sent Events (SSE), which may not display properly in Postman's response view
- For production use, replace `localhost` with your actual domain
- Ensure LM Studio is running on port 1234 before testing
- Default credentials are `admin`/`admin` (change in production)

## Troubleshooting

- **401 Unauthorized**: Run the Login request first
- **Connection refused**: Check if the gateway service is running
- **No models available**: Ensure LM Studio is running and has models loaded
- **Rate limit exceeded**: Wait for the rate limit window to reset
