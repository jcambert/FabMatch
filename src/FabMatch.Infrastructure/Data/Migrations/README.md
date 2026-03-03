# EF Core Migrations

Migrations are managed via the `dotnet ef` CLI tool.

## Generate a new migration

From the repository root:

```bash
dotnet ef migrations add InitialCreate \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web \
    --output-dir Data/Migrations
```

## Apply migrations

```bash
dotnet ef database update \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web
```

## Revert the last migration

```bash
dotnet ef migrations remove \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web
```

> **Note**: Migrations run automatically on application startup via `MigrateAsync()` in `Program.cs`.
