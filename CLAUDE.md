# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Commands

```bash
# Run the application (requires PostgreSQL running)
cd src/FabMatch.Web && dotnet run

# Run all tests
dotnet test

# Unit tests only (no Docker needed)
dotnet test --filter "Category!=Integration"

# Integration tests (requires Docker for Testcontainers PostgreSQL)
dotnet test --filter "Category=Integration"

# Build solution
dotnet build

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web \
    --output-dir Data/Migrations

# Apply migrations manually
dotnet ef database update \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web

# Start only PostgreSQL via Docker (for local dev)
docker compose up db -d

# Full stack via Docker
docker compose up -d
```

## Architecture

Clean Architecture with strict dependency direction: `Domain` ← `Application` ← `Infrastructure` ← `Web`

```
FabMatch.Domain        # Pure entities, enums, domain interfaces — zero external deps
FabMatch.Application   # CQRS handlers, application interfaces, pipeline behaviors
FabMatch.Infrastructure # EF Core, AI services, Stripe, MailKit, SignalR, file storage
FabMatch.Web           # Blazor Server + MudBlazor UI, auth endpoints, SignalR hub
tests/FabMatch.Tests   # xUnit, Moq, FluentAssertions, Testcontainers
```

### CQRS Pattern

Uses [martinothamar/Mediator](https://github.com/martinothamar/Mediator) — **source-generator based**, zero-reflection, compile-time dispatching (not MediatR).

Feature folder convention in `Application/Features/<Domain>/`:
- `Commands/<Name>/<Name>Command.cs` — the command/query record
- `Commands/<Name>/<Name>Handler.cs` — implements `ICommandHandler<,>` or `IQueryHandler<,>`
- `Commands/<Name>/<Name>Validator.cs` — FluentValidation (auto-discovered, runs via pipeline)

Pipeline order: `LoggingBehavior` → `ValidationBehavior` → handler.

### Domain Entities

`ApplicationUser` (Identity), `Client`, `Supplier`, `Project`, `Plan`, `Analysis`, `Match`, `Notification`, `Payment`, `ProductionCapability`

All entities use `Guid` PKs and soft-delete via `IsDeleted` (enforced by EF global query filters).

### Key Application Interfaces (`Application/Common/Interfaces/`)

| Interface | Infrastructure impl |
|---|---|
| `IAIService` | `OpenAIService` or `OllamaAIService` (config-driven) |
| `IPaymentService` | `StripePaymentService` |
| `IEmailService` | `MailKitEmailService` |
| `IFileStorageService` | `LocalFileStorageService` |
| `IPdfConverterService` | `PdfConverterService` (PDFtoImage/Pdfium) |
| `INotificationHubService` | `NotificationHubService` (SignalR) |
| `ITierPolicyService` | `TierPolicyService` (subscription limits) |
| `IUnitOfWork` | `UnitOfWork` (wraps `ApplicationDbContext`) |

### Authentication

Cookie-based via ASP.NET Core Identity. Login form POSTs to `/auth/login/execute` (Minimal API endpoint in `Program.cs`). The form uses a double-input checkbox pattern (`hidden value="false"` + `checkbox value="true"`), so the `rememberMe` parameter is bound as `string?` and parsed with `.Contains("true")`.

Roles: `Admin`, `Client`, `Supplier`. Policies: `AdminOnly`, `ClientOnly`, `SupplierOnly`.

Admin users are seeded on startup from `AdminSeed:Email` / `AdminSeed:Password` configuration.

### Infrastructure

- **EF Core 10 + Npgsql** — migrations in `Infrastructure/Data/Migrations/`
- **pg_trgm** GIN indexes applied idempotently on startup for supplier full-text search
- **AI provider** selected at startup via `AI:Provider` config (`OpenAI` or `Ollama`)
- **SignalR hub** at `/hubs/notifications`
- **Stripe webhook** controller at `/api/stripe/webhook`
- **Email** via MailKit SMTP (configured under `Smtp:`)

## Configuration Reference

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `AI:Provider` | `OpenAI` (default) or `Ollama` |
| `AI:OpenAI:ApiKey` | OpenAI API key |
| `AI:OpenAI:ChatModel` | Default: `gpt-4o` |
| `AI:OpenAI:EmbeddingModel` | Default: `text-embedding-3-small` |
| `AI:Ollama:BaseUrl` | Default: `http://localhost:11434` |
| `Payment:Stripe:SecretKey` | Stripe secret key |
| `Payment:Stripe:WebhookSecret` | Stripe webhook signing secret |
| `FileStorage:BasePath` | Upload directory |
| `Smtp:Host/Port/User/Password` | MailKit SMTP config |
| `AdminSeed:Email` | Admin account seeded on first startup |
| `AdminSeed:Password` | Admin password (min 8 chars, digit, uppercase) |

Environment variables use `__` instead of `:` (e.g. `AI__OpenAI__ApiKey`).

## Pending PBIs (Sprint 3)

- **PBI-A9**: Audit log — timestamped admin action history (lock, plan change, deletion)
- **PBI-A10**: Impersonate user — admin temporarily signs in as another user
- **PBI-17**: Full-text supplier/project search backed by pg_trgm (indexes already created)
- **PBI-20**: Two-factor authentication (TOTP, QRCoder package already referenced)
