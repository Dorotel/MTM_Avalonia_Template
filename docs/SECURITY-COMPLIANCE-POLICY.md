# Security and Compliance Policy
Summarizes security practices and read-only ERP constraints.

For humans
- Purpose: Prevent data leaks and unauthorized changes; document audit expectations.
- When used: Design reviews, audits, incident response.
- Dependencies: Auth, secrets, logging, audit trail, encryption-at-rest (mobile).
- What depends on it: All services and features.
- Priority: Critical.

For AI agents
- Intent: Enforce least privilege, secure secrets handling, TLS, audit logs for all state changes (app DB only), and strict Visual read-only access.
- Dependencies: Config/secrets, policy catalog, telemetry, data protection.
- Consumers: API, repositories, boot diagnostics.
- Non-functionals: Minimal overhead, deterministic policies, redaction of sensitive data.
- Priority: Critical.

Policies
- Visual ERP:
  - Access via Infor Visual API Toolkit only; never direct SQL.
  - Read-only enforced at Visual role and at repository surfaces (no write methods).
  - Android devices never hold Visual credentials; mobile uses API only.
  - Schema References:
    - PART table (Visual Data Table.csv, Line: 5779-5888): ID field nvarchar(30)
    - LOCATION table (Visual Data Table.csv, Line: 5246-5263): ID field nvarchar(15)
    - WAREHOUSE table (Visual Data Table.csv, Line: 14262-14288): ID field nvarchar(15)
    - SITE table (Visual Data Table.csv, Line: 9398-9443): ID field nvarchar(15)
    - Relationships documented in MTMFG Relationships.csv (1266 foreign key relationships)
- Credentials:
  - Visual username/password stored as salted+stretched hashes in MAMP MySQL; validated locally before any Visual session; no plaintext at rest.
- Transport:
  - HTTPS for API in production (prefer dev as well); pin TLS min version.
- Data at rest:
  - Encrypt sensitive caches and tokens on mobile; rotate keys as policy dictates.
- Audit:
  - All app DB writes record user, timestamp, reason; Visual reads are not writes but key references may be logged for traceability (no sensitive payloads).

Clarification questions
- Q: Minimum TLS version for API?
  - Why: Security baseline.
  - Suggested: TLS 1.2+.
  - Reason: Broad support and security.
  - Options: [A] 1.2 [B] 1.3 [C] Both