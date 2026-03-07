# CLAUDE.md — FabMatch

Fichier de contexte pour Claude Code. Lit ce fichier avant toute intervention sur le projet.

---

## Projet

**FabMatch** est un SaaS B2B de mise en relation entre acheteurs industriels (clients) et sous-traitants en tôlerie/usinage CNC, via un moteur de matching piloté par IA.

- **Clients** : déposent des projets avec plans techniques → l'IA analyse et propose les fournisseurs les plus pertinents.
- **Fournisseurs** : renseignent leurs capacités de production → un embedding vectoriel est généré automatiquement.
- **Admin** : gère la plateforme depuis un espace dédié, **entièrement séparé** de l'espace client/fournisseur.

---

## Commandes courantes

```bash
# Lancer en développement (PostgreSQL via Docker)
docker compose up db -d
cd src/FabMatch.Web && dotnet run

# Tests
dotnet test
dotnet test --filter "Category!=Integration"   # unitaires uniquement
dotnet test --filter "Category=Integration"    # nécessite Docker

# Build
dotnet build

# Migrations EF Core
dotnet ef migrations add <Nom> \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web \
    --output-dir Data/Migrations

dotnet ef database update \
    --project src/FabMatch.Infrastructure \
    --startup-project src/FabMatch.Web

# Stack complète
docker compose up -d
```

---

## Architecture

Clean Architecture, dépendances strictement vers l'intérieur : `Domain` ← `Application` ← `Infrastructure` ← `Web`

```
FabMatch.Domain         # Entités, enums, value objects, interfaces domaine — zéro dépendance externe
FabMatch.Application    # Handlers CQRS, interfaces applicatives, behaviors (logging, validation)
FabMatch.Infrastructure # EF Core, AI, Stripe, MailKit, SignalR, stockage fichiers
FabMatch.Web            # Blazor Server + MudBlazor, endpoints auth, SignalR hub
tests/FabMatch.Tests    # xUnit, Moq, FluentAssertions, Testcontainers
```

### CQRS

Utilise [martinothamar/Mediator](https://github.com/martinothamar/Mediator) — **source generator, zéro réflexion** (pas MediatR).

Convention dans `Application/Features/<Domain>/` :
- `Commands/<Nom>/<Nom>Command.cs` — record command/query
- `Commands/<Nom>/<Nom>Handler.cs` — `ICommandHandler<,>` ou `IQueryHandler<,>`
- `Commands/<Nom>/<Nom>Validator.cs` — FluentValidation (auto-découverte via pipeline)

Pipeline : `LoggingBehavior` → `ValidationBehavior` → handler.

### Entités domaine

`ApplicationUser` (Identity), `Client`, `Supplier`, `Project`, `Plan`, `Analysis`, `Match`, `Notification`, `Payment`, `ProductionCapability`

Toutes les entités utilisent des PKs `Guid` et le soft-delete via `IsDeleted` (global query filters EF).

### Interfaces applicatives clés (`Application/Common/Interfaces/`)

| Interface | Implémentation Infrastructure |
|---|---|
| `IAIService` | `OpenAIService` ou `OllamaAIService` (config-driven) |
| `IPaymentService` | `StripePaymentService` |
| `IEmailService` | `MailKitEmailService` |
| `IFileStorageService` | `LocalFileStorageService` |
| `IPdfConverterService` | `PdfConverterService` (PDFtoImage/Pdfium) |
| `INotificationHubService` | `NotificationHubService` (SignalR) |
| `ITierPolicyService` | `TierPolicyService` (limites abonnement) |
| `IUnitOfWork` | `UnitOfWork` (wraps `ApplicationDbContext`) |

### Authentification

Cookie-based via ASP.NET Core Identity. Login POST vers `/auth/login/execute` (Minimal API dans `Program.cs`). Le formulaire utilise un pattern double-input checkbox (`hidden value="false"` + `checkbox value="true"`), donc `rememberMe` est bindé en `string?` parsé avec `.Contains("true")`.

Rôles : `Admin`, `Client`, `Supplier`. Policies : `AdminOnly`, `ClientOnly`, `SupplierOnly`.

Le compte admin est seedé au démarrage depuis `AdminSeed:Email` / `AdminSeed:Password`.

---

## Exigences fondamentales (non négociables)

### 1. L'admin n'est PAS un utilisateur ordinaire

- **Pas de page Billing** pour l'admin
- **Pas de Dashboard** client/fournisseur
- **Pas de cloche Notifications** dans la barre de navigation
- **Redirection automatique** vers `/admin` au login (clients/fournisseurs → `/dashboard`)
- **Navigation dédiée** : Tableau de bord · Utilisateurs · Clients · Fournisseurs · Capacités · Abonnements · IA & Matching
- **Chip "Admin"** affiché dans la barre principale

### 2. Séparation stricte des rôles dans la nav

- `AuthorizeView Roles="Admin"` → nav admin uniquement
- `AuthorizeView Roles="Client,Supplier"` → nav utilisateur (Billing, Notifications inclus)
- Un rôle ne voit jamais les éléments de l'autre

### 3. Sécurité des routes admin

- Toutes les pages sous `/admin/**` portent `[Authorize(Roles = "Admin")]`

---

## Configuration

| Clé | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | Chaîne de connexion PostgreSQL |
| `AI:Provider` | `OpenAI` (défaut) ou `Ollama` |
| `AI:OpenAI:ApiKey` | Clé API OpenAI |
| `AI:OpenAI:ChatModel` | Défaut : `gpt-4o` |
| `AI:OpenAI:EmbeddingModel` | Défaut : `text-embedding-3-small` |
| `AI:Ollama:BaseUrl` | Défaut : `http://localhost:11434` |
| `Payment:Stripe:SecretKey` | Clé secrète Stripe |
| `Payment:Stripe:WebhookSecret` | Webhook signing secret Stripe |
| `FileStorage:BasePath` | Répertoire des uploads |
| `Smtp:Host/Port/User/Password` | Config MailKit SMTP |
| `AdminSeed:Email` | Compte admin seedé au premier démarrage |
| `AdminSeed:Password` | Mot de passe admin (min 8 chars, chiffre, majuscule) |

Variables d'environnement : remplacer `:` par `__` (ex. `AI__OpenAI__ApiKey`).

---

## Backlog Sprint 3 — Administration

### PBIs complétés ✅

| PBI | Description | Fichiers clés |
|-----|-------------|---------------|
| PBI-A1 | Nav admin isolée + redirection `/admin` au login + suppression Billing/Notifs pour admin | `NavMenu.razor`, `MainLayout.razor`, `Home.razor` |
| PBI-A2 | Tableau de bord KPIs (8 stat cards + répartition tier + MRR) | `GetPlatformStatsHandler.cs`, `AdminPanel.razor` onglet 0 |
| PBI-A3 | Gestion utilisateurs (tableau + lock/unlock + changement tier) | `GetAllUsersHandler.cs`, `SetUserLockoutHandler.cs`, `UpdateUserTierHandler.cs` |
| PBI-A4 | Gestion clients (liste + filtre + actions) | `GetAllClientsAdminHandler.cs`, `AdminPanel.razor` onglet 2 |
| PBI-A5 | Gestion fournisseurs (liste + badge embedding + actions) | `GetAllSuppliersAdminHandler.cs`, `AdminPanel.razor` onglet 3 |
| PBI-A6 | Modération capacités (liste plate + suppression) | `DeleteCapabilityCommand`, `AdminPanel.razor` onglet 4 |
| PBI-A7 | Dashboard abonnements / MRR (cartes par plan + table limites) | `AdminPanel.razor` onglet 5 |
| PBI-A8 | Contrôle IA & Matching (trigger global, Top-N, seuil affinité) | `TriggerGlobalMatchingHandler.cs`, `AdminPanel.razor` onglet 6 |

### PBIs à faire ❌

#### PBI-A9 : Audit log admin

**User Story** : En tant qu'admin, je veux consulter un journal horodaté de toutes mes actions (lock, changement plan, suppression capacité) dans un onglet dédié du panel.

**Critères d'acceptation :**
- Entité `AdminAuditLog` (Id, AdminUserId, Action, TargetType, TargetId, TargetLabel, CreatedAt)
- Enregistrement automatique dans `SetUserLockoutHandler`, `UpdateUserTierHandler`, `DeleteCapabilityHandler`
- Onglet "Audit" dans `AdminPanel.razor` (onglet index 7, tableau horodaté, filtre par type d'action)
- Migration EF Core

**Fichiers à créer/modifier :**
- `FabMatch.Domain/Entities/AdminAuditLog.cs`
- `FabMatch.Infrastructure/Data/Configurations/AdminAuditLogConfiguration.cs`
- Migration EF Core
- `FabMatch.Application/Features/Admin/Queries/GetAuditLogs/` (query + handler + DTO)
- `FabMatch.Application/Common/Interfaces/IAuditLogger.cs` + implémentation Infrastructure
- Modifier `SetUserLockoutHandler`, `UpdateUserTierHandler`, `DeleteCapabilityHandler`
- `AdminPanel.razor` : ajouter onglet index 7

---

#### PBI-A10 : Impersonate user

**User Story** : En tant qu'admin, je veux me connecter temporairement en tant qu'un utilisateur pour déboguer son expérience, puis revenir à mon compte admin en un clic.

**Critères d'acceptation :**
- Bouton "Impersonate" sur chaque ligne Utilisateurs/Clients/Fournisseurs
- Claim `OriginalAdminId` stocké dans la session impersonnée
- Bannière rouge persistante "Mode impersonation — Retour admin" dans `MainLayout`
- Route `/admin/impersonate/{userId}` (POST) + `/admin/stop-impersonate` (GET)
- Action loggée dans l'audit log (PBI-A9 requis)
- Interdit si la cible est aussi Admin

---

#### PBI-17 : Full-text search pg_trgm

Index GIN pg_trgm déjà créés au démarrage. Implémenter la recherche full-text fournisseurs/projets.

#### PBI-20 : Two-factor authentication

TOTP avec QR code. Package QRCoder déjà référencé.

---

## Bugs corrigés / pièges connus

### Règle d'or Blazor Server : pas de `Task.WhenAll` sur le DbContext

En Blazor Server, **1 DbContext scopé par circuit SignalR**. Deux handlers qui s'exécutent en parallèle via `Task.WhenAll` partagent le même contexte → `InvalidOperationException: second operation started`.

**Toujours** `await` séquentiels dans les composants Blazor :

```csharp
// ❌ INTERDIT
await Task.WhenAll(LoadUsersAsync(), LoadStatsAsync());

// ✅ CORRECT
await LoadStatsAsync();
await LoadUsersAsync();
```

### Règle UserManager : pas de `GetRolesAsync` en boucle

```csharp
// ❌ N+1 + risque concurrence
foreach (var user in users)
    var roles = await _userManager.GetRolesAsync(user);

// ✅ 2 requêtes upfront, HashSet lookup O(1)
var clientIds   = (await _userManager.GetUsersInRoleAsync("Client")).Select(u => u.Id).ToHashSet();
var supplierIds = (await _userManager.GetUsersInRoleAsync("Supplier")).Select(u => u.Id).ToHashSet();
```

### Toujours async en EF Core

```csharp
// ❌
var count = _userManager.Users.Count();

// ✅
var count = await _userManager.Users.CountAsync(ct);
```

---

## Points d'attention MudBlazor / Blazor

1. `MudChip` nécessite `T="string"` depuis MudBlazor 7.
2. `AuthorizeView` imbriqués : toujours nommer le `Context` différemment pour éviter le shadowing.
3. `AdminPanel.razor` a **7 onglets** (index 0–6). PBI-A9 ajoute l'onglet **index 7**. Ne pas décaler les index existants.
4. Les handlers Application qui ont besoin de joindre les tables Identity (UserRoles, Roles) doivent passer par une interface applicative, pas injecter `ApplicationDbContext` directement (Clean Architecture).
