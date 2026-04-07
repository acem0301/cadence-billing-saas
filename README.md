# Cadence Billing

A multi-tenant SaaS billing API built with .NET 10 and Clean Architecture. Designed to demonstrate production-ready patterns for managing customers, invoices, and automated billing cycles.

## Features

- **Multi-tenancy** — complete data isolation between tenants via global EF Core query filters
- **JWT Authentication** — stateless auth with tenant resolution from token claims
- **Invoice State Machine** — domain-enforced transitions (Draft → Approved → Sent → Paid / Cancelled)
- **Automated Billing** — background job that generates invoices from billing cadences on a daily schedule
- **Clean Architecture** — Domain, Application, Infrastructure, and Api layers with clear separation of concerns
- **Global Exception Handling** — middleware-based error handling with appropriate HTTP status codes
- **Test Coverage** — unit tests for domain logic and integration tests for API endpoints

## Tech Stack

- **Runtime**: .NET 10 / C#
- **Framework**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Database**: SQL Server (via Docker)
- **Auth**: JWT Bearer tokens + BCrypt password hashing
- **Testing**: xUnit, FluentAssertions, WebApplicationFactory

## Architecture
src/
├── Domain/          # Entities, enums, exceptions, repository interfaces
├── Application/     # Use cases (queries and commands), DTOs, abstractions
├── Infrastructure/  # EF Core, repositories, JWT service, background job
└── Api/             # Controllers, middleware, Program.cs
tests/
├── CadenceBilling.UnitTests/        # Domain logic tests
└── CadenceBilling.IntegrationTests/ # API endpoint tests

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for SQL Server)

### 1. Start SQL Server
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configure the API

Edit `src/Api/appsettings.json` with your connection string and JWT settings:
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=CadenceBilling;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "cadence-billing",
    "Audience": "cadence-billing"
  }
}
```

### 3. Apply Migrations
```bash
cd src/Api
dotnet ef database update
```

### 4. Run the API
```bash
dotnet run --project src/Api
```

The API will be available at `https://localhost:5187`.

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Register a new tenant and user |
| POST | `/auth/login` | Login and receive a JWT |

### Customers
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/customers` | List all customers for the current tenant |
| POST | `/customers` | Create a new customer |

### Invoices
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/invoices` | List all invoices for the current tenant |
| POST | `/invoices` | Create a new invoice |
| POST | `/invoices/{id}/transition` | Transition invoice status |

### Billing Cadences
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/billing-cadences` | List all billing cadences |
| POST | `/billing-cadences` | Create a billing cadence |

## Invoice State Machine

Draft ──→ Approved ──→ Sent ──→ Paid
│           │          │
└───────────┴──────────┴──→ Cancelled

Transitions are enforced at the domain level. Invalid transitions return `400 Bad Request`.

## Running Tests
```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/CadenceBilling.UnitTests

# Integration tests only
dotnet test tests/CadenceBilling.IntegrationTests
```