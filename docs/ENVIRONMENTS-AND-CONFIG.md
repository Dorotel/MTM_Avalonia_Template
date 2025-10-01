# Environments and Configuration
Defines environment-specific settings and overlay precedence.

For humans
- Purpose: Keep Dev/Staging/Prod cleanly separated; avoid surprise behavior.
- When used: Setup, deployment, troubleshooting.
- Dependencies: Config service, secrets, feature flags.
- What depends on it: Everything that reads endpoints or toggles features.
- Priority: High.

For AI agents
- Intent: Establish layered configuration (defaults → env file → user secrets → env vars), with environment selection and safe fallbacks.
- Dependencies: Config loader, secret providers, feature flags.
- Consumers: All services needing endpoints/flags.
- Non-functionals: Deterministic resolution; minimal runtime mutation.
- Priority: High.

Key settings
- Visual (read-only):
  - Toolkit endpoints: base URL, auth method, timeouts, retry/backoff.
  - Read-only enforce flag: Visual.ReadOnly=true (non-negotiable).
  - Poll cadence for master caches (e.g., 30 min) with maintenance windows.
  - Citations required for any toolkit command: Reference-{File Name} - {Chapter/Section/Page}.
- Credentials flow:
  - Visual username/password validated locally against MAMP MySQL hash records before any server connection.
  - No plaintext secrets in logs or files.
- API (your app server):
  - Base URL per environment, TLS on, timeouts, retry policy, rate limits.
- MySQL (MAMP, desktop-only direct):
  - Host, port, database name, user; Android uses API instead.
- Feature flags:
  - Visual.UseForItems/Locations/WorkCenters
  - OfflineModeAllowed
  - Printing.Enabled

Clarification questions
- Q: Support per-plant overrides for endpoints?
  - Why: Multi-site deployments.
  - Suggested: Yes, via scoped profiles.
  - Reason: Flexibility without global changes.
  - Options: [A] Global only [B] Per-plant profiles [C] Both