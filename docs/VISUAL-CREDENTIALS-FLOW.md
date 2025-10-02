# Visual Credentials Flow — Secure Storage, Local Validation, Read-Only Access

For humans
- Purpose: Protect Visual credentials, verify them safely before contacting the Visual environment, and guarantee read-only access across the app.
- When used: At login and on every session start prior to any Visual call.
- Dependencies: MAMP MySQL (for secure credential storage), Security/Compliance policy, Environments/Config, Visual API Toolkit integration.
- What depends on it: Any feature that consults Visual master data (receiving, put-away, kitting, cycle counts, traceability, reporting).
- Priority: Critical.

For AI agents
- Intent: Implement a secure, local-first credential check that validates Visual username/password against a salted, stretched hash stored in MAMP MySQL; only upon success initiate Visual API Toolkit session under read-only privileges. Never write to Visual DB. Never store or log plain passwords.
- Dependencies: Crypto (salted hashing, key stretching), parameterized DB access, secrets redaction, Visual API Toolkit client, policy enforcement.
- Consumers: Auth flow, Visual read-only repositories, boot diagnostics, API backend for Android.
- Non-functionals: Zero plaintext at rest, no secrets in logs, rate limits on login attempts, lockout on repeated failures, minimal latency.
- Priority: Critical.

Flow (no code; steps only)
1) User inputs Visual username/password in the app.
2) App validates locally in MAMP MySQL (salted+stretched hash) over a secure channel (no plaintext storage).
   - Visual username stored in APPLICATION_USER.NAME nvarchar(20) (Visual Data Table.csv Line: 565)
   - Password hash stored in APPLICATION_USER.USER_PWD nvarchar(90) (Visual Data Table.csv Line: 576)
3) On success, establish Visual API Toolkit session under read-only role.
4) Proceed with read-only Visual operations (Toolkit commands only), respecting allowlist and citations.
5) On logout/timeout, destroy session; purge any sensitive tokens.

Storage rules
- Salted hashing and key stretching (e.g., bcrypt/Argon2). Store only hash/salt/params.
- Encrypt additional sensitive profile fields at rest when applicable.
- Parameterize all DB queries; apply per-user rate limits; audit access without logging secrets.

Read-only enforcement
- Visual API Toolkit calls only; never direct SQL.
- Enforce read-only at Visual role and repository surfaces (no write methods).
- For every read-only reference, include precise citation: Reference-{File Name} - {Chapter/Section/Page}. If unknown at authoring time, mark “Page: <to be filled>” and add to “Review Required.”

Clarification questions
- Q: Periodic re-authentication to the Visual session?
  - Why: Balance security and usability.
  - Suggested: Yes, after 30–60 minutes of inactivity.
  - Reason: Common security practice.
  - Options: [A] 30 min [B] 60 min [C] Every shift [D] Other: ______