# Outstanding Clarification Questions: Secrets & Credential Management - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Secrets & Credential Management** feature.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (6 total)

### Q1: [Security] - Credential storage mechanism

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-021, FR-022 secure credential loading and encryption

What mechanism should be used for credential storage?

| Option | Description |
|--------|-------------|
| A | Windows Credential Manager / Android KeyStore |
| B | Encrypted local database (SQLite with encryption) |
| C | Encrypted configuration file (AES-256) |
| D | External secrets manager (Azure Key Vault, AWS Secrets Manager) |
| E | Hybrid: OS keychain for user credentials + external for app secrets |

**Answer: A — Windows Credential Manager / Android KeyStore**

Reasoning: Leverages OS-native secure storage with hardware-backed encryption (TPM/Secure Enclave). No custom crypto implementation reduces security risks. Per-platform APIs handle multi-user scenarios correctly. Manufacturing environment likely lacks external secrets infrastructure (D). **Note**: macOS Keychain support deferred - project currently targets Windows desktop + Android only.

---

### Q2: [Security] - Credential encryption key management

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-022 encrypt credentials at rest

How should encryption keys be managed?

| Option | Description |
|--------|-------------|
| A | User-provided passphrase (entered at startup) |
| B | Machine-specific key (DPAPI on Windows, equivalent on other OS) |
| C | Certificate-based encryption (PKI) |
| D | Hardware security module (TPM/Secure Enclave) |
| E | Combination: Machine key + user passphrase (2-factor) |

**Answer: B — Machine-specific key (DPAPI on Windows, KeyStore on Android)**

Reasoning: Integrated with answer Q1 - OS keychain APIs use DPAPI/KeyStore automatically. User passphrases (A) add friction to manufacturing workflows; HSM (D) not available in typical deployments. Machine binding prevents credential portability (security feature). **Note**: macOS equivalents deferred with macOS platform support.

---

### Q3: [Security] - Credential redaction scope

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-023 never log credentials

What patterns should be redacted from logs?

| Option | Description |
|--------|-------------|
| A | Only literal "password" and "token" fields |
| B | Pattern-based detection (any string following "password:", base64 tokens, etc.) |
| C | All strings longer than 20 characters |
| D | Comprehensive list: passwords, tokens, API keys, connection strings with credentials |
| E | Any field marked "sensitive" in data model + pattern detection |

**Answer: E — Any field marked "sensitive" in data model + pattern detection**

Reasoning: Structured approach - schema-driven (mark fields sensitive) + defensive patterns (catch unmarked secrets). Comprehensive list (D) requires maintenance; pattern-only (B) has false positives/negatives. Logging framework enforces automatically.

---

### Q4: [Security] - Credential rotation support

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: Overall credential management

Should the system support credential rotation?

| Option | Description |
|--------|-------------|
| A | Yes, automatic rotation based on policy (e.g., every 90 days) |
| B | Yes, manual rotation via admin interface |
| C | Yes, prompted rotation on first failed auth |
| D | No, credentials managed externally only |

**Answer: C — Yes, prompted rotation on first failed auth**

Reasoning: Reactive rotation on auth failure addresses actual security issues (password changed, account locked). Automatic rotation (A) needs coordination with Visual ERP admin; manual interface (B) is supplement. Prompt guides users through resolution.

---

### Q5: [Integration] - Multi-user credential handling

**Impact**: High
**Category**: Domain & Data Model
**Feature Context**: FR-021 Visual credentials for current user

How should credentials be managed in multi-user scenarios?

| Option | Description |
|--------|-------------|
| A | Each Windows user has separate encrypted credential store |
| B | Single shared credential store, user selected at login |
| C | Credentials stored per-device (shared by all users on that device) |
| D | Always prompt for credentials at startup (no storage) |
| E | Hybrid: Option to store or prompt per user preference |

**Answer: A — Each Windows user has separate encrypted credential store**

Reasoning: Aligns with OS security model - Windows/macOS user accounts provide natural isolation. Each operator uses their own Visual credentials (per FR-021 "current user"). OS keychain APIs automatically scope to current user.

---

### Q6: [Security] - Credential validation frequency

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-136 validate credentials before remote connections

When should stored credentials be re-validated?

| Option | Description |
|--------|-------------|
| A | Only at startup |
| B | At startup + before each Visual API call |
| C | At startup + hourly background check |
| D | At startup + after any authentication failure |
| E | At startup + on-demand via "Test Connection" button |

**Answer: D — At startup + after any authentication failure**

Reasoning: Startup validation catches expired/changed credentials early. Post-failure validation enables recovery flow (prompt for new credentials). Per-call validation (B) adds excessive overhead; background checks (C) unnecessary if auth failures trigger validation.

---
