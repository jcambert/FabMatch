# FabMatch – Industrial Sheet Metal & Machining Matchmaking SaaS

FabMatch is a B2B SaaS platform that connects manufacturing buyers (clients) with sheet metal / CNC machining suppliers using **AI-driven affinity matching**. Clients upload technical drawings, the AI analyses them to identify processes and estimate costs, and the platform automatically proposes the best-matched suppliers.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Technology Stack](#technology-stack)
3. [Solution Structure](#solution-structure)
4. [Key Features](#key-features)
5. [Getting Started](#getting-started)
6. [Configuration](#configuration)
7. [Database Migrations](#database-migrations)
8. [Running Tests](#running-tests)
9. [Docker Deployment](#docker-deployment)
10. [AI Integration](#ai-integration)
11. [Payment Integration](#payment-integration)
12. [Product Backlog (PBIs)](#product-backlog-pbis)

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│  FabMatch.Web  (Blazor Server + MudBlazor + SignalR)             │
│  ┌──────────────┐  ┌────────────────┐  ┌──────────────────────┐ │
│  │  Pages/      │  │ Layout/NavMenu │  │  NotificationHub     │ │
│  │  Components  │  │  (MudBlazor)   │  │  (SignalR)           │ │
│  └──────────────┘  └────────────────┘  └──────────────────────┘ │
└────────────────────────────┬─────────────────────────────────────┘
                             │ IMediator (CQRS)
┌────────────────────────────▼─────────────────────────────────────┐
│  FabMatch.Application  (CQRS Commands/Queries/Handlers)          │
│  ┌────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  │  Features/     │  │  Interfaces/     │  │  Behaviors/      │ │
│  │  Auth/Projects │  │  IAIService      │  │  Logging+Valid.  │ │
│  │  Matches/Plans │  │  IPaymentService │  │  (Pipeline)      │ │
│  └────────────────┘  └──────────────────┘  └──────────────────┘ │
└────────────────────────────┬─────────────────────────────────────┘
                             │ IUnitOfWork
┌────────────────────────────▼─────────────────────────────────────┐
│  FabMatch.Infrastructure  (EF Core + External Services)          │
│  ┌────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  │  PostgreSQL     │  │  OpenAI / Ollama │  │  Stripe          │ │
│  │  Repositories  │  │  AI Service      │  │  Payment Service │ │
│  └────────────────┘  └──────────────────┘  └──────────────────┘ │
└────────────────────────────┬─────────────────────────────────────┘
                             │ Domain entities
┌────────────────────────────▼─────────────────────────────────────┐
│  FabMatch.Domain  (Pure domain – no dependencies)                │
│  Client | Supplier | Project | Plan | Match | Notification       │
└──────────────────────────────────────────────────────────────────┘
```

The solution follows **Clean Architecture** with the **CQRS** pattern powered by
[Mediator](https://github.com/martinothamar/Mediator) (Martin Othamar's source-generator based library –
zero reflection, compile-time generated dispatcher).

---

## Technology Stack

| Concern | Library / Framework |
|---|---|
| Runtime | .NET 10, C# 14 |
| Web UI | Blazor Server + MudBlazor 7 |
| CQRS | Mediator 2.x (martinothamar) |
| Validation | FluentValidation 11 |
| ORM | EF Core 10 + Npgsql |
| Database | PostgreSQL 16 |
| Identity | ASP.NET Core Identity |
| Real-time | SignalR |
| AI | OpenAI SDK 2 (gpt-4o / text-embedding-3-small) |
| Alternative AI | Ollama (local, configurable) |
| Payments | Stripe.net 47 |
| PDF→Image | PDFtoImage (Pdfium) |
| Logging | Serilog (console, file, PostgreSQL sink) |
| Testing | xUnit 2.9, Moq, FluentAssertions, Testcontainers |
| Containerisation | Docker + Docker Compose |

---

## Solution Structure

```
FabMatch/
├── src/
│   ├── FabMatch.Domain/          # Entities, enums, value objects, interfaces
│   ├── FabMatch.Application/     # CQRS handlers, application interfaces, behaviors
│   ├── FabMatch.Infrastructure/  # EF Core, repositories, AI/payment/file services
│   └── FabMatch.Web/             # Blazor Server app (MudBlazor UI + SignalR hub)
├── tests/
│   └── FabMatch.Tests/           # Unit & integration tests (xUnit + Testcontainers)
├── deploy/
│   ├── postgres/init.sql         # PostgreSQL initialization script
│   └── nginx/nginx.conf          # Nginx reverse-proxy configuration
├── Dockerfile
├── docker-compose.yml
└── docker-compose.override.yml   # Development overrides
```

---

## Key Features

### Client Features
- Register / login with email + password (ASP.NET Core Identity)
- Create multiple manufacturing projects with budget, delivery date, processes and materials
- Upload technical plans in **PNG, JPEG or PDF** format
  - PDF plans are automatically converted to images via Pdfium before AI analysis
- AI analysis of each plan: processes identified, materials detected, cost estimates
- View AI-proposed supplier matches per project with affinity scores and rationale
- Accept or reject match proposals
- Real-time notifications via SignalR when new matches arrive

### Supplier Features
- Full company profile with presentation text
- Detailed production capabilities (machines, tolerances, processes)
- Materials, certifications, capacity information
- AI embedding generated automatically from profile text
- Receive notifications when a project matches your profile
- Browse active client projects

### System / AI Features
- Embedding-based cosine similarity for fast large-scale matching
- GPT-4o vision analysis of technical drawings
- Configurable AI provider (OpenAI ↔ Ollama) via `appsettings.json`
- CQRS pipeline with logging and validation behaviors
- Stripe subscription and one-off payment support
- Soft-delete across all entities

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- An OpenAI API key (or a local Ollama instance)
- A Stripe account (test keys are sufficient for development)

### 1. Clone and configure

```bash
git clone <repo-url>
cd FabMatch

# Copy the example env file
cp .env.example .env
# Edit .env and fill in your API keys
```

### 2. Run with Docker Compose (recommended)

```bash
# Start the database and application
docker compose up -d

# To also start pgAdmin (dev only)
docker compose --profile dev up -d
```

The application will be available at **http://localhost:8080**.

### 3. Run locally (development)

```bash
# Start only the database
docker compose up db -d

# Run the web app
cd src/FabMatch.Web
dotnet run
```

The app will be available at **https://localhost:5001** / **http://localhost:5000**.

---

## Configuration

All configuration is via `appsettings.json` / environment variables.

### Key settings

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `AI:Provider` | `OpenAI` or `Ollama` |
| `AI:OpenAI:ApiKey` | Your OpenAI API key |
| `AI:OpenAI:ChatModel` | Chat model (default: `gpt-4o`) |
| `AI:OpenAI:EmbeddingModel` | Embedding model (default: `text-embedding-3-small`) |
| `AI:Ollama:BaseUrl` | Ollama base URL (default: `http://localhost:11434`) |
| `Payment:Stripe:SecretKey` | Stripe secret key |
| `Payment:Stripe:PublishableKey` | Stripe publishable key |
| `Payment:Stripe:WebhookSecret` | Stripe webhook signing secret |
| `FileStorage:BasePath` | Directory for uploaded plan files |

### Environment variable format

Docker Compose and ASP.NET Core support replacing `:` with `__`:

```bash
ConnectionStrings__DefaultConnection=Host=db;Database=fabmatch;...
AI__OpenAI__ApiKey=sk-...
```

---

## Database Migrations

EF Core migrations are generated and applied via the `dotnet ef` CLI:

```bash
# Install EF tools (once)
dotnet tool install --global dotnet-ef

# Generate a migration
dotnet ef migrations add <MigrationName> \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web \
    --output-dir Data/Migrations

# Apply migrations
dotnet ef database update \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web
```

> Migrations also run automatically on application startup.

---

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only (fast)
dotnet test --filter "Category!=Integration"

# Integration tests (require Docker for PostgreSQL container)
dotnet test --filter "Category=Integration"

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Docker Deployment

### Build image

```bash
docker build -t fabmatch:latest .
```

### Production deployment with Nginx TLS

1. Place your TLS certificates in `deploy/nginx/certs/`
2. Update the `server_name` in `deploy/nginx/nginx.conf`
3. Start with the production profile:

```bash
docker compose --profile production up -d
```

### Environment file (`.env`)

```env
DB_PASSWORD=your_secure_db_password
OPENAI_API_KEY=sk-...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
```

---

## AI Integration

### OpenAI (default)

The system uses:
- **gpt-4o** for plan image analysis and match rationale generation
- **text-embedding-3-small** for semantic embedding vectors

### Switching to Ollama (local / private)

Set `AI:Provider` to `Ollama` in `appsettings.json`:

```json
{
  "AI": {
    "Provider": "Ollama",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "Model": "llava",
      "EmbeddingModel": "mxbai-embed-large"
    }
  }
}
```

### Adding a custom AI provider

1. Implement `IAIService` in `FabMatch.Infrastructure/Services/AI/`
2. Register it in `DependencyInjection.cs` with the appropriate config switch

---

## Payment Integration

Stripe integration supports:
- **One-off payments** (e.g. per-project fees, premium features)
- **Subscriptions** (Starter, Professional, Enterprise plans)
- **Webhook processing** via `/api/stripe/webhook`

Configure Stripe keys and set up a webhook endpoint pointing to your deployment URL.

---

## Product Backlog (PBIs)

### Completed (MVP)
- [x] PBI-01: User registration and authentication (Client & Supplier roles)
- [x] PBI-02: Client project management (CRUD)
- [x] PBI-03: Plan upload with PDF→PNG conversion
- [x] PBI-04: AI plan analysis (processes, materials, cost estimates)
- [x] PBI-05: AI-driven supplier matching with affinity scores
- [x] PBI-06: Match lifecycle management (accept/reject by both parties)
- [x] PBI-07: Real-time SignalR notifications
- [x] PBI-08: Supplier profile with production capabilities
- [x] PBI-09: Stripe payment integration
- [x] PBI-10: Docker deployment with PostgreSQL

### Next Sprint
- [ ] PBI-11: Supplier capability management UI (add/edit machines)
- [ ] PBI-12: Admin panel (user management, trigger matching runs)
- [ ] PBI-13: Email notifications (MailKit integration)
- [ ] PBI-14: Project/supplier search with filters
- [ ] PBI-15: Subscription management UI (upgrade / cancel)
- [ ] PBI-16: Stripe webhook handler for subscription events
- [ ] PBI-17: Full-text search with pg_trgm
- [ ] PBI-18: AI re-analysis trigger on plan update
- [ ] PBI-19: Password reset flow
- [ ] PBI-20: Two-factor authentication (TOTP)

### Backlog
- [ ] PBI-21: Azure Blob Storage adapter for production file storage
- [ ] PBI-22: Export project to PDF quote request
- [ ] PBI-23: In-app messaging between client and supplier
- [ ] PBI-24: Rating and review system post-project
- [ ] PBI-25: Analytics dashboard for admins
- [ ] PBI-26: Multi-language support (i18n)

---

## License

Proprietary – All rights reserved.
