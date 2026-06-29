# AID Dashboard — Architecture & Build Plan

> My proposed architecture and step-by-step build sequence. This is a plan for reference; details may be refined as we build.
> Last updated: 2026-06-29.

## 1. Solution layout (monorepo)

```
AID Dashboard/
├─ docs/                     # these reference documents
├─ src/
│  ├─ Api/                   # ASP.NET Core Web API (.NET 8)
│  │  ├─ Controllers/        # thin controllers
│  │  ├─ Auth/               # IAuthProvider abstraction (Local now, Entra later)
│  │  └─ Middleware/         # global exception handler, etc.
│  ├─ Application/           # services, DTOs, validation, mapping
│  ├─ Domain/                # entities (Account, ReferenceItem, audit base, etc.)
│  ├─ Infrastructure/        # EF Core DbContext, repositories, migrations, import/export
│  └─ Web/                   # React + TS (Vite) frontend
│     ├─ components/         # reusable: EditableTable, FilterBar, RefSelect, ...
│     ├─ features/           # one folder per tab, composing shared components
│     ├─ api/                # typed API client
│     └─ theme/              # Stefanini theme tokens
└─ AidDashboard.sln
```

## 2. Backend architecture

- **Layers:** Domain ← Infrastructure / Application ← Api (clean-ish layering, kept pragmatic for a simple app).
- **EF Core** with SQLite provider; migrations checked in. Connection string + provider selected via config to enable the Azure SQL swap.
- **Auth abstraction:** `IAuthenticationProvider` with a `LocalCredentialsProvider` (username/password → JWT) today; an `EntraIdProvider` (OIDC) can be added without touching controllers/services.
- **Authorization:** policy-based — `AdminOnly` for writes/admin section, authenticated read for everyone.
- **Audit:** a base entity (`CreatedBy/At`, `ModifiedBy/At`) + an `AuditLog`/change-history table; populated in the DbContext `SaveChanges` override.
- **Cross-cutting:** global exception middleware → consistent error responses; request validation (FluentValidation or data annotations); structured logging.
- **Import/Export services:** XLSX/CSV/PDF export and XLSX/CSV import + template generation, scoped to the open account.

## 3. Frontend architecture

- **React + TypeScript + Vite.**
- **Reusable core:** a generic `EditableTable` built on **AG Grid Community** that takes column definitions + reference data + a data source, and provides: sorting, per-column filters, global search, edit mode (X-to-delete row, empty trailing add row), and export/import hooks. Every table tab is a thin configuration of this component.
- Shared form controls: `RefSelect` (single), `RefMultiSelect`, `YesNoToggle`, date pickers — driven by reference tables.
- **State/data:** typed API client; a data-fetching layer (e.g. React Query) for caching/error handling.
- **Theme:** Stefanini tokens (navy/gray/white + accent) in one place; responsive layout.

## 4. Data model (high level)

- `Account` (1) ──< per-account records for each tab (Overall fields, StaffingModel rows, ContractualLanguages rows, CountryDeviceRows, Automations, Opportunities, SupportHours, SlaKpi).
- **Reference tables** (Locations, Roles, Languages×2, Devices, Countries, ServiceTowers, OpportunityType/Status/RelatedService, SlaType, MeasurementType, Overall dropdown lists) — standalone, admin-managed, referenced by the per-account records.
- Audit base on all editable entities + change-history log.

## 5. Build sequence (phased)

1. **Scaffold** solution: .NET API + EF Core/SQLite + React/Vite/TS; wire App Service-friendly hosting (API serves built SPA).
2. **Auth & users:** local JWT login, `IAuthenticationProvider` abstraction, seed `admin1..admin5`, role-based authorization, forced password change.
3. **Reference tables + admin section:** entities, seed data (§5 of business doc), admin CRUD UI.
4. **Account shell:** account entity, account picker, "Add Account", tab navigation, edit-mode framework, audit plumbing.
5. **Reusable EditableTable** (AG Grid) + shared form controls.
6. **Tabs**, in order: Overall → Staffing Models → Contractual Languages → Countries/Users/Devices → Automations → Opportunities → Support Hours → SLAs & KPIs. (Services/FAQ/AID Dashboard = empty placeholders.)
7. **Import/Export:** XLSX/CSV/PDF export, XLSX/CSV import + templates, per open account.
8. **Hardening:** security pass (authz, validation, OWASP), global error handling, responsive polish, Stefanini theming.
9. **Azure-readiness:** config/secrets, SQLite on persisted storage, deployment notes.

## 6. Confirmed decisions (recap)
- Two **separate** language reference tables (Contractual vs Support Hours).
- **One shared** Devices reference table (incl. Servers, Switch, Storage device).
- Non-admins read-only; edit = admins only (for now).
- Accounts start empty; only reference tables seeded.
- Azure-ready, simplest (single App Service); no IaC/CI-CD for now.

## 7. Resolved decisions
1. **Theme colors live in one config/token file** (CSS variables); accent stays provisional red `#E4002B` for now.
2. **Admin login:** no forced change; temp password = username (`adminN`/`adminN`).
3. **Industry list:** seed with what we have; user completes it via admin section later.
