---
description: Generate an actionable, dependency-ordered tasks.md for the feature based on available design artifacts.
---

The user input to you can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

1. Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json` from repo root and parse FEATURE_DIR and AVAILABLE_DOCS list. All paths must be absolute.
2. Load and analyze available design documents:
   - Always read plan.md for tech stack and libraries
   - IF EXISTS: Read data-model.md for entities
   - IF EXISTS: Read contracts/ for API endpoints
   - IF EXISTS: Read research.md for technical decisions
   - IF EXISTS: Read quickstart.md for test scenarios
   - **IF EXISTS: Read clarify/ folder** for clarification decisions:
     * Check for `{FEATURE_DIR}/clarify/outstanding-questions-*.md` files
     * Extract answered questions (look for `**Answer:` or `Answer:` patterns)
     * Use clarification answers to inform task breakdown and validation criteria
     * Example: Retry policy clarifications inform error handling tasks
     * Example: Performance threshold clarifications inform testing tasks

   Note: Not all projects have all documents. For example:
   - CLI tools might not have contracts/
   - Simple libraries might not need data-model.md
   - Generate tasks based on what's available

3. Generate tasks following the template:
   - Use `.specify/templates/tasks-template.md` as the base
   - Replace example tasks with actual tasks based on:
     * **Setup tasks**: Project init, dependencies, linting
     * **Test tasks [P]**: One per contract, one per integration scenario
     * **Core tasks**: One per entity, service, CLI command, endpoint
     * **Integration tasks**: DB connections, middleware, logging
     * **Polish tasks [P]**: Unit tests, performance, docs

4. Task generation rules:
   - Each contract file → contract test task marked [P]
   - Each entity in data-model → model creation task marked [P]
   - Each endpoint → implementation task (not parallel if shared files)
   - Each user story → integration test marked [P]
   - Different files = can be parallel [P]
   - Same file = sequential (no [P])

5. Order tasks by dependencies:
   - Setup before everything
   - Tests before implementation (TDD)
   - Models before services
   - Services before endpoints
   - Core before integration
   - Everything before polish

6. Include parallel execution examples:
   - Group [P] tasks that can run together
   - Show actual Task agent commands

7. Create FEATURE_DIR/tasks.md with:
   - Correct feature name from implementation plan
   - Numbered tasks (T001, T002, etc.)
   - Clear file paths for each task
   - Dependency notes
   - Parallel execution guidance

8. Generate human-friendly task summary:
   - Load `.specify/templates/tasks-template-human.md` for structure
   - Create `tasks-summary.md` in the same directory as `tasks.md`
   - Transform technical task list into project management format:
     - Group tasks by phase with plain-language goals
     - Add "What it means" explanations for each task
     - Include time estimates and "What you'll see when done" outcomes
     - Show progress tracking with visual indicators (✅ 🔄 ⏳ ⚠️)
     - Add weekly timeline view for status updates
     - Include blocked tasks section with unblock dates
   - Cross-reference technical document: Link to tasks.md for developers
   - Use [CLARIFICATION NEEDED - HUMAN EXPLANATION] marker if task purpose is unclear
   - Maintain parallel numbering: Task T001 in tasks.md = Task 1 in tasks-summary.md

   **Translation Patterns** (Technical → Human-Friendly):
   - "Implement ConfigurationService" → "Create the system that manages settings"
   - "Write unit tests for OrderValidator" → "Test that order checking works correctly"
   - "Set up CI/CD pipeline" → "Set up automatic building and testing"
   - "Integrate with Visual API" → "Connect our system to the Visual software"
   - "Optimize database queries" → "Make data lookups faster"
   - "Implement error handling" → "Make system handle problems gracefully"
   - "Add logging" → "Add diagnostic messages for troubleshooting"
   - "Configure connection pooling" → "Set up efficient database connections"
   - "Deploy to staging environment" → "Install on test system"
   - "Perform load testing" → "Test if system stays fast with many users"

Context for task generation: $ARGUMENTS

The tasks.md should be immediately executable - each task must be specific enough that an LLM can complete it without additional context. The tasks-summary.md should be readable by non-technical stakeholders tracking project progress.
