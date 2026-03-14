# ROADMAP FabMatch

*Dernière mise à jour : 2026-03-14*

---

## État d'avancement global

| Sprint | Thème | Statut |
|--------|-------|--------|
| Sprint 1 | Foundation (Domain, Infrastructure, Auth) | ✅ Terminé |
| Sprint 2 | Core Business (Projets, Plans, Matching, Paiements) | ✅ Terminé |
| Sprint 3 | Administration (A1–A9) | ✅ Terminé |
| Sprint 4 | Admin avancé + Sécurité | 🔲 En cours |
| Sprint 5 | Product Completion (UX, Onboarding, Profils) | 🔲 À venir |
| Sprint 6 | Production Readiness | 🔲 À venir |
| Sprint 7 | Features avancées | 🔲 À venir |

---

## Sprint 3 — Administration ✅ COMPLÉTÉ

Tous les PBIs A1–A9 sont livrés.

| PBI | Titre | Statut |
|-----|-------|--------|
| PBI-A1 | Nav admin isolée + redirection `/admin` | ✅ |
| PBI-A2 | Tableau de bord KPIs (8 cartes + MRR) | ✅ |
| PBI-A3 | Gestion utilisateurs (lock/unlock, changement tier) | ✅ |
| PBI-A4 | Gestion clients (liste + filtre + actions) | ✅ |
| PBI-A5 | Gestion fournisseurs (badge embedding + actions) | ✅ |
| PBI-A6 | Modération capacités (liste plate + suppression) | ✅ |
| PBI-A7 | Dashboard abonnements / MRR | ✅ |
| PBI-A8 | Contrôle IA & Matching (trigger global, Top-N, seuil) | ✅ |
| PBI-A9 | Audit log admin (journal horodaté, filtrable) | ✅ |

---

## Sprint 4 — Admin avancé + Sécurité 🔲

### PBI-A10 : Impersonate user ❌

**User Story :**
> En tant qu'admin, je veux me connecter temporairement en tant qu'un utilisateur pour déboguer son expérience, puis revenir à mon compte admin en un clic.

**Critères d'acceptation :**
- [ ] Bouton "Impersonate" sur chaque ligne Utilisateurs / Clients / Fournisseurs
- [ ] Route POST `/admin/impersonate/{userId}` (Minimal API)
- [ ] Claim `OriginalAdminId` stocké dans la session impersonnée
- [ ] Bannière rouge persistante "Mode impersonation — Retour admin" dans `MainLayout`
- [ ] Route GET `/admin/stop-impersonate` pour revenir au compte admin
- [ ] Action loggée dans l'audit log (PBI-A9) → déjà livré
- [ ] Interdit si la cible est Admin (guard + message d'erreur)
- [ ] Tests unitaires : guard "Admin ne peut pas impersonate Admin"

**Fichiers à créer/modifier :**
- `FabMatch.Web/Endpoints/ImpersonateEndpoints.cs` (Minimal API)
- `FabMatch.Web/Components/Layout/MainLayout.razor` (bannière)
- `FabMatch.Web/Components/Pages/Admin/AdminPanel.razor` (bouton Impersonate)
- `FabMatch.Application/Features/Admin/Commands/ImpersonateUser/`

---

### PBI-20 : Two-factor authentication (TOTP) ❌

**User Story :**
> En tant qu'utilisateur, je veux activer l'authentification à deux facteurs avec une app TOTP (Google Authenticator, Authy) pour sécuriser mon compte.

**Critères d'acceptation :**
- [ ] Page de gestion 2FA dans les paramètres compte
- [ ] Génération QR code via `QRCoder` (déjà référencé)
- [ ] Validation du code TOTP lors du login
- [ ] Codes de récupération d'urgence (10 codes à usage unique)
- [ ] Désactivation 2FA (confirmation mot de passe requise)
- [ ] Indicateur 2FA actif dans le profil utilisateur

**Fichiers à créer/modifier :**
- `FabMatch.Web/Components/Pages/Auth/TwoFactorSetup.razor`
- `FabMatch.Web/Components/Pages/Auth/TwoFactorLogin.razor`
- `FabMatch.Web/Endpoints/AuthEndpoints.cs` (étape TOTP post-login)
- Modification du flow login dans `Program.cs`

---

### PBI-17 : Full-text search pg_trgm ❌

**User Story :**
> En tant que client, je veux rechercher des fournisseurs avec un texte libre (ex. "laser inox Bretagne") et obtenir des résultats pertinents même avec des fautes de frappe.
> En tant qu'admin, je veux rechercher dans les projets et utilisateurs avec tolérance aux fautes.

**Critères d'acceptation :**
- [ ] Migration EF Core : activation extension `pg_trgm` + index GIN sur colonnes ciblées
- [ ] `SearchSuppliersHandler` : recherche full-text sur `CompanyName`, `Materials`, `Certifications`, `ProcessType`
- [ ] `GetAllUsersHandler` : recherche full-text sur `Email`, `FullName`
- [ ] Score de similarité retourné et utilisé pour le tri des résultats
- [ ] Tests d'intégration PostgreSQL pour valider les résultats

**Fichiers à modifier :**
- Migration EF Core : activation `pg_trgm` + index GIN
- `SearchSuppliersHandler.cs` : requête LIKE → trigram similarity
- `GetAllUsersHandler.cs` : idem

---

## Sprint 5 — Product Completion 🔲

### PBI-B1 : Onboarding Client ❌

**User Story :**
> En tant que nouveau client, après mon inscription, je veux être guidé pour créer mon profil d'entreprise avant d'accéder au dashboard.

**Critères d'acceptation :**
- [ ] Détection `IsOnboarded = false` → redirection vers `/onboarding/client`
- [ ] Wizard multi-étapes : Informations entreprise → Secteur d'activité → Confirmation
- [ ] Création de l'entité `Client` lors de la validation
- [ ] `IsOnboarded` mis à `true` après completion
- [ ] Impossibilité d'accéder au dashboard tant que non onboardé

**Fichiers à créer :**
- `FabMatch.Web/Components/Pages/Onboarding/ClientOnboarding.razor`
- `FabMatch.Application/Features/Clients/Commands/CompleteClientOnboarding/`

---

### PBI-B2 : Onboarding Supplier ❌

**User Story :**
> En tant que nouveau fournisseur, après mon inscription, je veux être guidé pour renseigner mes capacités de production avant d'être visible dans le moteur de matching.

**Critères d'acceptation :**
- [ ] Détection `IsOnboarded = false` → redirection vers `/onboarding/supplier`
- [ ] Wizard : Informations entreprise → Adresse → Certifications → Capacités (min 1) → Confirmation
- [ ] Création des entités `Supplier` + au moins une `ProductionCapability`
- [ ] Déclenchement génération embedding IA après onboarding
- [ ] `IsOnboarded` mis à `true` après completion

**Fichiers à créer :**
- `FabMatch.Web/Components/Pages/Onboarding/SupplierOnboarding.razor`
- `FabMatch.Application/Features/Suppliers/Commands/CompleteSupplierOnboarding/`

---

### PBI-B3 : Édition de profil utilisateur ❌

**User Story :**
> En tant qu'utilisateur (client ou fournisseur), je veux pouvoir modifier mes informations personnelles et changer mon mot de passe depuis une page dédiée.

**Critères d'acceptation :**
- [ ] Page `/account/profile` accessible à tous les rôles
- [ ] Modification : prénom, nom, email (avec confirmation)
- [ ] Changement de mot de passe (ancien + nouveau + confirmation)
- [ ] Upload d'avatar (facultatif)
- [ ] Message de confirmation après sauvegarde

**Fichiers à créer :**
- `FabMatch.Web/Components/Pages/Account/ProfileSettings.razor`
- `FabMatch.Application/Features/Auth/Commands/UpdateProfile/`
- `FabMatch.Application/Features/Auth/Commands/ChangePassword/`

---

### PBI-B4 : Édition profil Client ❌

**User Story :**
> En tant que client, je veux pouvoir modifier les informations de mon entreprise (nom, secteur, description, taille) depuis mon espace.

**Critères d'acceptation :**
- [ ] Page `/client/profile` ou onglet dans les paramètres compte
- [ ] Modification : `CompanyName`, `Industry`, `Description`, `CompanySize`, `Address`
- [ ] Validation : CompanyName non vide

**Fichiers à créer :**
- `FabMatch.Application/Features/Clients/Commands/UpdateClientProfile/`
- Modification ou création `ClientProfile.razor`

---

### PBI-B5 : Édition profil Supplier ❌

**User Story :**
> En tant que fournisseur, je veux mettre à jour les informations de mon entreprise et voir l'impact sur ma visibilité dans le moteur de matching.

**Critères d'acceptation :**
- [ ] Modification : `CompanyName`, `Country`, `Address`, `Materials`, `Certifications`, `Description`, `LeadTimeDays`, `MinOrderValue`
- [ ] Regénération automatique de l'embedding IA après mise à jour significative
- [ ] Indicateur "Embedding à jour / en cours de régénération"
- [ ] Validation : CompanyName non vide, LeadTimeDays ≥ 1

**Fichiers à modifier :**
- `SupplierProfile.razor` (ajout formulaire d'édition)
- `FabMatch.Application/Features/Suppliers/Commands/UpdateSupplierProfile/`

---

### PBI-B6 : Workflow complet Match client/fournisseur ❌

**User Story :**
> En tant que client, je veux accepter ou rejeter une proposition de match et initier une demande de devis auprès du fournisseur sélectionné.
> En tant que fournisseur, je veux voir les matches qui me concernent et signifier mon intérêt ou mon refus.

**Critères d'acceptation :**
- [ ] Client : boutons Accepter / Rejeter sur `ClientMatchDashboard`
- [ ] Fournisseur : boutons Intéressé / Passer sur `SupplierMatchDashboard`
- [ ] Statut `Match` mis à jour (`AcceptedByClient`, `AcceptedBySupplier`, `Finalised`)
- [ ] Notification temps réel (SignalR) à l'autre partie lors de changement de statut
- [ ] Email envoyé à chaque transition de statut (MailKit)
- [ ] État `Finalised` atteint quand les deux parties ont accepté

**Fichiers à modifier :**
- `ClientMatchDashboard.razor`
- `SupplierMatchDashboard.razor`
- `UpdateMatchStatusHandler.cs` (logique Finalised + notifications)

---

### PBI-B7 : Pages d'erreur (404, 500) ❌

**User Story :**
> En tant qu'utilisateur, si je navigue vers une page inexistante ou si une erreur survient, je veux voir une page claire avec un lien de retour.

**Critères d'acceptation :**
- [ ] Page 404 custom avec lien vers Dashboard
- [ ] Page 500 custom avec bouton Réessayer
- [ ] Intégration dans `App.razor` / `Routes.razor`
- [ ] Design cohérent MudBlazor

**Fichiers à créer :**
- `FabMatch.Web/Components/Pages/Errors/NotFound.razor`
- `FabMatch.Web/Components/Pages/Errors/ServerError.razor`

---

## Sprint 6 — Production Readiness 🔲

### PBI-C1 : Templates d'emails transactionnels ❌

**User Story :**
> En tant qu'utilisateur, je veux recevoir des emails HTML bien formatés pour les événements importants (bienvenue, nouveau match, paiement, reset password).

**Critères d'acceptation :**
- [ ] Templates HTML pour : Bienvenue, Nouveau match, Match accepté/rejeté, Paiement, Reset password
- [ ] Service `IEmailTemplateService` avec rendu Razor ou Scriban
- [ ] Branding FabMatch dans les templates
- [ ] Tests d'envoi en staging

**Fichiers à créer :**
- `FabMatch.Infrastructure/Templates/Email/*.html`
- `FabMatch.Infrastructure/Services/EmailTemplateService.cs`

---

### PBI-C2 : Stockage fichiers cloud ❌

**User Story :**
> En tant qu'exploitant, je veux que les plans techniques uploadés soient stockés sur un service cloud (Azure Blob / S3) et non sur le disque local du serveur.

**Critères d'acceptation :**
- [ ] Implémentation `AzureBlobStorageService` ou `S3FileStorageService` de `IFileStorageService`
- [ ] Sélection via config (`FileStorage:Provider = Local|AzureBlob|S3`)
- [ ] SAS tokens ou URLs pré-signées pour les téléchargements

**Fichiers à créer :**
- `FabMatch.Infrastructure/Services/AzureBlobStorageService.cs`

---

### PBI-C3 : Webhook Stripe complet ❌

**User Story :**
> En tant que système, je veux traiter de manière fiable tous les événements Stripe pour maintenir la synchronisation des tiers utilisateurs.

**Critères d'acceptation :**
- [ ] Handler `/webhooks/stripe` (Minimal API, vérification signature HMAC)
- [ ] Événements traités : `payment_intent.succeeded`, `payment_intent.payment_failed`, `customer.subscription.deleted`, `invoice.payment_failed`
- [ ] Mise à jour automatique du `SubscriptionTier` de l'utilisateur
- [ ] Notification + email à l'utilisateur concerné
- [ ] Idempotence : pas de double traitement

**Fichiers à créer/modifier :**
- `FabMatch.Web/Endpoints/StripeWebhookEndpoints.cs`
- `FabMatch.Application/Features/Payments/Commands/HandleStripeWebhook/`

---

### PBI-C4 : Observabilité (logs structurés + métriques) ❌

**User Story :**
> En tant qu'exploitant, je veux des logs structurés (JSON) et des métriques applicatives pour monitorer la plateforme en production.

**Critères d'acceptation :**
- [ ] Intégration Serilog (JSON sink + console sink)
- [ ] Corrélation request avec `TraceId` dans les logs
- [ ] Métriques : latence handlers CQRS, taux d'erreur, appels IA
- [ ] Health check endpoint `/health` (DB, AI, Stripe)

---

### PBI-C5 : Tests coverage ❌

**User Story :**
> En tant que développeur, je veux une couverture de tests suffisante pour livrer en confiance.

**Critères d'acceptation :**
- [ ] Couverture ≥ 70% sur `FabMatch.Application`
- [ ] Tests unitaires pour tous les handlers non testés
- [ ] Tests d'intégration Testcontainers pour les repos EF Core critiques
- [ ] Tests bout en bout : Register → Onboarding → Create Project → Match

**Handlers non testés à couvrir :**
- `SetUserLockoutHandler`, `UpdateUserTierHandler`, `UpdateSubscriptionPlanHandler`
- `UploadPlanHandler`, `ReanalysePlanHandler`
- `CreateSubscriptionHandler`, `CancelSubscriptionHandler`
- `GetNotificationsHandler`, `MarkAllNotificationsReadHandler`

---

## Sprint 7 — Features avancées 🔲

### PBI-D1 : Messagerie client/fournisseur ❌

**User Story :**
> En tant que client, une fois un match finalisé, je veux pouvoir échanger des messages directement avec le fournisseur dans la plateforme.

**Critères d'acceptation :**
- [ ] Entité `Message` (Id, SenderId, ReceiverId, MatchId, Content, CreatedAt, IsRead)
- [ ] Interface de chat intégrée dans la page de détail Match
- [ ] Messages temps réel via SignalR
- [ ] Notification (cloche + email) à réception d'un nouveau message
- [ ] Historique paginé

---

### PBI-D2 : Export rapport PDF ❌

**User Story :**
> En tant que client, je veux exporter un rapport PDF de mes projets et matches pour le partager en interne.

**Critères d'acceptation :**
- [ ] Rapport : résumé projet, analyses IA, liste matches avec scores affinité
- [ ] Génération via `QuestPDF` ou `Playwright` headless
- [ ] Téléchargement direct depuis la page projet
- [ ] Soumis aux limites de tier (ex. Enterprise uniquement)

---

### PBI-D3 : Tableau de bord analytique fournisseur ❌

**User Story :**
> En tant que fournisseur, je veux voir des métriques sur ma visibilité et mes performances (matches reçus, taux d'acceptation, projets gagnés).

**Critères d'acceptation :**
- [ ] Métriques : matches reçus (30j), taux d'acceptation, projets finalisés
- [ ] Graphique d'évolution (MudBlazor Charts)
- [ ] Comparaison vs moyenne plateforme (anonymisée)

---

### PBI-D4 : Multi-langue (i18n) ❌

**User Story :**
> En tant qu'utilisateur francophone ou anglophone, je veux naviguer dans la plateforme dans ma langue.

**Critères d'acceptation :**
- [ ] Support FR + EN
- [ ] Sélecteur de langue dans la nav
- [ ] Ressources `.resx` pour toutes les chaînes UI
- [ ] Persistance du choix de langue (cookie)

---

## Récapitulatif par priorité

### Sprint 4 — Admin avancé + Sécurité
| PBI | Titre | Effort |
|-----|-------|--------|
| PBI-A10 | Impersonate user | M |
| PBI-20 | Two-factor auth (TOTP) | L |
| PBI-17 | Full-text search pg_trgm | S |

### Sprint 5 — Product Completion
| PBI | Titre | Effort |
|-----|-------|--------|
| PBI-B1 | Onboarding Client | M |
| PBI-B2 | Onboarding Supplier | M |
| PBI-B3 | Édition profil utilisateur | S |
| PBI-B4 | Édition profil Client | S |
| PBI-B5 | Édition profil Supplier | S |
| PBI-B6 | Workflow Match complet | M |
| PBI-B7 | Pages d'erreur | XS |

### Sprint 6 — Production Readiness
| PBI | Titre | Effort |
|-----|-------|--------|
| PBI-C1 | Templates emails HTML | M |
| PBI-C2 | Stockage cloud | M |
| PBI-C3 | Webhook Stripe complet | M |
| PBI-C4 | Observabilité | L |
| PBI-C5 | Tests coverage | L |

### Sprint 7 — Features avancées
| PBI | Titre | Effort |
|-----|-------|--------|
| PBI-D1 | Messagerie client/fournisseur | XL |
| PBI-D2 | Export rapport PDF | M |
| PBI-D3 | Analytics fournisseur | M |
| PBI-D4 | Multi-langue | XL |

*Légende effort : XS < 2h · S < 4h · M < 1j · L < 3j · XL > 3j*

---

## Architecture — Rappel

```
FabMatch.Domain         # Entités, enums, value objects
FabMatch.Application    # CQRS handlers, interfaces, validators
FabMatch.Infrastructure # EF Core, AI, Stripe, MailKit, SignalR
FabMatch.Web            # Blazor Server + MudBlazor
tests/FabMatch.Tests    # xUnit, Moq, FluentAssertions, Testcontainers
```

**Stack :** .NET 9 · Blazor Server · MudBlazor · PostgreSQL · EF Core · Mediator (source gen) · OpenAI · Stripe · SignalR · MailKit
