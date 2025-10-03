# Security and Compliance Policy

> **Purpose:** Prevent data leaks and unauthorized changes; document audit expectations.
> **Priority:** Critical
> **Scope:** Applies to all services, features, and agents (human + AI).

---

## Human-Facing Summary

- **Usage:**

  - Design reviews
  - Audits
  - Incident response

- **Dependencies:**

  - Authentication
  - Secrets management
  - Structured logging
  - Audit trail
  - Encryption-at-rest (mobile)

- **Downstream Impact:**
  - All services and features depend on this policy

---

## AI Agent Guidance

- **Intent:**

  - Enforce least privilege
  - Secure secrets handling
  - Require TLS for all API traffic
  - Audit logs for all state changes (app DB only)
  - Strict Visual ERP read-only access

- **Dependencies:**

  - Config/secrets
  - Policy catalog
  - Telemetry
  - Data protection

- **Consumers:**

  - API
  - Repositories
  - Boot diagnostics

- **Non-Functionals:**
  - Minimal overhead
  - Deterministic policies
  - Redaction of sensitive data

---

## Policies

### Visual ERP

- **Access:**

  - Only via Infor Visual API Toolkit
  - **Never** direct SQL access

- **Read-Only Enforcement:**

  - Enforced at Visual role and repository surfaces
  - No write methods exposed

- **Mobile:**

  - Android devices **never** hold Visual credentials
  - Mobile uses API only

- **Schema References:**
  - **PART** table: `ID` field `nvarchar(30)`
    _(Visual Data Table.csv, Lines 5779-5888)_
  - **LOCATION** table: `ID` field `nvarchar(15)`
    _(Lines 5246-5263)_
  - **WAREHOUSE** table: `ID` field `nvarchar(15)`
    _(Lines 14262-14288)_
  - **SITE** table: `ID` field `nvarchar(15)`
    _(Lines 9398-9443)_
  - Relationships: See `MTMFG Relationships.csv` (1266 foreign key relationships)

### Credentials

- Visual username/password stored as salted + stretched hashes in MAMP MySQL
- Validated locally before any Visual session
- **No plaintext credentials at rest**

### Transport

- **HTTPS required** for API in production (prefer dev as well)
- Pin TLS minimum version

### Data at Rest

- Encrypt sensitive caches and tokens on mobile
- Rotate keys per policy

### Audit

- All app DB writes record:
  - User
  - Timestamp
  - Reason
- Visual reads are not writes, but key references may be logged for traceability
  - **No sensitive payloads logged**

---

## Clarification Questions

- **Q:** Minimum TLS version for API?
  - **Why:** Security baseline
  - **Suggested:** TLS 1.2+
  - **Reason:** Broad support and security
  - **Options:**
    - [A] 1.2
    - [B] 1.3
    - [C] Both

---

> **Constitutional Alignment:**
>
> - Cross-platform: Applies to all platforms
> - MVVM: Secure secrets via DI
> - Test-first: Audit and security tests required
> - Theme: No sensitive data in UI resources
> - Null safety: No null credentials
> - CompiledBinding: No secrets in XAML
> - DI: Secure credential services via AppBuilder
