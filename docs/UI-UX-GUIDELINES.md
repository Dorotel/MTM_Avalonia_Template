# UI & UX Guidelines for Shop-Floor Use

> **Purpose:** Reliable operator experience—reduce errors, speed up tasks, ensure clarity and touch-friendliness.

## Overview

| Audience      | Intent / Priority                                                                                                  | Dependencies / Consumers                                       | Non-Functionals                                                           |
| ------------- | ------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------- | ------------------------------------------------------------------------- |
| **Humans**    | Clear, fast, error-resistant screens. <br> **Priority:** High                                                      | Localization, converters, accessibility                        | All features depend on these guidelines.                                  |
| **AI Agents** | Large targets, high-contrast themes, scan-first, progress/error feedback, offline banners. <br> **Priority:** High | Theming (post-splash), barcode service, validation, navigation | All views. <br> Low cognitive load, latency < 300ms for critical actions. |

---

## Design Guidelines

- **Touch Targets:** ≥ 44x44 px; use large type.
- **Scan-First Workflows:** Minimize free text input.
  - **Part Number:** Max 30 chars (`PART.ID` constraint, Visual ERP)
  - **Location Code:** Max 15 chars (`LOCATION.ID` constraint, Visual ERP)
  - **Lot Number:** Max 30 chars (`TRACE.ID` constraint, Visual ERP)
- **State Banners:**
  - **Online:** Normal operation.
  - **Degraded:** Show last Visual sync timestamp; indicate cache age.
  - **Offline:** Display queued transaction count (queue depth).
- **Progress & Blocking Actions:** Show cancellable progress indicators.
- **Accessibility:** Ensure color contrast, visible focus states, keyboard support.

---

## Theming & Feedback

- **Default Theme After Splash:** Auto (with user override recommended for comfort/safety).
  - **Options:** [A] Light [B] Dark [C] Auto
- **Banner Feedback:** Always show current state (Online/Degraded/Offline) with relevant details.

---

## Clarification Questions

- **Q:** What is the default theme after splash?
  - **A:** Auto, with user override.
  - **Reason:** Comfort and safety; adaptable with control.

---

> **Note:** All UI/XAML must use `x:DataType` and `{CompiledBinding}` (see [Constitution VI](../.specify/memory/constitution.md)). Accessibility and scan-first patterns are mandatory for manufacturing reliability.
