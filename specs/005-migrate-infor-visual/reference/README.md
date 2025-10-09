# Reference Files Index

**Date**: October 8, 2025

**Purpose**: Index and usage guide for all Feature 005 reference files

---

## Overview

This directory contains **6 categorized reference files** to support Feature 005 specification development. Each file serves a specific purpose and should be consulted during different phases of spec creation.

---

## Reference Files

### 1. REFERENCE-CLARIFICATIONS.md

**Purpose**: Complete record of 21 clarification questions and user's answers

**When to Use**:

- Understanding user's original intent
- Resolving ambiguities during spec writing
- Validating scope decisions
- Reviewing risk assessments

**Key Sections**:

- Feature Scope (all-in-one mega-feature confirmed)
- Debug Terminal Approach (complete rewrite, not refactor)
- Implementation Strategy (5 phases)
- Timeline & Risk Assessment (HIGH RISK items documented)

**Decision Summary**:

- **Approach**: All-in-one mega-feature (not split)
- **Debug Terminal**: Complete rewrite with SplitView navigation
- **Custom Controls**: Extract FIRST, then use in new UIs
- **Development Mode**: Radio Silence with autonomous PATCH output
- **Testing**: Test-first with 80%+ coverage

---

### 2. REFERENCE-EXISTING-PATTERNS.md

**Purpose**: Document established codebase patterns to follow

**When to Use**:

- Writing new ViewModels (MVVM patterns)
- Creating new XAML files (CompiledBinding patterns)
- Implementing services (DI patterns)
- Writing database queries (MySQL patterns)
- Creating tests (xUnit patterns)

**Key Sections**:

- MVVM Patterns (CommunityToolkit.Mvvm 8.4.0)
- Avalonia XAML Patterns (CompiledBinding examples)
- Current Repeated XAML Patterns (controls to extract)
- Configuration Service Patterns (Feature 002)
- Database Patterns (MySQL parameterized queries)
- Service Registration (DI)
- Error Handling Patterns
- Testing Patterns

**Code Examples**: Every section includes concrete examples from codebase

---

### 3. REFERENCE-CUSTOM-CONTROLS.md

**Purpose**: Catalog of custom controls to extract and create

**When to Use**:

- Planning custom control extraction phase
- Designing control APIs (properties, events, commands)
- Writing control documentation
- Determining implementation order

**Key Sections**:

- Extraction Threshold Rule (3+ occurrences)
- Controls to Extract (10 controls identified)
  - StatusCard (15+ occurrences)
  - MetricDisplay (20+ occurrences)
  - ErrorListPanel (5+ occurrences)
  - ConnectionHealthBadge (8+ occurrences)
  - BootTimelineChart (3+ occurrences)
  - SettingsCategory (new)
  - SettingRow (new)
  - NavigationMenuItem (new)
  - ActionButtonGroup (new)
  - ConfigurationErrorDialog (new)
- Control Library Structure
- Testing Strategy (80%+ coverage)
- Implementation Order (priority-based)

**Design Details**: Proposed API for each control with properties

---

### 4. REFERENCE-SETTINGS-INVENTORY.md

**Purpose**: Complete catalog of all application settings requiring UI

**When to Use**:

- Designing settings screen layout
- Implementing settings persistence
- Creating validation rules
- Planning settings categories

**Key Sections**:

- Configuration Architecture (3-tier precedence)
- Settings Categories (8 categories)
  - Visual ERP Integration (15 settings)
  - Database Configuration (7 settings)
  - Logging Configuration (8 settings)
  - UI Configuration (10 settings)
  - Cache Configuration (7 settings)
  - Performance Configuration (6 settings)
  - Feature Flags (6 flags)
  - User Folder Paths (5 paths)
- Settings Screen UI Design
- Validation Feedback
- Configuration Change Handling
- Settings Export/Import (JSON format)
- User Preference Persistence (MySQL + offline cache)

**Data Tables**: Each setting documented with key, type, default, description, validation

---

### 5. REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md

**Purpose**: Checklist of Constitution principles to validate against

**When to Use**:

- Writing acceptance criteria
- Validating implementation compliance
- Running constitutional audits
- Pre-PR checklist

**Key Sections**:

- Principle I: Spec-Driven Development
- Principle II: Nullable Reference Types
- Principle III: CompiledBinding Everywhere
- Principle IV: Test-Driven Development (TDD)
- Principle V: Performance Budgets
- Principle VI: Error Categorization
- Principle VII: Secrets Never Touch Code
- Principle VIII: Graceful Degradation
- Principle IX: Structured Logging
- Principle X: Dependency Injection
- Principle XI: Reusable Custom Controls
- Constitution TODOs (Feature 005 responsibilities)
- Feature 003 TODOs (DebugTerminalViewModel)
- Radio Silence Mode Compliance
- Validation Checklist Summary
- Constitutional Violations to Avoid

**Checklists**: Each principle has compliance checklist with examples

---

### 6. REFERENCE-VISUAL-API-SCOPE.md

**Purpose**: Define Visual ERP integration requirements and API scope

**When to Use**:

- Designing Visual API client
- Writing Visual integration tests
- Planning offline mode strategy
- Defining data contracts

**Key Sections**:

- Integration Overview (read-only, mobile-first)
- API Architecture (base config, client structure)
- Data Entities
  - Items (VisualItem contract)
  - Work Orders (VisualWorkOrder contract)
  - Inventory Transactions (VisualInventoryTransaction contract)
- Barcode Scanning Integration (scan workflows)
- Offline Mode Operation (cache strategy, sync queue)
- Error Handling (retry policies)
- Visual API Mock Service (testing without ERP access)
- Performance Requirements (targets for each operation)
- Testing Requirements (unit, integration, contract tests)
- Security & Compliance (GDPR, CCPA, SOC 2)
- Visual Integration Roadmap (4 phases)

**Data Contracts**: Complete C# records for all API responses

---

## How to Use These References

### During Specification Writing (SPEC_005.md)

1. **Start with**: `REFERENCE-CLARIFICATIONS.md` - Understand user's intent
2. **Consult**: `REFERENCE-SETTINGS-INVENTORY.md` - List all settings requiring UI
3. **Consult**: `REFERENCE-CUSTOM-CONTROLS.md` - List all controls to create
4. **Consult**: `REFERENCE-VISUAL-API-SCOPE.md` - Define Visual integration scope
5. **Validate against**: `REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md` - Ensure constitutional compliance

### During Planning (PLAN_005.md)

1. **Reference**: `REFERENCE-EXISTING-PATTERNS.md` - Follow established patterns
2. **Reference**: `REFERENCE-CUSTOM-CONTROLS.md` - Plan implementation order
3. **Reference**: `REFERENCE-SETTINGS-INVENTORY.md` - Design data models
4. **Reference**: `REFERENCE-VISUAL-API-SCOPE.md` - Design API client architecture
5. **Validate against**: `REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md` - Identify constitutional risks

### During Task Breakdown (TASKS_005.md)

1. **Use**: `REFERENCE-CUSTOM-CONTROLS.md` - Task per control (10 tasks)
2. **Use**: `REFERENCE-SETTINGS-INVENTORY.md` - Task per settings category (8 tasks)
3. **Use**: `REFERENCE-VISUAL-API-SCOPE.md` - Task per data entity (3 tasks)
4. **Validate against**: `REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md` - Task for each constitutional TODO

### During Implementation (Radio Silence Mode)

1. **Follow**: `REFERENCE-EXISTING-PATTERNS.md` - Code style and patterns
2. **Follow**: `REFERENCE-CUSTOM-CONTROLS.md` - Control API design
3. **Follow**: `REFERENCE-SETTINGS-INVENTORY.md` - Setting keys and validation
4. **Follow**: `REFERENCE-VISUAL-API-SCOPE.md` - API contracts and error handling
5. **Validate against**: `REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md` - Continuous compliance checking

### During Validation (Pre-PR)

1. **Run**: Constitutional audit script against `REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md`
2. **Verify**: All controls documented per `REFERENCE-CUSTOM-CONTROLS.md`
3. **Verify**: All settings implemented per `REFERENCE-SETTINGS-INVENTORY.md`
4. **Verify**: All Visual contracts implemented per `REFERENCE-VISUAL-API-SCOPE.md`

---

## Quick Reference: File Purpose Matrix

| File                                      | Spec Writing | Planning | Tasks | Implementation | Validation |
| ----------------------------------------- | ------------ | -------- | ----- | -------------- | ---------- |
| REFERENCE-CLARIFICATIONS.md               | ✅ Start here | ✅       | ✅    | ⚠️ As needed   | ✅         |
| REFERENCE-EXISTING-PATTERNS.md            | ⚠️ As needed | ✅       | ✅    | ✅ Primary     | ⚠️ As needed|
| REFERENCE-CUSTOM-CONTROLS.md              | ✅ Critical  | ✅       | ✅    | ✅ Primary     | ✅         |
| REFERENCE-SETTINGS-INVENTORY.md           | ✅ Critical  | ✅       | ✅    | ✅ Primary     | ✅         |
| REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md  | ✅ Validate  | ✅       | ✅    | ✅ Continuous  | ✅ Critical|
| REFERENCE-VISUAL-API-SCOPE.md             | ✅ Critical  | ✅       | ✅    | ✅ Primary     | ✅         |

**Legend**:

- ✅ Critical - Must consult
- ✅ Primary - Primary reference during this phase
- ⚠️ As needed - Consult when relevant

---

## File Sizes & Complexity

| File                                      | Lines | Complexity | Read Time |
| ----------------------------------------- | ----- | ---------- | --------- |
| REFERENCE-CLARIFICATIONS.md               | ~200  | Low        | 5 min     |
| REFERENCE-EXISTING-PATTERNS.md            | ~400  | Medium     | 10 min    |
| REFERENCE-CUSTOM-CONTROLS.md              | ~450  | Medium     | 12 min    |
| REFERENCE-SETTINGS-INVENTORY.md           | ~400  | High       | 15 min    |
| REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md  | ~550  | High       | 20 min    |
| REFERENCE-VISUAL-API-SCOPE.md             | ~500  | High       | 18 min    |
| **Total**                                 | ~2500 | -          | ~80 min   |

---

## Update History

- **2025-10-08**: Initial creation of all 6 reference files
- **Next Update**: After Feature 005 spec completion (add lessons learned)

---

## Related Files

- **Main Prompt**: `../RESTART-PROMPT.md` (use with `/speckit.specify` command)
- **Constitution**: `../../.specify/memory/constitution.md` (v1.1.0)
- **Feature Docs**: `../../../docs/` (various feature documentation)
- **Existing Specs**: `../../001-boot-sequence-splash/`, `../../002-environment-and-configuration/`, `../../003-debug-terminal-modernization/`

---

## Notes for AI Agents

When using these reference files:

1. **Read REFERENCE-CLARIFICATIONS.md FIRST** - Understand user's intent before diving into details
2. **Use REFERENCE-EXISTING-PATTERNS.md as code style guide** - Always follow established patterns
3. **Treat REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md as checklist** - Validate against it continuously
4. **Use REFERENCE-CUSTOM-CONTROLS.md and REFERENCE-SETTINGS-INVENTORY.md as authoritative catalogs** - Don't invent new controls/settings not listed
5. **Follow REFERENCE-VISUAL-API-SCOPE.md data contracts exactly** - Don't deviate from defined schemas

---

## Success Criteria

Feature 005 specification is complete when:

- [ ] All 6 reference files consulted during spec writing
- [ ] All constitutional requirements addressed in spec
- [ ] All custom controls (10) documented in spec
- [ ] All settings (60+) documented in spec
- [ ] All Visual API contracts (3) documented in spec
- [ ] All TODOs from Constitution and Feature 003 addressed
- [ ] Validation script passes with 100% task completion
- [ ] Constitutional audit passes with 100% compliance
