# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 3.1: Setup
- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools

## Phase 3.1a: Database Schema Documentation *(if feature modifies database)*
**CRITICAL: Complete BEFORE writing any database code**
- [ ] T004 Read `.github/mamp-database/schema-tables.json` to verify existing schema
- [ ] T005 Document new/modified tables in spec (exact names, columns, types, constraints)
- [ ] T006 Update `.github/mamp-database/schema-tables.json` with new table definitions
- [ ] T007 Update `.github/mamp-database/indexes.json` with performance indexes
- [ ] T008 Increment version in `.github/mamp-database/migrations-history.json`
- [ ] T009 Verify schema accuracy (case-sensitive table/column names match JSON)

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T010 [P] Contract test POST /api/users in tests/contract/test_users_post.py
- [ ] T011 [P] Contract test GET /api/users/{id} in tests/contract/test_users_get.py
- [ ] T012 [P] Integration test user registration in tests/integration/test_registration.py
- [ ] T013 [P] Integration test auth flow in tests/integration/test_auth.py

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T014 [P] User model in src/models/user.py
- [ ] T015 [P] UserService CRUD in src/services/user_service.py
- [ ] T016 [P] CLI --create-user in src/cli/user_commands.py
- [ ] T017 POST /api/users endpoint
- [ ] T018 GET /api/users/{id} endpoint
- [ ] T019 Input validation
- [ ] T020 Error handling and logging

## Phase 3.4: Integration
- [ ] T021 Connect UserService to DB
- [ ] T022 Auth middleware
- [ ] T023 Request/response logging
- [ ] T024 CORS and security headers

## Phase 3.5: Polish
- [ ] T025 [P] Unit tests for validation in tests/unit/test_validation.py
- [ ] T026 Performance tests (<200ms)
- [ ] T027 [P] Update docs/api.md
- [ ] T028 Remove duplication
- [ ] T029 Run manual-testing.md

## Phase 3.6: Database Documentation Audit *(if database modified)*
- [ ] T030 Run `mamp-database-sync.prompt.md` audit to verify accuracy
- [ ] T031 Verify `lastUpdated` timestamps are current in all modified JSON files
- [ ] T032 Confirm schema changes match actual database structure

## Dependencies
- Database schema docs (T004-T009) before tests (T010-T013)
- Tests (T010-T013) before implementation (T014-T020)
- T014 blocks T015, T021
- T022 blocks T024
- Implementation before polish (T025-T029)
- Database audit (T030-T032) after all database code complete

## Parallel Example
```
# Launch T010-T013 together (contract tests):
Task: "Contract test POST /api/users in tests/contract/test_users_post.py"
Task: "Contract test GET /api/users/{id} in tests/contract/test_users_get.py"
Task: "Integration test registration in tests/integration/test_registration.py"
Task: "Integration test auth in tests/integration/test_auth.py"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task

2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks

3. **From Database Schema Changes** (if applicable):
   - Read `.github/mamp-database/schema-tables.json` → reference task
   - Each table created/modified → schema update task
   - After database changes → JSON file update task
   - Increment version in `migrations-history.json` → versioning task

4. **From User Stories**:
   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

5. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Database documentation BEFORE implementation
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task
- [ ] Database schema changes documented in `.github/mamp-database/` (if applicable)
- [ ] Database audit tasks included (if database modified)
