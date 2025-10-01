# UI and UX Guidelines for Shop-Floor Use
Design guidance for reliable operator experience.

For humans
- Purpose: Reduce errors and speed up tasks with clear, touch-friendly screens.
- When used: Designing or reviewing any screen.
- Dependencies: Localization, converters/formatters, accessibility settings.
- What depends on it: All features.
- Priority: High.

For AI agents
- Intent: Provide patterns for large targets, high-contrast themes, minimal typing (scan-first), progress and error feedback, and offline banners.
- Dependencies: Theming (post-splash), barcode service, validation, navigation.
- Consumers: All views.
- Non-functionals: Low cognitive load, latency under 300 ms for critical interactions.
- Priority: High.

Guidelines
- Touch targets ≥ 44x44 px; large type.
- Scan-first workflows; minimize free text.
- Clear state banners: Online, Degraded (Visual cache), Offline (queued).
- Timers and blocking actions show cancellable progress.
- Accessibility: Color contrast, focus states, keyboard support on desktop.

Clarification questions
- Q: Default theme after splash?
  - Why: Comfort and safety on the floor.
  - Suggested: Auto with user override.
  - Reason: Adaptable with control.
  - Options: [A] Light [B] Dark [C] Auto