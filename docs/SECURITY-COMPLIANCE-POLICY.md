# Feature Specification: Security and Compliance Policy

Feature Branch: security-compliance-policy
Created: 2025-10-06
Status: Draft
Input: Source policy document describing security posture, transport requirements, credential handling, Visual ERP access controls, auditing, and AI agent guidance.

---

## Execution Flow (main)

1. Parse input policy and extract intent, dependencies, consumers, and scope
  → Security and Compliance Policy for all services, features, and agents
2. Identify actors and systems
  → Developers, System Administrators, Auditors, Incident Responders, AI Agents, API/Repositories
3. Extract policy domains
  → Visual ERP access, credentials, transport/TLS, data at rest, auditing, agent behavior
4. Transform statements into testable functional and non-functional requirements
5. Preserve references and schema notes verbatim (Visual tables and relationships)
6. Record unresolved decisions as Clarifications with rationale
7. Define user scenarios, acceptance criteria, and edge cases
8. Return: SUCCESS (spec structured without data loss; ready for planning)

---

## Clarifications

Use this section whenever any [NEEDS CLARIFICATION: …] markers exist in the spec. Keep each question atomic and traceable.

### Index
- CL-001 (Minimum TLS version)
- CL-002 (Credential storage location vs repo standards)
- CL-003 (Audit log scope for Visual reads)
- CL-004 (Transport enforcement in Development)
- CL-005 (Audit implementation: table vs per-row columns)

### CL-001: Minimum TLS version
- Status: Unanswered
- Question: What minimum TLS version must be enforced for API?
- Context: Policy suggests TLS 1.2+, options include 1.2, 1.3, or both.
- Options:
  - [A] TLS 1.2 only
  - [B] TLS 1.3 only
  - [C] Accept 1.2 and prefer 1.3
- Reason: Security baseline and broad client support.

### CL-002: Credential storage location vs repo standards
- Status: Unanswered
- Question: Should Visual username/password be stored as salted + stretched hashes in MAMP MySQL or exclusively in OS-native secure storage?
- Context: This policy states MySQL hashed storage and local validation; repository-wide standards mandate OS-native secure storage (DPAPI/KeyStore) with no DB-stored secrets.
- Options:
  - [A] OS-native only (repository standard)
  - [B] DB-hash + OS-native hybrid
  - [C] DB-hash only (policy text)
- Impact: Security posture, offline auth, compliance with existing architecture.

### CL-003: Audit log scope for Visual reads
- Status: Unanswered
- Question: What level of Visual read traceability is required without logging sensitive payloads?
- Options:
  - [A] Log entity keys only (IDs)
  - [B] Log request metadata (user, action, time, endpoint) plus keys
  - [C] No logging for reads; log only failures
- Constraint: “No sensitive payloads logged.”

### CL-004: Transport enforcement in Development
- Status: Unanswered
- Question: Is HTTPS strictly required in Development or only in Production (preferred in Dev per policy)?
- Options:
  - [A] Required in Dev and Prod
  - [B] Required in Prod; recommended in Dev
  - [C] Allowed HTTP in Dev with explicit opt-in flag

### CL-005: Audit implementation structure
- Status: Unanswered
- Question: Should auditing be implemented via a dedicated AuditTrail table or per-row audit columns on each table?
- Options:
  - [A] Central AuditTrail table
  - [B] Per-table audit columns
  - [C] Hybrid (central table + minimal per-row stamps)
- Constraint: “All app DB writes record: user, timestamp, reason.”

---

## Quick Guidelines

- Purpose: Prevent data leaks and unauthorized changes; document audit expectations.
- Priority: Critical.
- Scope: All services, features, and agents (human + AI).
- Non-Functionals: Minimal overhead, deterministic policies, redaction of sensitive data.

---

## Human-Facing Summary

- Usage:
  - Design reviews
  - Audits
  - Incident response
- Dependencies:
  - Authentication
  - Secrets management
  - Structured logging
  - Audit trail
  - Encryption-at-rest (mobile)
- Downstream Impact:
  - All services and features depend on this policy

---

## AI Agent Guidance

- Intent:
  - Enforce least privilege
  - Secure secrets handling
  - Require TLS for all API traffic
  - Audit logs for all state changes (app DB only)
  - Strict Visual ERP read-only access
- Dependencies:
  - Config/secrets
  - Policy catalog
  - Telemetry
  - Data protection
- Consumers:
  - API
  - Repositories
  - Boot diagnostics
- Non-Functionals:
  - Minimal overhead
  - Deterministic policies
  - Redaction of sensitive data

---

## Policies

### Visual ERP

- Access:
  - Only via Infor Visual API Toolkit
  - Never direct SQL access
- Read-Only Enforcement:
  - Enforced at Visual role and repository surfaces
  - No write methods exposed
- Mobile:
  - Android devices never hold Visual credentials
  - Mobile uses API only
- Schema References:
  - PART table: ID field nvarchar(30) (Visual Data Table.csv, Lines 5779-5888)
  - LOCATION table: ID field nvarchar(15) (Lines 5246-5263)
  - WAREHOUSE table: ID field nvarchar(15) (Lines 14262-14288)
  - SITE table: ID field nvarchar(15) (Lines 9398-9443)
  - Relationships: See MTMFG Relationships.csv (1266 foreign key relationships)

### Credentials

- Visual username/password stored as salted + stretched hashes in MAMP MySQL
- Validated locally before any Visual session
- No plaintext credentials at rest

### Transport

- HTTPS required for API in production (prefer dev as well)
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
  - No sensitive payloads logged

---

## User Scenarios & Testing

### Primary User Stories

- Developer Scenario: Implements repositories and API clients that must enforce read-only Visual access, use HTTPS with pinned minimum TLS, and avoid logging sensitive payloads while emitting structured audit events for state changes.
- System Administrator Scenario: Configures transport security (TLS policy), verifies secrets handling, and ensures mobile devices never store Visual credentials, relying on API-only access.
- Auditor/Incident Responder Scenario: Reviews audit records for all app DB writes including user, timestamp, and reason; confirms Visual read traces exclude sensitive payloads; validates policy adherence during incidents.

### Acceptance Scenarios

1. Given a repository that accesses Visual, when executing operations, then only read-only Toolkit commands are allowed and no write methods are exposed.
2. Given the API is deployed to production, when clients connect, then HTTPS is required and the minimum TLS version is enforced per policy decision.
3. Given an application write occurs to the app DB, when the write completes, then audit data includes user, timestamp, and reason.
4. Given a Visual read occurs, when logging is produced, then only key references or metadata are logged and no sensitive payloads are included.
5. Given the mobile application runs on Android, when credentials are needed for Visual, then the device does not store Visual credentials and accesses data via API only.
6. Given secrets are handled by services, when telemetry/logging is emitted, then sensitive data is redacted.

### Edge Cases

- HTTPS in Development: Clarify whether HTTP is ever allowed locally (CL-004).
- TLS Version: Clarify minimum version and whether to allow both 1.2 and 1.3 (CL-001).
- Visual Reads Logging: Define minimal metadata set to avoid sensitive payloads while preserving traceability (CL-003).
- Credential Storage: Resolve conflict between MySQL hashed storage vs OS-native secrets repository standard (CL-002).
- Audit Structure: Decide centralized vs per-table approach (CL-005).

---

## Requirements

### Functional Requirements

#### Visual ERP Access Control
- FR-001: System MUST access Visual only via Infor Visual API Toolkit; direct SQL access MUST be disallowed.
- FR-002: System MUST enforce read-only access for Visual at role and repository layers; no write methods exposed.
- FR-003: Android/mobile clients MUST NOT hold Visual credentials locally and MUST use API-only access.

#### Credential Handling
- FR-004: Visual credentials MUST NOT be stored in plaintext at rest.
- FR-005: Visual credentials SHOULD be stored as salted + stretched hashes in MAMP MySQL and validated locally before Visual sessions (subject to CL-002 decision).
- FR-006: Secrets handling MUST ensure redaction in logs and telemetry.

#### Transport Security
- FR-007: HTTPS MUST be required for API in production; Development enforcement is subject to CL-004.
- FR-008: A minimum TLS version MUST be pinned per CL-001 decision.

#### Data at Rest
- FR-009: Sensitive caches and tokens on mobile MUST be encrypted at rest.
- FR-010: Encryption keys MUST be rotated per defined policy schedule.

#### Auditing
- FR-011: All application DB write operations MUST record user, timestamp, and reason.
- FR-012: Visual read operations MUST NOT log sensitive payloads; key references and/or minimal metadata MAY be logged for traceability per CL-003.
- FR-013: Audit logs MUST use structured logging compatible with existing telemetry.

#### AI Agent Behavior
- FR-014: Agents MUST enforce least privilege in generated designs and code.
- FR-015: Agents MUST enforce TLS for all API traffic in examples and scaffolds.
- FR-016: Agents MUST avoid emitting any write-capable Visual methods and MUST preserve read-only posture.

### Non-Functional Requirements

- NFR-001: Policy enforcement MUST introduce minimal overhead.
- NFR-002: Policies MUST be deterministic and consistently applied across services.
- NFR-003: Sensitive data MUST be redacted in all logs/telemetry.
- NFR-004: Scope applies cross-platform (desktop and mobile).
- NFR-005: Security and audit tests MUST exist to validate the above behaviors.

---

## Key Entities

- PolicyCatalog: Centralized repository of security and compliance rules.
- TransportPolicy: Defines HTTPS requirement and pinned TLS minimum version.
- CredentialsPolicy: Defines credential storage, hashing/validation, and redaction rules.
- VisualAccessPolicy: Enforces Toolkit-only and read-only access constraints.
- AuditService/AuditTrail: Emits and persists audit records (user, timestamp, reason) for app DB writes.
- Telemetry/Logger: Structured logging with redaction.

---

## References

- Visual Schema References (verbatim from policy):
  - PART table: ID nvarchar(30) (Visual Data Table.csv, Lines 5779-5888)
  - LOCATION table: ID nvarchar(15) (Lines 5246-5263)
  - WAREHOUSE table: ID nvarchar(15) (Lines 14262-14288)
  - SITE table: ID nvarchar(15) (Lines 9398-9443)
  - Relationships: MTMFG Relationships.csv (1266 foreign key relationships)

---

## Database Schema Changes

- No explicit schema defined in policy. Auditing requirement (user, timestamp, reason) implies either:
  - Option A: Central AuditTrail table, or
  - Option B: Per-table audit columns.
- Decision required (CL-005). Until resolved, document intent only; no schema migration specified here.

---

## Review & Acceptance Checklist

- Content Quality
  - [x] Original policy content preserved without data loss
  - [x] Clear mapping to testable requirements
- Requirement Completeness
  - [x] Functional and non-functional requirements extracted
  - [x] Acceptance scenarios and edge cases documented
  - [x] References preserved
  - [ ] Clarifications resolved (pending CL-001, CL-002, CL-003, CL-004, CL-005)

---

## Execution Status

- [x] Policy parsed and structured as spec
- [x] Requirements generated
- [x] Entities identified
- [x] Scenarios defined
- [x] Clarifications recorded
- [ ] Open clarifications resolved
- [x] Ready for planning

---

## Constitutional Alignment

- Cross-platform: Applies to all platforms
- MVVM/DI: Secure secrets via DI; no secrets in XAML
- Test-first: Audit and security tests required
- Theme: No sensitive data in UI resources
- Null safety: No null credentials
- CompiledBinding: No secrets exposed in bindings
- DI: Secure credential services via AppBuilder
- TLS: Enforce HTTPS/TLS per policy decision
- Visual: Strict read-only access and Toolkit-only usage
