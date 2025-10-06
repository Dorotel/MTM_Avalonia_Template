---
description: Identify underspecified areas in the current feature spec by asking up to 5 highly targeted clarification questions and encoding answers back into the spec.
---

# Clarify Prompt Workflow

The user input to you can be prov- For the first integrated answer in this session:
- Ensure a "## Clarifications" section exists (create it just after the highest-level contextual/overview section if missing).
- Under it, create (if not present) an "### Index" subheading listing clarifications by status (Resolved, Open).
- Example Index format: `**Resolved (Session YYYY-MM-DD)**: CL-001 (Short Label), CL-002 (Short Label), ...`
- For each accepted answer, create a detailed clarification entry using this format:

```markdown
### CL-###: [Short Descriptive Title]
- **Status**: Answered (YYYY-MM-DD)
- **Question**: [Full question text with context]
- **Answer/Decision**: [Final answer with rationale explaining why this option was chosen]
- **Spec Changes**: [Specific list of sections/requirements updated, e.g., "Updated FR-013 to include credential recovery dialog requirement"]
```

- Update the Index section to reference this clarification in the Resolved list.
- Then immediately apply the clarification to the most appropriate sections:irectly by ### 6) Validation (Each Write + Final Pass)

- Clarifications section contains detailed entries with Status, Question, Answer/Decision, and Spec Changes for each accepted answer (no duplicates).
- Index section accurately lists all resolved and open clarifications.
- Total asked (accepted) questions ≤ 5 in single-feature mode.
- Updated sections contain no lingering placeholders the new answer was meant to resolve.
- No contradictory earlier statements remain.
- Markdown structure valid; allowed new headings: "## Clarifications", "### Index", "### CL-###: [Title]".
- Terminology consistency maintained.t or as a command argument — you MUST consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

Goal: Detect and reduce ambiguity or missing decision points in the active feature specification and record the clarifications directly in the spec file.

Note: This clarification workflow is expected to run (and be completed) BEFORE invoking `/plan`. If the user explicitly states they are skipping clarification (e.g., exploratory spike), you may proceed, but must warn that downstream rework risk increases.

## Execution Steps

### 1) Prerequisites and Blocked Clarifications

- Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly` from the repository root once (combined `-Json -PathsOnly` mode).
- Parse minimal JSON payload fields:
  - FEATURE_DIR
  - FEATURE_SPEC
  - Optionally capture IMPL_PLAN, TASKS for future chained flows
- If JSON parsing fails, abort and instruct the user to re-run `/specify` or verify the feature branch environment.

Check for existing clarification-needed file:
- If `{FEATURE_DIR}/clarification-needed.md` exists:
  - Load the file and extract the blocked context (which command generated it, what progress was made, what clarification is needed).
  - Present the clarification questions from that file to the user interactively.
  - Once answered, integrate answers into the spec.md.
  - Rename `clarification-needed.md` to `clarification-needed-complete.md` to mark resolution.
  - Report completion and suggest re-running the original blocked command (e.g., `/tasks` or `/implement`).
  - Exit clarification workflow (do not proceed with normal ambiguity scanning).

### 2) Load Spec and Detect Multi-Feature Mode

- Load the current spec file.
- Detect if the spec contains multiple distinct features by scanning for:
  - Multiple top-level “Feature Specification:” headings, OR
  - Multiple major feature sections enumerated in the document (e.g., numbered feature lists like “1. Splash Screen, 2. Configuration Service, 3. Logging Service”), OR
  - Explicit statement that this is a multi-feature specification (phrases like “Features/Implementations Covered by This Document”).
- If multiple features detected:
  - Set mode to Multi-Feature Clarification Mode (UNLIMITED questions).
  - For each distinct feature, create a separate clarification file:
    - `{FEATURE_DIR}/clarify/outstanding-questions-[feature-name]-{YYYY-MM-DD}.md`
  - Generate comprehensive question sets for each feature (write to files only; no interactive questioning).
  - After creating all files, provide a summary table with question counts per feature.
- If a single feature detected:
  - Proceed with single-feature mode (question limit applies).

Perform a structured ambiguity and coverage scan using this taxonomy. Mark each category internally as Clear / Partial / Missing to prioritize questions (do not output the raw map unless no questions will be asked):

- Functional Scope & Behavior
  - Core user goals & success criteria
  - Explicit out-of-scope declarations
  - User roles / personas differentiation
- Domain & Data Model
  - Entities, attributes, relationships
  - Identity & uniqueness rules
  - Lifecycle/state transitions
  - Data volume / scale assumptions
- Interaction & UX Flow
  - Critical user journeys / sequences
  - Error/empty/loading states
  - Accessibility or localization notes
- Non-Functional Quality Attributes
  - Performance (latency, throughput targets)
  - Scalability (horizontal/vertical, limits)
  - Reliability & availability (uptime, recovery expectations)
  - Observability (logging, metrics, tracing signals)
  - Security & privacy (authN/Z, data protection, threat assumptions)
  - Compliance / regulatory constraints (if any)
- Integration & External Dependencies
  - External services/APIs and failure modes
  - Data import/export formats
  - Protocol/versioning assumptions
- Edge Cases & Failure Handling
  - Negative scenarios
  - Rate limiting / throttling
  - Conflict resolution (e.g., concurrent edits)
- Constraints & Tradeoffs
  - Technical constraints (language, storage, hosting)
  - Explicit tradeoffs or rejected alternatives
- Terminology & Consistency
  - Canonical glossary terms
  - Avoided synonyms / deprecated terms
- Completion Signals
  - Acceptance criteria testability
  - Measurable “Definition of Done” indicators
- Misc / Placeholders
  - TODO markers / unresolved decisions
  - Ambiguous adjectives (“robust”, “intuitive”) lacking quantification

Add a candidate question for any category marked Partial or Missing unless:
- Clarification would not materially change implementation or validation strategy, or
- Information is better deferred to the planning phase (note internally).

### 3) Build Prioritized Question Queue

CRITICAL: Use plain, non-technical business language. Focus on user-observable behavior and business outcomes. Avoid developer jargon (“API contract”, “throw exception”, “async methods”, “return values”, etc.).

- Multi-Feature Mode:
  - Unlimited questions per feature.
  - Each question must be answerable with either:
    - A short multiple-choice selection (5–10 distinct, mutually exclusive options), or
    - A one-word/short-phrase answer (“Answer in <=5 words”).
  - Group by feature and taxonomy category.
  - Write all questions to files (no interactive questioning).
- Single-Feature Mode:
  - Maximum of 10 total questions generated; interactive ask limit is 5.
  - Each question must be answerable with either:
    - A short multiple-choice selection (5–10 distinct, mutually exclusive options), or
    - A one-word/short-phrase answer (“Answer in <=5 words”).
  - Only include questions whose answers materially impact architecture, data modeling, task decomposition, test design, UX behavior, operational readiness, or compliance validation.
  - Ensure category coverage balance; prioritize high-impact unresolved areas.
  - If more than 5 questions are generated, write all questions to `{FEATURE_DIR}/clarify/outstanding-questions-{YYYY-MM-DD}.md`, inform the user, and exit (no interactive questions).

### 4) Sequential Questioning Loop

Multi-Feature Mode pre-check:
- Create directory `{FEATURE_DIR}/clarify/` if it doesn’t exist.
- For each feature, create a file using this format:

```markdown
# Outstanding Clarification Questions: [Feature Name] - {YYYY-MM-DD}

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **[Feature Name]** feature.

Note: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions ({N} total)

### Q1: [Category] — [Question text]
Impact: [High/Medium/Low]
Category: [Functional Scope/Data Model/etc.]
Feature Context: [Specific feature/subsystem this applies to]

[For multiple choice:]
| Option | Description |
| -----: | ----------- |
|      A | …           |
|      B | …           |
|      C | …           |
|      D | …           |
|      E | …           |

[For short answer:]
Format: Short answer (<=5 words)

---
[Repeat for each question]
```

- After creating all files, inform the user with a summary table and exit (no interactive questioning):

```
Multi-feature specification detected. Generated clarification files:

| Feature     | Questions | File Path                                                               |
| ----------- | --------- | ----------------------------------------------------------------------- |
| [Feature 1] | {N}       | {FEATURE_DIR}/clarify/outstanding-questions-[feature-1]-{YYYY-MM-DD}.md |
| [Feature 2] | {N}       | {FEATURE_DIR}/clarify/outstanding-questions-[feature-2]-{YYYY-MM-DD}.md |
| …           | …         | …                                                                       |

Total: {X} features, {Y} total questions

Please review each file and address high-priority questions in the spec, then re-run `/clarify` to continue.
```

Single-Feature Mode pre-loop check:
- If the prioritized queue contains MORE than 5 questions:
  - Create `{FEATURE_DIR}/clarify/outstanding-questions-{YYYY-MM-DD}.md` with this format:

```markdown
# Outstanding Clarification Questions — {YYYY-MM-DD}

Generated from feature specification analysis. Review and address high-priority items, then re-run `/clarify`.

## Questions

### Q1: [Category] — [Question text]
Impact: [High/Medium/Low]
Category: [Functional Scope/Data Model/etc.]

[For multiple choice:]
| Option | Description |
| -----: | ----------- |
|      A | …           |
|      B | …           |
|      C | …           |
|      D | …           |
|      E | …           |

[For short answer:]
Format: Short answer (<=5 words)

---
[Repeat for each question]
```

- Inform the user: “Generated {N} clarification questions (exceeds interactive limit of 5). Created file at: {filepath}. Please review and address high-priority questions in the spec, then re-run `/clarify` to continue.”
- Exit without interactive questioning.

Interactive loop (only when 5 or fewer questions):
- Present exactly one question at a time.
- Start with a concrete example scenario explaining why this matters:

```
📋 Example Scenario: [1–2 sentences describing a real-world situation where this ambiguity would cause problems or confusion]
```

- For multiple choice, render options as a table:

| Option | Description                                  |
| -----: | -------------------------------------------- |
|      A | Option A description                         |
|      B | Option B description                         |
|      C | Option C description                         |
|      D | Option D description                         |
|      E | Option E description                         |
|  Short | Provide a different short answer (<=5 words) |

- After the options table, provide your recommendation:

Bold recommendation:
- Recommendation: [Option Letter] — [1–2 sentence explanation of why this option best balances user experience, reliability, and maintainability]

- For short-answer style (no meaningful discrete options), output a single line after the question:
  - Format: Short answer (<=5 words).
- After the user answers:
  - Validate the answer maps to one option or fits the <=5 word constraint.
  - If ambiguous, ask for a quick disambiguation (counts as the same question; do not advance).
  - Once satisfactory, record it in working memory (do not yet write to disk) and move to the next queued question.
- Stop asking when:
  - All critical ambiguities are resolved, OR
  - The user signals completion (“done”, “good”, “no more”), OR
  - You reach 5 asked questions.
- Never reveal future queued questions in advance.
- If no valid questions exist at the start, immediately report no critical ambiguities.

### 5) Integration After Each Accepted Answer

- Maintain an in-memory representation of the spec (loaded once) plus the raw file contents.
- For the first integrated answer in this session:
  - Ensure a “## Clarifications” section exists (create it just after the highest-level contextual/overview section if missing).
  - Under it, create (if not present) a “### Session YYYY-MM-DD” subheading for today.
- Append a bullet line immediately after acceptance:
  - `- Q: <question> → A: <final answer>`
- Then immediately apply the clarification to the most appropriate sections:
  - Functional ambiguity → Update Functional Requirements.
  - User interaction/actor distinction → Update User Stories or Actors.
  - Data shape/entities → Update Data Model (fields, types, relationships, constraints).
  - Non-functional constraint → Update Quality Attributes with measurable criteria.
  - Edge case/negative flow → Add under Edge Cases / Error Handling.
  - Terminology conflict → Normalize the term across the spec; keep prior term once with “(formerly referred to as ‘X’)”.
- If the clarification invalidates earlier ambiguous text, replace it (do not duplicate).
- Save the spec file after each integration (atomic overwrite).
- Preserve formatting: do not reorder unrelated sections; keep heading hierarchy intact.
- Keep each inserted clarification minimal and testable.

### 6) Validation (Each Write + Final Pass)

- Clarifications session contains exactly one bullet per accepted answer (no duplicates).
- Total asked (accepted) questions ≤ 5 in single-feature mode.
- Updated sections contain no lingering placeholders the new answer was meant to resolve.
- No contradictory earlier statements remain.
- Markdown structure valid; only allowed new headings: “## Clarifications”, “### Session YYYY-MM-DD”.
- Terminology consistency maintained.

### 7) Write Updated Spec

- Write the updated content back to FEATURE_SPEC.

### 8) Report Completion

Report:
- Number of questions asked and answered.
- Path to the updated spec.
- Sections touched (list names).
- Coverage summary table listing each taxonomy category with Status:
  - Resolved (was Partial/Missing and addressed),
  - Deferred (exceeds question quota or better suited for planning),
  - Clear (already sufficient),
  - Outstanding (still Partial/Missing but low impact).
- If any Outstanding or Deferred remain, recommend whether to proceed to `/plan` or run `/clarify` again later post-plan.
- Suggest the next command.

## Behavior Rules

- If no meaningful ambiguities found (or all potential questions are low-impact), respond:
  - “No critical ambiguities detected worth formal clarification.” and suggest proceeding.
- If spec file is missing, instruct the user to run `/specify` first (do not create a new spec here).
- Multi-Feature Mode: Question limit is UNLIMITED; generate comprehensive question sets and separate files per feature.
- Single-Feature Mode: Never exceed 5 total asked questions (retries for a single question do not count as new questions).
- If more than 5 questions are generated in single-feature mode, create the outstanding questions file and exit instead of asking interactively.
- Avoid speculative tech stack questions unless absence blocks functional clarity.
- Respect user early termination signals (“stop”, “done”, “proceed”).
- If no questions asked due to full coverage, output a compact coverage summary (all categories Clear) then suggest advancing.
- If the interactive quota is reached with unresolved high-impact categories remaining, explicitly flag them under Deferred with rationale.

### Multi-Feature Detection Criteria

- Enumerated lists of features/implementations in overview sections.
- Phrases like “Features/Implementations Covered by This Document”.
- Documents with 10+ distinct functional requirement groupings that represent separate subsystems.
- When in doubt, treat large specifications (100+ requirements spanning multiple subsystems) as multi-feature.

---

Context for prioritization: $ARGUMENTS
