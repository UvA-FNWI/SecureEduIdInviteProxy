# Secure EduId Invite Proxy

## Overview

The Secure EduId Invite Proxy is a service that acts as an intermediary between applications and the EduId invitation API. It solves a critical security limitation in the EduId platform, which does not natively support role-based differentiation for API tokens.

In the EduId system, a single API token grants permission to invite users for any role. This proxy service adds a security layer that ensures applications can only invite users with specific roles that are coupled to their respective API tokens.

## Key Features

- **Role-Based Token Authorization**: Maps specific API tokens to specific roles, ensuring applications can only invite users for their authorized roles
- **Request Validation**: Validates incoming invitation requests to ensure they contain exactly one role identifier
- **Transparent Proxying**: Forwards valid requests to the EduId Invitation API with appropriate authentication
- **Comprehensive Auditing**: Logs all invitation operations with detailed information for security and compliance purposes
- **Azure Monitor Integration**: Optional integration with Azure Application Insights for monitoring and logging
- **Kubernetes Ready**: Includes Helm charts for deployment to Kubernetes environments

## Architecture

The solution follows a vertical slice architecture with clear separation between:

- **Infrastructure**: Configuration, monitoring, and external service connections
- **Persistence**: Audit logging and data storage
- **Presentation**: API endpoints and DTOs

## Configuration

The application uses the standard .NET configuration system with the following key sections:

### EduId Configuration

| Setting | Description |
|---|---|
| `InvitationApiUrl` | Base URL of the SURFconext Invite API |
| `InvitationApiToken` | API token for authenticating with the SURFconext Invite API |
| `RoleIds` | Maps each internal role name to its numeric SURFconext role ID |
| `RoleTokens` | Maps each internal role name to the token the calling application must send |

The `RoleIds` and `RoleTokens` together enforce that a caller can only invite users for the role they are authorised for. The numeric role ID is assigned by SURF when you create a role in the SURFconext Invite admin panel, it is visible in the URL.

```json
{
  "EduId": {
    "InvitationApiUrl": "<url>",
    "InvitationApiToken": "<token>",
    "RoleIds": {
      "<role-name>": "<numeric-surf-role-id>"
    },
    "RoleTokens": {
      "<role-name>": "<api-token>"
    }
  }
}
```

### Azure Monitor Configuration (Optional)

Configuration for Azure Application Insights:

```json
{
  "AzureMonitor": {
    "ConnectionString": "your-application-insights-connection-string"
  }
}
```

## API Endpoints

### Create Invitation

```
POST /api/external/v1/invitations
```

**Headers:**
- `X-API-TOKEN`: The API token for the specific role

**Request Body:**
```json
{
  "roleIdentifiers": ["role-id"],
  "invites": ["email1@example.com", "email2@example.com"]
}
```

**Response:**
- `200 OK`: Invitation created successfully
- `400 Bad Request`: Invalid request (e.g., multiple role identifiers)
- `401 Unauthorized`: Invalid API token
- `500 Internal Server Error`: Error from the EduId invitation service

## Deployment

The service can be deployed as a Docker container or to a Kubernetes cluster using the provided Helm charts.

### Docker

```bash
docker build -t secure-eduid-invite-proxy .
docker run -p 8080:80 secure-eduid-invite-proxy
```

### Kubernetes (Helm)

```bash
helm install eduidproxy ./charts/eduidproxy
```

## Development

### Prerequisites

- .NET 9.0 SDK
- Docker (optional)
- Kubernetes and Helm (optional for deployment)

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run --project UvA.SecureEduIdInviteProxy
```

### Testing

```bash
dotnet test
```

## Security Considerations

- API tokens should be kept secure and rotated regularly
- All invitation operations are logged for audit purposes
- The service validates that only one role can be specified per invitation request
- IP addresses of clients are logged for security tracking

## License

[Specify your license here]
