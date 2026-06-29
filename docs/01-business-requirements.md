# AID Dashboard — Business Requirements

> Reference document formalizing the business requirements dictated for the AID Dashboard application.
> Source: `instructions.docx` and the screenshots in `print screens/`. Last updated: 2026-06-29.

## 1. Overview

AID Dashboard is a web application that replaces an existing **SharePoint hand-entry app**. It records structured delivery information about service **Accounts**. All data is entered manually by users; there is no automated data ingestion.

The central entity is the **Account**. Everything in the app is recorded **per Account**. The app provides an account picker at the top, and an "Add Account" capability. Each account holds all the information described in the tabs below.

> **Terminology note:** the screenshots show an account selector labelled "Project B" — this was a naming mistake. The entity is always an **Account** (e.g. "Account B"). The UI must use "Account" throughout.

## 2. Navigation / Tabs

Each tab is a section or table scoped to the currently selected account.

| # | Tab | Type | Status |
|---|-----|------|--------|
| 1 | Overall | Form (grouped fields) | In scope |
| 2 | Initial Staffing Model | Table | In scope |
| 3 | Latest Approved Model | Table (same layout as #2) | In scope |
| 4 | Contractual Languages | Table | In scope |
| 5 | Countries / Users / Devices Supported | Table | In scope |
| 6 | Automations | Table | In scope |
| 7 | Services | Table | **Empty placeholder** (deferred — more complex, to be done later) |
| 8 | Opportunities | Table | In scope |
| 9 | Support Hours | Table | In scope |
| 10 | SLAs & KPIs | Table | In scope |
| 11 | FAQ | — | **Empty placeholder** |
| 12 | AID Dashboard | — | **Empty placeholder** |

## 3. Cross-cutting rules

- **Seed data:** accounts start **empty**. Admins enter the first rows by hand. Only **reference tables** are pre-populated (see §5).
- **Edit mode:** each table/form has an **Edit** toggle. Clicking it makes fields editable (with a clear visual cue). A **Save/Close** action ends edit mode. No data is editable outside edit mode.
- **Row add/delete (tables):** there are **no** separate "Add row"/"Delete row" buttons. In edit mode:
  - an **X** appears on each row to delete it;
  - an **empty row** at the bottom lets the user add a new entry.
- **Standard table behaviour:** every table supports column **sorting** (click header), a **per-column filter** field, and an optional **global search** box outside the table.
- **Export:** every table is exportable to **XLSX, CSV, and PDF**.
- **Import:** every table supports import from **XLSX and CSV**, with a **downloadable template**. Import always targets the **currently open account only** — multi-account imports are not allowed.
- **Responsive:** the UI must work across screen sizes.
- **Styling:** styled per Stefanini brand (see technical spec), modernized for good UX — it does **not** need to copy the SharePoint look exactly.

## 4. Roles & access

- **Roles:** Admin and User. No self-registration — admins create all accounts/users.
- **Admins:** full edit rights on all accounts and all tabs; manage users and roles; manage reference tables (admin section).
- **Users (non-admin):** **read-only** view of all accounts.
- *(Deferred)* The original intent that "anyone named in the Overall top section can edit their account" is **postponed**, because those are free-text name fields with no link to user accounts. For now, **edit = admins only**.

## 5. Reference tables (pre-populated)

These are maintained by admins in the admin section and reused across tabs.

### 5.1 Locations
Rows for Staffing Models (#2, #3) and Contractual Languages (#4):
Romania, Moldova, Poland, EMEA (Management and Specialists), Belgium, Portugal, Morroco, South Africa, Brasil, Argentina, Southfield/US, Philippines/Manila, China/Jilin, China/Dalian, Mexico, Other.

### 5.2 Staffing Roles
Columns for both Staffing Models:
Support Engineers, TL, SDM, Operation Specialists, Service Manager, AL, IMS, QS, KE, Technical Training Specialist, ForS, RTS, EUX, Problem Analyst, Cognitive Data Specialist, PPM Specialist (Proactive problem), Reporting Specialist, Change Coordinator, Change Manager, TSM, Business Analyst, Transition Manager, DAM/CRD.

### 5.3 Contractual Languages *(separate table from Support Hours languages)*
Columns: English, French, German, Polish, Spanish, Italian, Portughese, Dutch, Hungarian, Greek, Turkish, Russian, Romanian, Bulgarian, Croatian, Lithuanian, Macedonian, Serbian, Czech, Slovak, Slovenian, Norwegian, Danish, Swedish, Finish, Hebrew, Ukrainian, Hindi, Cantonese, Japanese, Mandarin, Korean, Kazakh, Tagalog, Thai.

### 5.4 Support Hours Languages *(separate, region-split list)*
Rows: EMEA English, NA English, EMEA French, NA French, German, Polish, EMEA Spanish, LATAM Spanish, Italian, EMEA Portughese, LATAM Portughese, Dutch, Hungarian, Greek, Turkish, Russian, Romanian, Bulgarian, Croatian, Lithuanian, Macedonian, Serbian, Czech, Slovak, Slovenian, Norwegian, Danish, Swedish, Finish, Hebrew, Ukrainian, Hindi, Cantonese, Japanese, Mandarin, Korean, Arabic, Kazakh, Tagalog, Thai, Urdu.

### 5.5 Devices *(single shared table — used by #5 columns and Overall "Devices supported")*
No of Users, No of LTS Staff, Laptops, MAC Books, Desktops, Engineering Workstations, Tablets, Smart Phone – Android, Smart Phone – Apple, Mobile Phones, Printers & Plotters, Scanners & Hand Scanners, Label Printers, Wifi/Router, DHS – Health related devices, Servers, Switch, Storage device.

### 5.6 Countries
Full public/ISO country list (used by tab #5 rows and the Overall "Headquarter (Country)" field).

### 5.7 Service Towers ("Delivered By" / "In Scope")
SD, RTS, LTS, LTS-Staffing, DHS, DWP &UC, MEM, ITAM, HCI &NW, CSS, SAP, TSMO, AMS, SMP.

### 5.8 Opportunity reference lists
- **Opportunity Type:** New service, Additional scope for existing service, Project based work.
- **Status:** Opportunity identified, Proposal Delivered, Ok from Client, Contract Signed, Implemented, On hold, Cancelled, Lost, Abandoned.
- **Related Service:** Sophie, Preventive Device Management, SAP Cloud Platforms, License Resell, IT Asset Management, Data Analytics, Service Desk, Programme or Project Mgmt, Local Technical Support.

### 5.9 SLA / KPI reference lists
- **Type:** SLA, KPI, HCM.
- **Measurement type:** High, Low.

### 5.10 Overall dropdown lists
- **Type of account:** ITO, ITO (SOS), DHS, BPO.
- **Connectivity (Country):** MPLS, VPN Tunnel, Stefanini VDI, Customer VDI.
- **Telecom:** EMEA, US, Client.
- **Managed by:** Stefanini, Client, 3rd party.
- **ITSM Tool:** SNOW, 4me, BMC Remedy, TopDesk, HPSM, Jira, HEAT, Salesforce, ControlSeries, Zendesk, Ivanti, ServiceDesk Plus, Matrix 42, ServiceAide, Cherwell, SMAX, SysAid, OTRS, FreshService.
- **Contract Type** (multiselect): Managed Services, Staffing, Managed&Staffing.
- **Billing (Pricing model)** (multiselect; same list reused for the base, *-LTS*, and *-Infra* fields): Price per tickets, Price per users, Price per workstation, Price per tickets per users ratio (2D), Price per tickets per user per FLR ratio, Other (details please), Price per FTE, Monthly fixed price, Price per intervention, Price per study, Price per CI (Configuration Item).
- **Technology supported** (multiselect): IBM, Toshiba, HP, Apple, Lenovo, Epson, Canon, Fujitsu, Brother, Dell, Zebra, Ricoh, Xerox, Kyocera, Microsoft, Konica Minolta, Samsung.
- **Industry of Company:** standard industry taxonomy (partial from screenshot — to be completed): Fishing, Horticulture, Tobacco, Wood, Aerospace, Automotive, Chemical-other, Chemical-Pharmaceutical, Construction/Engineering, Defense, Environmental, Electric power, Electronics-other, Electronics-Computer, Electronics-Semiconductor, Energy, Food, Industrial robot, Low technology, Meat packing, Mining, Petroleum, Pulp and paper, Steel, …

## 6. Tab specifications

### 6.1 Overall (form)
Grouped sections: **SG specifics**, **Client specifics**, **Technology**, **Other**. Field input types:
- **Single-select dropdowns:** Type of account, Flagship account (Yes/No), Connectivity (Country), Telecom, Managed by, ITSM Tool, Industry of Company, Headquarter (Country).
- **Multiselect:** Contract Type, Billing (Pricing model) + *-LTS* + *-Infra*, In Scope (service towers), Technology supported, Devices supported.
- **Yes/No:** Hardware: Client hardware (Y/N), Work from home approved by client, Sharing constraints (contractual point of view).
- **Free text:** people/role-name fields (Global SM, Global SDM, EMEA/NA/APAC SDMs, EMEA SM, TSM, BU Leader, Transformation Manager, Account Manager/CRD, Contract supervisor, etc.), dates (Launch Date, Contract End Date), and other generic text fields (Regional Ownership, No of users supported, Project Training duration, Support channels, etc.).

### 6.2 Initial Staffing Model & 6.3 Latest Approved Model (tables)
- Rows = **Locations** (§5.1); columns = **Staffing Roles** (§5.2); cells = numeric.
- Same layout for both; only the values differ.

### 6.4 Contractual Languages (table)
- Rows = **Locations** (§5.1); columns = **Contractual Languages** (§5.3); cells = numeric.

### 6.5 Countries / Users / Devices Supported (table)
- Rows = **Countries** (§5.6); columns = **Devices** (§5.5); cells = numeric.

### 6.6 Automations (table)
Columns: Automation Name, Deployment Date, Cost of implementation (one time – $), Running cost (monthly – $), Efficiency impact (FTE/month), **Delivered By** (dropdown — Service Towers §5.7), Details. Mostly numeric/text.

### 6.8 Opportunities (table)
Columns: Opportunity Name, **Opportunity Type** (dropdown §5.8), **Related service** (dropdown §5.8), **Status** (dropdown §5.8), Estimated monthly value ($), Estimated contract duration (Months), **Delivered By** (dropdown §5.7), Details.

### 6.9 Support Hours (table)
- Rows = **Support Hours Languages** (§5.4).
- Columns: From (M-F) time, To (M-F) time, then **three mutually-exclusive radios — 8x5, 24/5, 24/7** (pick one coverage), then independent checkboxes **On call / Interpret (DHS)** and **Sophie**.

### 6.10 SLAs & KPIs (table)
Columns: Name, **Type** (dropdown §5.9 — SLA/KPI/HCM), Description, Formula, Target %, **Measurement type** (dropdown §5.9 — High/Low), **Can it be reported?** (Yes/No), **Financial penalties** (Yes/No), **Bonus** (Yes/No).

## 7. Admin section
- Manage **users** (create admins and users, assign roles).
- Manage **reference tables** (§5 — edit the values that populate dropdowns, table rows/columns, etc.).
