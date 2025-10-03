# Environments and Configuration

> **Purpose**: Clean separation of Dev, Staging, and Prod environments to prevent configuration drift and surprise behavior.

## Overview

This document defines environment-specific settings, configuration overlay precedence, and compliance with MTM Avalonia Template constitutional standards.

---

## For Humans

- **When Used**: Setup, deployment, troubleshooting.
- **Dependencies**: Config service, secrets, feature flags.
- **Consumers**: All components reading endpoints or toggling features.
- **Priority**: **High**

---

## For AI Agents

- **Intent**: Layered configuration (defaults → env file → user secrets → env vars) with deterministic resolution and safe fallbacks.
- **Dependencies**: Config loader, secret providers, feature flags.
- **Consumers**: All services needing endpoints/flags.
- **Non-Functionals**: Deterministic resolution; minimal runtime mutation.
- **Priority**: **High**

---

## Configuration Overlay Precedence

1. **Defaults** (shipped config)
2. **Environment File** (`appsettings.{Environment}.json`)
3. **User Secrets** (OS-native credential storage)
4. **Environment Variables** (highest precedence)

---

## Key Settings

### Visual (Read-Only)

- **Toolkit Endpoints**: Base URL, auth method, timeouts, retry/backoff.
- **Read-Only Enforce Flag**: `Visual.ReadOnly=true` (**non-negotiable**)
- **Poll Cadence**: Master cache polling (e.g., every 30 min), maintenance windows.
- **Table Size Awareness**:
  - `PART` table: ~117 fields (`MTMFG Tables.csv` lines 1657–1773)
  - `LOCATION` table: ~14 fields (lines 1513–1526)
  - `WAREHOUSE` table: ~16 fields (lines 4229–4244)
  - `SHOP_RESOURCE` table: ~42 fields (lines 3452–3493)
- **Citations**: All toolkit commands require citation: `Reference-{File Name} - {Chapter/Section/Page}`

### Credentials Flow

- **Validation**: Visual username/password validated locally against MAMP MySQL hash records (`APPLICATION_USER.USER_PWD` nvarchar(90)) before any server connection.
- **Security**: No plaintext secrets in logs or files.
- **Field Constraints**: `USERNAME` max 20 chars, `PASSWORD_HASH` 90 chars.

### API (App Server)

- **Base URL**: Per environment.
- **TLS**: Required.
- **Timeouts/Retry Policy**: Configurable.
- **Rate Limits**: Enforced.

### MySQL (MAMP, Desktop-Only Direct)

- **Connection**: Host, port, database name, user.
- **Android**: Uses API, not direct DB.

### Feature Flags

- `Visual.UseForItems`
- `Visual.UseForLocations`
- `Visual.UseForWorkCenters`
- `OfflineModeAllowed`
- `Printing.Enabled`

---

## Clarification Questions

- **Q:** Support per-plant overrides for endpoints?
  - **Why:** Multi-site deployments.
  - **Suggested:** Yes, via scoped profiles.
  - **Reason:** Flexibility without global changes.
  - **Options:**
    - [A] Global only
    - [B] Per-plant profiles
    - [C] Both

---

## Constitutional Alignment

- **CompiledBinding**: All configuration UI must use `x:DataType` and `{CompiledBinding}` in Avalonia XAML.
- **DI**: Configuration services registered via `AppBuilder.ConfigureServices()` in `Program.cs`.
- **Null Safety**: All config accessors use nullable reference types and error resilience patterns.
- **Security**: Credentials stored in OS-native storage, never logged.
- **Testing**: Configuration logic covered by xUnit tests with >80% coverage.

---

_Last updated: 2025-10-03 | See [constitution.md](../.specify/memory/constitution.md) for project principles._
