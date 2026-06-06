# TaskFlow API

> A production-deployed task management REST API — built to learn, shipped to prove it.

[![Live](https://img.shields.io/badge/Live%20on-Render-46E3B7?style=flat-square&logo=render&logoColor=white)](https://taskflow-api-wh4n.onrender.com)
[![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?style=flat-square&logo=swagger&logoColor=black)](https://taskflow-api-wh4n.onrender.com/index.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

---

## What this is

TaskFlow is a RESTful Web API for task management with full JWT authentication, user-scoped data, and a production deployment on Render with a managed PostgreSQL database.

Built session by session as a structured learning project — every feature added to understand *why*, not just *how*.

**Try it live:** https://taskflow-api-wh4n.onrender.com/index.html

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 8) |
| Database | PostgreSQL (Render managed) |
| ORM | Entity Framework Core 8 — Code First |
| Authentication | JWT Bearer Tokens |
| Logging | Serilog (console + rolling file) |
| Docs | Swagger / OpenAPI |
| Versioning | URL segment strategy (`/api/v1/`) |
| Deployment | Docker → Render |

---

## Features

- **JWT auth** — register, login, token-based access with expiry
- **User-scoped tasks** — users only see and manage their own data
- **DTO pattern** — request/response models fully separated from DB entities
- **Repository pattern** — data access decoupled from controllers
- **Filtering** — by completion status, category, and title keyword
- **Sorting** — by title, creation date, or completion status
- **Pagination** — configurable page size, server-side, capped at 50
- **PATCH support** — partial updates without replacing the full resource
- **Rate limiting** — separate limits for auth, read, and write endpoints
- **Global exception middleware** — consistent `ProblemDetails` error responses
- **Structured logging** — every request and error logged with Serilog
- **API versioning** — URL segment strategy, version headers on all responses
- **Auto-migration on startup** — EF Core migrations run on deploy

---

## Project structure

```
TaskFlowAPI/
├── Controllers/      # HTTP layer — routing, request/response handling
├── Data/             # AppDbContext
├── DTOs/             # Request and response models
├── Exceptions/       # Custom exception types
├── Middleware/       # Global exception handling middleware
├── Migrations/       # EF Core database migrations
├── Models/           # Database entity models
├── Repositories/     # Data access layer (ITaskRepository)
├── Services/         # JWT token generation
└── Logs/             # Runtime log files (gitignored)
```

---

## API reference

### Auth (public)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/register` | Create account, receive JWT |
| POST | `/api/v1/auth/login` | Login, receive JWT |

### Tasks (protected — requires JWT)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/tasks` | List tasks (paginated, filterable, sortable) |
| GET | `/api/v1/tasks/{id}` | Get a single task |
| POST | `/api/v1/tasks` | Create a task |
| PUT | `/api/v1/tasks/{id}` | Replace a task |
| PATCH | `/api/v1/tasks/{id}` | Partially update a task |
| DELETE | `/api/v1/tasks/{id}` | Delete a task |

### Categories (public)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/categories` | List all categories |
| GET | `/api/v1/categories/{id}` | Get a single category |
| POST | `/api/v1/categories` | Create a category |

---

## Query parameters — GET /api/v1/tasks

| Parameter | Type | Description |
|---|---|---|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Results per page (max: 50) |
| `isCompleted` | bool | Filter by completion status |
| `categoryId` | int | Filter by category |
| `searchTitle` | string | Keyword search on title |
| `sortBy` | string | `title`, `createdat`, `iscompleted` |

Example:
```
GET /api/v1/tasks?isCompleted=false&categoryId=2&searchTitle=study&sortBy=createdat&page=1&pageSize=10
```

---

## Pagination response envelope

```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 47,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "data": [
    {
      "id": 101,
      "title": "Study React",
      "description": "Complete module 4",
      "isCompleted": false,
      "createdAt": "2026-06-05T12:00:00Z",
      "categoryId": 2,
      "categoryName": "Personal"
    }
  ]
}
```

---

## Auth flow

```
POST /api/v1/auth/register  →  { token, expiresAt, fullName, email }
POST /api/v1/auth/login     →  { token, expiresAt, fullName, email }

All protected requests:
  Authorization: Bearer <token>
```

---

## Error responses

All errors return `application/problem+json`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "404",
  "status": 404,
  "detail": "Resource not found",
  "traceId": "..."
}
```

| Status | Meaning |
|---|---|
| 400 | Validation failed |
| 401 | Missing or invalid JWT |
| 404 | Resource not found (or not owned by user) |
| 409 | Conflict (e.g. email already registered) |
| 429 | Rate limit exceeded — retry with backoff |
| 500 | Unexpected server error |

---

## Running locally

**Prerequisites:** .NET 8 SDK, PostgreSQL running on port 5432

```bash
git clone https://github.com/Rethabile2004/taskflow-api.git
cd taskflow-api
```

Update `TaskFlowAPI/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskFlowDb;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "SecretKey": "YourLocalSecretKeyAtLeast32CharsLong",
    "Issuer": "TaskFlowAPI",
    "Audience": "TaskFlowAPIUsers",
    "ExpiryHours": 24
  }
}
```

```bash
dotnet run --project TaskFlowAPI
```

Migrations run automatically on startup. Swagger available at `http://localhost:8080`.

---

## Deployment

Deployed via Docker on Render with a managed PostgreSQL database.

All secrets (JWT key, DB connection string) are injected as environment variables — nothing sensitive is committed to the repository.

Production environment variables on Render:

| Key | Description |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Render internal PostgreSQL URL |
| `JwtSettings__SecretKey` | JWT signing key |
| `JwtSettings__Issuer` | Token issuer |
| `JwtSettings__Audience` | Token audience |
| `JwtSettings__ExpiryHours` | Token lifetime |

---

## What I learned building this

- How REST API design differs from MVC — controllers as a thin HTTP layer
- Why DTOs matter for protecting internal models and evolving the API without breaking consumers
- EF Core Code First migrations in practice — including running them automatically on startup in production
- The Repository Pattern — why it makes controllers testable and data access swappable
- JWT authentication end to end — token generation, claim extraction, and user-scoped authorization
- Global error handling with middleware and `ProblemDetails` — one place for all error logic
- Structured logging with Serilog — request tracing, log levels, rolling files
- API versioning — treating the API as a contract with consumers
- Docker multi-stage builds — keeping the final image lean
- Production deployment on Render — environment variables, managed databases, and debugging startup failures from logs

---

## Author

**Rethabile Eric Siase**
Advanced Diploma in Information Technology — *Cum Laude*
Central University of Technology, Free State

[![GitHub](https://img.shields.io/badge/GitHub-Rethabile2004-181717?style=flat-square&logo=github)](https://github.com/Rethabile2004)
