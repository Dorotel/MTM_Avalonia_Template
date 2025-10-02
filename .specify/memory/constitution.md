<!--
SYNC IMPACT REPORT - Constitution Update
Version Change: TEMPLATE → 1.0.0 (Initial ratification)
Modified Principles: 
  - Added: I. Cross-Platform First (Avalonia multi-platform support)
  - Added: II. MVVM Community Toolkit Standard (Source generation patterns)
  - Added: III. Test-First Development (TDD mandatory)
  - Added: IV. Theme V2 Semantic Tokens (Consistent theming)
  - Added: V. Null Safety and Error Resilience (Manufacturing reliability)
Added Sections:
  - Manufacturing Domain Requirements (Industry-specific constraints)
  - Development Workflow (Production-ready template requirements)
Templates Requiring Updates:
  ✅ plan-template.md: Constitution version reference updated (v2.1.1 → v1.0.0)
  ✅ spec-template.md: Aligned with constitutional principles
  ✅ tasks-template.md: TDD principles integrated
Follow-up TODOs: None - all placeholders resolved
-->

# MTM Avalonia Template Constitution

## Core Principles

### I. Cross-Platform First

All features MUST work across all supported platforms (Desktop, Mobile, Browser); Platform-specific code MUST be abstracted through interfaces; Shared business logic lives in common libraries; Platform differences handled via dependency injection.

**Rationale**: Avalonia's strength is true cross-platform capability. Breaking this principle fragments the user experience and increases maintenance overhead.

### II. MVVM Community Toolkit Standard (NON-NEGOTIABLE)

Use MVVM Community Toolkit 8.3+ patterns exclusively: [ObservableObject] with [ObservableProperty] for ViewModels; [RelayCommand] for commands; NO ReactiveUI patterns (ReactiveObject, ReactiveCommand, RaiseAndSetIfChanged); Constructor dependency injection for services.

**Rationale**: Provides source generation, reduces boilerplate, ensures consistency. ReactiveUI patterns conflict and create maintenance confusion.

### III. Test-First Development (NON-NEGOTIABLE)

TDD mandatory: Tests written → User approved → Tests fail → Then implement; Red-Green-Refactor cycle strictly enforced; Contract tests for all API endpoints; Integration tests for user workflows; Unit tests for business logic.

**Rationale**: Avalonia's complexity requires reliable testing. Template users need confidence in foundation stability.

### IV. Theme V2 Semantic Tokens

All styling MUST use Theme V2 dynamic resources for adaptive theming; Consistent Manufacturing Design System integration; Semantic token usage over hardcoded values; Base styles with variant overrides for customization.

**Rationale**: Ensures consistent theming across the application and enables easy theme switching and customization.

### V. Null Safety and Error Resilience

Enable nullable reference types; ArgumentNullException.ThrowIfNull() for method parameters; Proper error boundaries in ViewModels; Graceful degradation for offline scenarios; Comprehensive logging with structured data.

**Rationale**: Manufacturing environments are mission-critical. Application crashes are unacceptable and can halt operations.

## Manufacturing Domain Requirements

All template features MUST align with manufacturing/warehouse operations; Support offline-first scenarios with sync capabilities; Implement barcode scanning and labeling standards; Role-based access control for manufacturing environments; Data contracts designed for industrial reliability.

## Development Workflow

Code reviews MUST verify constitutional compliance; All template components must be production-ready; Documentation includes quickstart guides and troubleshooting; Agent-specific files (CLAUDE.md, .github/copilot-instructions.md) kept under 150 lines; Complexity deviations require explicit justification.

## Governance

Constitution supersedes all other development practices; Amendments require documentation, approval, and template migration plan; All PRs/reviews must verify constitutional compliance; Complexity must be justified in implementation plans; Use docs/TOC-IMPLEMENTATION-ORDER.md for structured development guidance.

**Version**: 1.0.0 | **Ratified**: 2025-10-02 | **Last Amended**: 2025-10-02