# Product Inventory API

A production-minded ASP.NET Core Web API for managing product inventory, built with Clean Architecture, EF Core (code-first), and a distributed-safe 6-digit product identifier.

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Web API (controllers, not minimal APIs) |
| Data access | EF Core 10, SQL Server, code-first migrations |
| Validation | FluentValidation, wired through a global `IAsyncActionFilter` |
| Logging | Serilog (console + rolling daily file sink under `logs/`) |
| API docs | Swashbuckle (Swagger UI in Development) |
| Health checks | `AspNetCore.HealthChecks.SqlServer` at `/health` |
| Unit tests | xUnit + NSubstitute (mocking) + FluentAssertions |
| Integration tests | xUnit + `WebApplicationFactory` + Testcontainers.MsSql (real SQL Server in a container) |
| BDD tests | Reqnroll (Gherkin feature files) on top of the same API/Testcontainers harness |
| Containerization | Multi-stage Dockerfile + Docker Compose (SQL Server + scalable API + nginx) |

## Solution structure

```
ProductInventory.slnx
src/
  ProductInventory.Domain/          Product entity, domain exceptions — no external dependencies
  ProductInventory.Application/     DTOs, IProductService/ProductService, FluentValidation validators, mapping, IProductRepository (interface only)
  ProductInventory.Infrastructure/  ApplicationDbContext, EF configuration, migrations, ProductRepository, seed data
  ProductInventory.Api/             Controllers, global exception handler, Program.cs composition root, Dockerfile
tests/
  ProductInventory.Application.UnitTests/   Service + validator unit tests (no DB, no HTTP)
  ProductInventory.Api.IntegrationTests/    Full HTTP + real SQL Server tests via Testcontainers
  ProductInventory.Bdd.Tests/               Reqnroll feature files reusing the integration test harness
docker-compose.yml, nginx/nginx.conf         Multi-instance API + SQL Server + load balancer
```

Dependency direction: `Domain` ← `Application` ← `Infrastructure`/`Api`. Controllers depend only on `IProductService`; nothing in `Application` or `Domain` references EF Core or ASP.NET Core.

## Prerequisites

Everything runs in containers, so a fresh clone only needs:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine / a Docker-API-compatible Podman setup) — running, with network access to pull `mcr.microsoft.com/mssql/server`, `mcr.microsoft.com/dotnet/*`, and `nginx` images
- Ports `8080` (nginx) and `1433` (SQL Server) free on the host
- Git, to clone the repository

No local .NET SDK, SQL Server, or `dotnet-ef` install is required to run the app — the Dockerfile builds it and migrations are applied automatically on container startup. The .NET SDK is only needed if you want to build/run outside Docker or work on the code:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `dotnet tool restore` (uses the pinned `dotnet-ef` version in `dotnet-tools.json` — no global install needed)

## Running locally

```powershell
docker compose up --build
# scale to multiple API instances behind the nginx load balancer:
docker compose up --build --scale api=3
```

This starts a SQL Server container, one or more API containers (migrations + seed data applied automatically on startup, see [Decisions](#decisions--trade-offs)), and an nginx reverse proxy on `http://localhost:8080`. Swagger UI is available at `http://localhost:8080/swagger` (Compose runs the API with `ASPNETCORE_ENVIRONMENT=Development` for local/demo convenience — see [Decisions](#decisions--trade-offs)). Logs are written to the console and to `logs/log-.txt` (rolling daily) inside each API container, persisted via the `api-logs` volume.

### Connecting via SSMS / Azure Data Studio

The SQL Server container publishes port `1433` to the host, so once `docker compose up` is running you can connect with any SQL client using:

| Setting | Value |
|---|---|
| Server name | `localhost,1433` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `YourStrong!Passw0rd` |
| Database | `ProductInventory` |

(Credentials are for local development only — see `docker-compose.yml`.)

## Running the tests

```powershell
# Unit tests — no external dependencies, always runnable
dotnet test tests/ProductInventory.Application.UnitTests

# Integration tests — require Docker (or Podman configured as a Docker-API-compatible host)
# to pull and run mcr.microsoft.com/mssql/server:2022-latest via Testcontainers
dotnet test tests/ProductInventory.Api.IntegrationTests

# BDD tests — same prerequisites as integration tests
dotnet test tests/ProductInventory.Bdd.Tests
```

> **Note:** integration and BDD tests each spin up their own isolated API instance and ephemeral SQL Server container via Testcontainers (integration tests share one instance across the collection; BDD tests boot a separate instance for their run), so the two suites never interfere with each other. All 20 integration tests and 8 BDD scenarios pass locally with Docker Desktop.

## API endpoints

| Method | Route | Notes |
|---|---|---|
| GET | `/api/products?page=&pageSize=` | Paginated (default page=1, pageSize=20, max 100) |
| POST | `/api/products` | Validates body; 201 with `Location` header |
| GET | `/api/products/{id}` | 404 if not found |
| PUT | `/api/products/{id}` | Requires `rowVersion` from a prior response; 409 on stale version |
| DELETE | `/api/products/{id}` | 204 on success, 404 if not found |
| POST | `/api/products/{id}/decrement-stock/{quantity}` | 200 with updated product, 409 if insufficient stock, 400 if quantity < 1 |
| POST | `/api/products/{id}/add-to-stock/{quantity}` | 200 with updated product, 400 if quantity < 1 |
| GET | `/api/products/search?name=` | Partial, case-insensitive match; `name` must be ≥ 2 characters |
| GET | `/api/products/stock-level?min=&max=` | Inclusive range; `min` must be ≤ `max` |

Every response that returns product(s) includes the current `stock` field, per the requirements.

## Decisions & trade-offs

- **6-digit product Id via SQL Server `IDENTITY(100000,1)`**, with default identity caching left **on**. The DB engine guarantees atomic, collision-free id generation regardless of how many API instances insert concurrently — no custom distributed-id generator is needed. Trade-off, chosen deliberately (performance over gap-avoidance): identity caching can produce gaps after a SQL Server failover/restart, and the 6-digit range caps the catalog at ~900,000 products (100000–999999). Both are acceptable for this exercise and documented rather than engineered around.
- **Stock mutation concurrency**: `decrement-stock`/`add-to-stock` use EF Core's `ExecuteUpdateAsync` with a conditional `WHERE Stock >= quantity` (for decrement), executing the check-and-update atomically in a single round trip. This avoids a read-modify-write race entirely, rather than relying on optimistic concurrency + retries.
- **Full-entity `PUT` concurrency**: uses the `RowVersion` (SQL Server `rowversion`) optimistic-concurrency token in the disconnected pattern — the client must echo back the `rowVersion` value from a previous response; a stale value results in `409 Conflict`. Chosen over pessimistic locking (which would hold DB locks across the request) and over manual version counters (which are easy to forget to bump).
- **No generic repository / Unit of Work**: the API has a single aggregate (`Product`), so one `IProductRepository` plus `DbContext.SaveChangesAsync` is enough. Introducing a generic repository or UoW abstraction here would add indirection without a corresponding benefit.
- **Manual mapping extensions instead of AutoMapper**: the DTO shapes are simple 1:1 projections; a mapping library would add a dependency and reflection overhead without meaningfully reducing code.
- **Validation**: FluentValidation validators are resolved and run automatically by a global `ValidationActionFilter` for any action argument with a registered `IValidator<T>`, so controllers never call validation code directly. Route-bound primitives (e.g. `quantity`) use `[Range]` data annotations instead, since a full FluentValidation validator would be overkill for a single bound integer.
- **Error handling**: a single `IExceptionHandler` (`GlobalExceptionHandler`) maps domain exceptions to RFC 7807 `ProblemDetails` responses (404 not found, 409 conflict/insufficient stock/concurrency, 400 validation, 500 fallback with the real exception logged but not leaked to the client).
- **Pagination & search**: `GET /api/products` supports optional `page`/`pageSize` (defaults 1/20, capped at 100) so the endpoint remains usable as the catalog grows; `search` requires at least 2 characters to avoid effectively-unfiltered full-table scans.
- **Read/write query shape**: read-only queries use `AsNoTracking()` (no benefit to change tracking when the result is never saved back); update/delete flows use a tracked lookup so EF can compute the diff and apply the `RowVersion` check.
- **Seeding**: sample products are seeded via EF Core migration `HasData` (deterministic, fixed `CreatedAtUtc`/Ids) rather than a runtime seeding routine, keeping the seed data itself under migration/version control.
- **Migrations on container startup**: the Docker Compose API service applies pending migrations automatically (`ApplyMigrationsOnStartup=true`) for a one-command demo experience. EF Core's migration history table is guarded by a database-level lock, so this is safe even with multiple API replicas starting concurrently — but in a real production pipeline, migrations would typically run as a separate, explicit release step rather than on every app instance's startup.
- **Load balancing in Compose**: nginx is configured with a `resolver`-based dynamic upstream (not a static `upstream` block) so it re-resolves Docker's embedded DNS and picks up all replicas when scaled with `--scale api=N`.
- **`ASPNETCORE_ENVIRONMENT=Development` in Compose**: Swagger is intentionally gated behind `IsDevelopment()`, and Compose is the primary way this project is run/demoed, so it sets `Development` to keep Swagger reachable at `http://localhost:8080/swagger`. A real production deployment would instead set `Production` and expose API docs (if at all) through a separate, access-controlled channel.

## Known limitations

- Product Id range is capped at ~900,000 values (100000–999999) by design; exceeding it would require re-keying, which is out of scope here.
- Identity caching means Ids are not strictly gap-free after a SQL Server restart/failover — uniqueness is still guaranteed, only sequentiality is not.
- Integration/BDD tests require Docker (or a Docker-API-compatible Podman setup) with network access to pull `mcr.microsoft.com/mssql/server` (see [Running the tests](#running-the-tests)).
