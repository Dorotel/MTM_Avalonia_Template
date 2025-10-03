# Visual Credentials Flow — Secure Storage, Local Validation, Read-Only Access

## Overview

**Purpose:**
Protect Visual credentials, verify them safely before contacting the Visual environment, and guarantee read-only access across the app.

**Usage:**

- At login and on every session start prior to any Visual call.

**Dependencies:**

- MAMP MySQL (secure credential storage)
- Security/Compliance policy
- Environments/Config
- Visual API Toolkit integration

**Consumers:**

- Any feature consulting Visual master data (receiving, put-away, kitting, cycle counts, traceability, reporting)

**Priority:**

- **Critical**

---

## AI Agent Implementation Intent

- **Secure, local-first credential check:**
  Validate Visual username/password against a salted, stretched hash stored in MAMP MySQL.
- **Session initiation:**
  Only upon successful validation, initiate Visual API Toolkit session under read-only privileges.
- **Security:**
  - Never write to Visual DB.
  - Never store or log plain passwords.
- **Dependencies:**
  - Crypto (salted hashing, key stretching)
  - Parameterized DB access
  - Secrets redaction
  - Visual API Toolkit client
  - Policy enforcement
- **Non-functionals:**
  - Zero plaintext at rest
  - No secrets in logs
  - Rate limits on login attempts
  - Lockout on repeated failures
  - Minimal latency

---

## Flow Steps

1. **User Input:**
   User enters Visual username/password in the app.
2. **Local Validation:**
   App validates credentials locally in MAMP MySQL using salted+stretched hash over a secure channel.
   - Visual username: `APPLICATION_USER.NAME` (`nvarchar(20)`)
     _Reference: Visual Data Table.csv Line 565_
   - Password hash: `APPLICATION_USER.USER_PWD` (`nvarchar(90)`)
     _Reference: Visual Data Table.csv Line 576_
3. **Session Establishment:**
   On success, establish Visual API Toolkit session under read-only role.
4. **Read-Only Operations:**
   Proceed with Visual operations (Toolkit commands only), respecting allowlist and citations.
5. **Session Termination:**
   On logout/timeout, destroy session and purge any sensitive tokens.

---

## Storage Rules

- Use salted hashing and key stretching (e.g., bcrypt/Argon2).
  Store only hash, salt, and parameters.
- Encrypt additional sensitive profile fields at rest when applicable.
- Parameterize all DB queries.
- Apply per-user rate limits.
- Audit access without logging secrets.

---

## Read-Only Enforcement

- Only Visual API Toolkit calls; **never direct SQL**.
- Enforce read-only at Visual role and repository surfaces (no write methods).
- For every read-only reference, include precise citation:
  `Reference-{File Name} - {Chapter/Section/Page}`
  If unknown at authoring time, mark as:
  `Page: <to be filled>` and add to “Review Required.”

---

## Clarification Questions

- **Periodic re-authentication to the Visual session?**
  - **Why:** Balance security and usability.
  - **Suggested:** Yes, after 30–60 minutes of inactivity.
  - **Reason:** Common security practice.
  - **Options:**
    - [A] 30 min
    - [B] 60 min
    - [C] Every shift
    - [D] Other: **\_\_**

---

<!-- Constitutional Compliance:
- Follows cross-platform and MVVM standards.
- All DB access is parameterized.
- No plaintext secrets at rest or in logs.
- Read-only enforcement aligns with manufacturing domain requirements.
- Citations and references included for traceability.
-->
