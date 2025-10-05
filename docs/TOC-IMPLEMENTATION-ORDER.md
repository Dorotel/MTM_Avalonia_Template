# Implementation Order (TOC) — Constitutional Compliance

_Audience: Step-by-step order, always building foundations first. Each step explains why it's next, dependencies, and what "done" means. All steps comply with the MTM Avalonia Template Constitution v1.1.0._

---

## Major Constitutional Rules

- **Cross-Platform First:** All features must work on Windows desktop and Android. Platform-specific code is abstracted via DI and interfaces. Note: macOS and Linux desktop support deferred to future releases.
- **MVVM Community Toolkit:** All ViewModels use `[ObservableObject]`, `[ObservableProperty]`, `[RelayCommand]`, and constructor DI.
- **CompiledBinding Only:** All XAML must use `{CompiledBinding}` with `x:DataType` and `x:CompileBindings="True"`.
- **Test-First Development:** All features require xUnit tests before implementation; >80% coverage on critical paths.
- **Async/Await Patterns:** All async methods require `CancellationToken` parameters.
- **Security-First:** Credentials stored in OS-native storage; no Visual writes; audit logging enforced.
- **Spec-Driven:** All features follow Spec-Kit workflow (`/specify`, `/plan`, `/tasks`, etc.).

---

## Phase 1 — Security and Foundations

1. **`docs/SECURITY-COMPLIANCE-POLICY.md`**

- **Why:** Establish Visual read-only boundaries, credential storage, and audit requirements.
- **Dependencies:** None
- **Done When:** Team understands Visual read-only, credential handling (OS-native), audit logging (Serilog).
- **Constitution:** Security guidelines, Null Safety, Error Resilience.

2. **`docs/ENVIRONMENTS-AND-CONFIG.md`**

- **Why:** Manage Dev/Staging/Prod configs, feature flags, and Toolkit endpoints.
- **Dependencies:** Security policy
- **Done When:** Environment switching works; configs use DI; credentials not hardcoded.
- **Constitution:** Cross-Platform, DI via AppBuilder.

3. **`docs/BOOT-SEQUENCE.md`** - COMPLETE

- **Why:** Predictable startup, service initialization order, splash screen.
- **Dependencies:** Security, environment config
- **Done When:** Splash screen initializes via DI; caches warmed; diagnostics pass.
- **Constitution:** MVVM, DI, CompiledBinding, Async/Await.

---

## Phase 2 — Data and Visual Boundaries

4. **`docs/DATA-CONTRACTS.md`**

- **Why:** Stable DTOs for API, storage, and UI; exact field lengths/types from Visual.
- **Dependencies:** Security, Visual schema
- **Done When:** DTOs use nullable reference types; validation via FluentValidation.
- **Constitution:** Null Safety, Validation, Test-First.

5. **`docs/VISUAL-CREDENTIALS-FLOW.md`**

- **Why:** Secure credential handling before Visual access.
- **Dependencies:** Security, data contracts
- **Done When:** Local validation; OS-native credential storage; no plaintext.
- **Constitution:** Security, Null Safety.

6. **`docs/VISUAL-WHITELIST.md`**

- **Why:** Explicit allowlist for Visual reads; CSV line references.
- **Dependencies:** Data contracts, credentials flow
- **Done When:** Allowlist documented; FK relationships mapped.
- **Constitution:** Security, Spec-Driven.

---

## Phase 3 — Platform Access and Reliability

7. **`docs/API-SPECIFICATION.md`**

- **Why:** Android-server contract; endpoints with Visual field constraints.
- **Dependencies:** Data contracts, Visual whitelist
- **Done When:** Endpoints defined; error shapes standardized; cancellation supported.
- **Constitution:** Async/Await, Null Safety, Test-First.

8. **`docs/OFFLINE-SYNC-POLICY.md`**

- **Why:** Graceful connectivity handling; offline-first support.
- **Dependencies:** API spec, Visual whitelist
- **Done When:** Modes defined; queue implemented; conflict resolution rules clear.
- **Constitution:** Manufacturing Domain, Null Safety.

---

## Phase 4 — UX, Scanning, Roles

9. **`docs/ROLES-AND-PERMISSIONS.md`**

- **Why:** Authorization model; role-to-action mappings.
- **Dependencies:** Security, API spec
- **Done When:** Role mappings clear; approval workflows; Visual read-only enforced.
- **Constitution:** Manufacturing Domain, Security.

10. **`docs/UI-UX-GUIDELINES.md`**

- **Why:** Consistent operator experience; accessibility.
- **Dependencies:** Roles and permissions
- **Done When:** Touch targets, scan-first patterns, CompiledBinding in XAML, semantic tokens for theming.
- **Constitution:** Theme V2, CompiledBinding, Accessibility.

11. **`docs/BARCODE-AND-LABELING-STANDARDS.md`**

- **Why:** Reliable scanning/printing; GS1 parsing; label templates.
- **Dependencies:** Data contracts, UI/UX guidelines
- **Done When:** Label templates use semantic tokens; validation against Visual cache.
- **Constitution:** Manufacturing Domain, Theme V2.

---

## Phase 5 — Quality, Traceability, Support

12. **`docs/TEST-STRATEGY-AND-TRACEABILITY.md`**

- **Why:** Quality gates; AC-to-test mapping; Visual citation validation.
- **Dependencies:** All above
- **Done When:** xUnit tests for all features; >80% coverage; Spec-Kit traceability.
- **Constitution:** Test-First, Spec-Driven.

13. **`docs/TROUBLESHOOTING-CATALOG.md`**

- **Why:** Support efficiency; Visual-specific errors.
- **Dependencies:** All operational docs
- **Done When:** Common issues documented; resolution steps clear.
- **Constitution:** Documentation Standards.

14. **`docs/COPILOT-ASSETS-CHECKLIST.md`**

- **Why:** AI assistance consistency; Copilot instructions reflect Visual constraints.
- **Dependencies:** All documentation complete
- **Done When:** `copilot-instructions.md` matches constitution; templates reference specs.
- **Constitution:** Spec-Driven, Documentation.

15. **`docs/SPECIFY-CHECKLIST.md`**

- **Why:** Documentation generation consistency.
- **Dependencies:** All core docs
- **Done When:** `/specify` prompts validate Visual constraints; CSV citations automated.
- **Constitution:** Spec-Driven.

---

## Phase 6 — Feature Docs (run `/specify`)

16. **Generate feature specs under `docs/specs/features/*` using Spec-Kit.**

- Each spec must reference Visual schema with CSV line numbers.
- All Visual data access must cite Toolkit commands/pages.
- All ViewModels use MVVM Community Toolkit and CompiledBinding.
- All async methods support cancellation.
- All features have xUnit tests before implementation.

---

**All phases and documents must comply with the MTM Avalonia Template Constitution v1.1.0.**
