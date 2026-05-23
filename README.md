# TaskFlow API

A task management REST API built with ASP.NET Core Web API, developed as a progressive learning project covering core Web API concepts from CRUD operations to JWT authentication and structured logging.

---

## Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server (EF Core Code First)
- **Authentication:** JWT Bearer Tokens
- **Logging:** Serilog (console + file sinks)
- **ORM:** Entity Framework Core 8

---

## Features

- Full CRUD for tasks and categories
- JWT authentication — register, login, token-based access
- User-scoped data — users only see and manage their own tasks
- DTO pattern — request and response models separated from database models
- Repository pattern — database logic decoupled from controllers
- Filtering — by completion status, category, and title keyword
- Sorting — by title, creation date, or completion status
- Pagination — configurable page size, capped at 50
- Global exception handling — consistent ProblemDetails responses
- Structured logging — every request and auth event logged with Serilog
- Data validation — enforced via Data Annotations on request DTOs

---

## Project Structure

```
TaskFlowAPI/
├── Controllers/          # HTTP layer — request handling and responses
├── Data/                 # AppDbContext
├── DTOs/                 # Request and response models
├── Exceptions/           # Custom exception types
├── Middleware/           # Global exception handling middleware
├── Migrations/           # EF Core database migrations
├── Models/               # Database entity models
├── Repositories/         # Data access layer
├── Services/             # Token generation service
└── Logs/                 # Runtime log files (gitignored)
```

---

## API Endpoints

### Auth
| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Create a new account |
| POST | `/api/auth/login` | Public | Login and receive a JWT |

### Tasks
| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/tasks` | Protected | Get all tasks (paginated, filterable) |
| GET | `/api/tasks/{id}` | Protected | Get a single task |
| POST | `/api/tasks` | Protected | Create a new task |
| PUT | `/api/tasks/{id}` | Protected | Replace a task |
| PATCH | `/api/tasks/{id}` | Protected | Partially update a task |
| DELETE | `/api/tasks/{id}` | Protected | Delete a task |

### Categories
| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/categories` | Public | Get all categories |
| GET | `/api/categories/{id}` | Public | Get a single category |
| POST | `/api/categories` | Public | Create a category |

---

## Query Parameters

```
GET /api/tasks?isCompleted=false
GET /api/tasks?categoryId=1
GET /api/tasks?searchTitle=study
GET /api/tasks?sortBy=title
GET /api/tasks?page=2&pageSize=10
GET /api/tasks?isCompleted=false&categoryId=1&sortBy=title&page=1&pageSize=5
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Postman (for testing)

### Setup

**1. Clone the repository:**
```bash
git clone https://github.com/Rethabile2004/taskflow-api.git
cd taskflow-api
```

**2. Update the connection string in `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskFlowDb;Trusted_Connection=True;"
}
```

**3. Apply migrations:**
```bash
dotnet ef database update
```

**4. Run the project:**
```bash
dotnet run
```

**5. Open Swagger UI:**
```
https://localhost:{port}/swagger
```

---

## Authentication

All task endpoints require a valid JWT. To authenticate:

1. Register via `POST /api/auth/register`
2. Copy the token from the response
3. Add it to your requests as a Bearer token:

```
Authorization: Bearer <your-token>
```

---

## Pagination Response Format

All paginated endpoints return this envelope:

```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 47,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "data": []
}
```

---

## What I Learned Building This

This project was built session by session as a structured learning exercise covering:

- How REST API design differs from MVC
- Why DTOs matter for separating concerns and protecting internal models
- How EF Core Code First migrations work in practice
- The Repository Pattern and why it makes controllers testable
- How JWT authentication works end to end — from token generation to claim extraction
- Global error handling with middleware and ProblemDetails
- Structured logging with Serilog

---

## Author

**Rethabile Eric Siase**
Advanced Diploma in Information Technology — Central University of Technology, Free State
GitHub: [@Rethabile2004](https://github.com/Rethabile2004)
