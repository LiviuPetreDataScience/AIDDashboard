# AID Dashboard — Technical Specification

> Reference document formalizing the technical direction dictated for the AID Dashboard application.
> Last updated: 2026-06-29.

## 1. Stack summary

| Layer | Choice |
|-------|--------|
| Backend | **.NET 8 (LTS)**, ASP.NET Core **Web API** (REST) |
| ORM | **EF Core** (provider-agnostic) |
| Database | **SQLite** for now; designed to swap to **Azure SQL** later with minimal change |
| Frontend | **React + TypeScript** (Vite build) |
| Table/grid | **AG Grid (Community edition)** — sorting, per-column filtering, inline edit, export |
| Auth | Local username/password (**JWT**) now; **pluggable** for future **Entra ID / Active Directory (OIDC)** |
| Hosting | **Azure App Service** (single instance) serving both the API and the built React app |

## 2. Principles & quality bar

> The user's explicit instruction: *"I want this done professionally."*

- **Reusable components.** The frontend must be properly componentized — shared, reused components, **not** one bespoke component per page. A single generic editable-table component should drive every table tab, configured by column definitions + reference data.
- **Layered backend.** Clear separation: API/controllers → service layer → data/repository layer. Use DTOs and request validation; keep EF entities out of the API surface.
- **Provider-agnostic data.** No SQLite-specific SQL. All data access through EF Core so the database can be replaced (Azure SQL) by changing the provider + connection string.
- **Security** (the app must be protected against standard threats):
  - HTTPS only; secure JWT handling; hashed/salted passwords (e.g. ASP.NET Core Identity hasher).
  - Server-side authorization checks on every endpoint (read vs. edit; admin-only operations).
  - Input validation and output encoding; protection against the common OWASP issues (injection, XSS, CSRF as applicable, broken access control).
  - Secrets via configuration / Azure App Settings / Key Vault — never committed.
- **Robust error handling.** Global exception-handling middleware on the API; graceful try/catch where appropriate; structured logging. The app should fail gracefully and never crash on a handled error. (The app is simple, so this should be straightforward.)
- **Responsive UI** across screen sizes.
- **Scale:** low volume — does **not** need to handle high concurrency. This justifies SQLite + single App Service instance for now. (Note: SQLite means no horizontal scale-out; if scale is needed later, move to Azure SQL.)

## 3. Authentication & users

- **Now:** local login with username + password, issuing a JWT to the React SPA.
- **Initial users:** ~5 **placeholder admin accounts** seeded — `admin1`, `admin2`, … `admin5`. These are the only users at launch. Default login approach: a preset temporary password with **forced change on first login** (to be confirmed).
- **Future:** Active Directory / **Microsoft Entra ID** integration. The auth layer must be abstracted (an authentication provider interface) so OIDC/Entra can be dropped in **without** rewriting the rest of the app.
- **Authorization:** Admin = full edit + user management + reference-table management; User = read-only.

## 4. Data & audit

- Per-account data model; reference tables shared across accounts.
- **Audit trail required:** every record tracks **created-by / created-at** and **modified-by / modified-at**, plus a **change history**. (Confirmed by user.)

## 5. Import / Export

- **Export:** XLSX, CSV, **PDF** (PDF is export-only).
- **Import:** XLSX, CSV, with a **downloadable template** per table. Import is always scoped to the **currently open account**.

## 6. Azure hosting

- **Goal: "Azure-ready, simplest."** Single **App Service** hosts the .NET API and serves the built React static assets. Single instance (consistent with SQLite).
- SQLite database file stored on the App Service persisted storage (`/home`) so it survives restarts/deploys.
- App settings / connection strings via Azure configuration (and Key Vault for secrets where appropriate).
- **No** Infrastructure-as-Code or CI/CD pipeline required for now — just make the app cleanly deployable. (Can be added later.)

## 7. Branding & styling (Stefanini)

Researched from stefanini.com and brand listings (2026-06-29):

| Role | Color | Hex |
|------|-------|-----|
| Primary (dark navy) — "Bunting" | navy | `#111D41` |
| Secondary (gray) — "Dove Gray" | gray | `#686868` |
| Background / surface | white | `#FFFFFF` |
| **Accent (buttons/links)** | red | **`#E4002B`** *(provisional — to confirm with user)* |

- **Logo (SVG):** `https://stefanini.com/wp-content/themes/stefanini/images/logo-stefanini.svg` (white variant: `…logo-stefanini-white.svg`).
- **Typography:** clean sans-serif; dark navy header/nav, generous white space, professional tech-consultancy aesthetic.
- The exact published palette is grayscale + dark navy; the **accent color is not publicly documented**, so the red above is a provisional default for interactive elements — **single open item to confirm/swap.**
- Guideline: follow the Stefanini look as direction, but modernize for UX (this is React, not SharePoint).

## 8. Working preferences (process)

- **Avoid PowerShell commands** — they trigger approval prompts and the user may be away. Prefer Python scripts, built-in file tooling, or Bash. Use PowerShell only when it is the only viable option.
- **Batch internet access up front** so the user can approve it in one sitting (already done for branding/research).

## 9. Resolved decisions (were open items)
1. **Accent color:** keep provisional red `#E4002B` for now (may change later). **All main/theme colors must live in a single config / theme-token file (CSS variables)** so they can be changed in one place — never hardcoded across stylesheets.
2. **Admin login:** **no** forced password change. Temp password = same as the username (`admin1`/`admin1`, … `admin5`/`admin5`).
3. **Industry of Company list:** use the values captured from the screenshot / web research; the user will complete the rest himself via the admin section once it exists.
