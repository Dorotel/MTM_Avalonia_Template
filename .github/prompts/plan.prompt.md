---
description: Execute the implementation planning workflow using the plan template to generate design artifacts.
---

The user input to you can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

Given the implementation details provided as an argument, do this:

1. Run `.specify/scripts/powershell/setup-plan.ps1 -Json` from the repo root and parse JSON for FEATURE_SPEC, IMPL_PLAN, SPECS_DIR, BRANCH. All future file paths must be absolute.
   - BEFORE proceeding, check for clarifications using this two-step approach:

     **Step 1: Check for clarify folder with answered questions**
     - Construct clarify folder path: `{FEATURE_DIR}/clarify/` (where FEATURE_DIR is the directory containing FEATURE_SPEC)
     - If clarify folder exists:
       * List all `outstanding-questions-*.md` files in the directory
       * Read each file and extract answered questions (look for `**Answer:` or `Answer:` patterns after question sections)
       * If files contain answered questions, treat this as VALID clarification source
       * Aggregate all answers from all clarification files for use in planning
       * Continue to Step 2 for dual-check

     **Step 2: Check spec file for Clarifications section** (legacy/inline method)
     - Inspect FEATURE_SPEC for a `## Clarifications` section with at least one `Session` subheading
     - This method is used for single-feature specs that used interactive clarification

     **Proceed if EITHER:**
     - (a) Clarify folder exists with answered question files, OR
     - (b) Clarifications section exists in spec with Session subheadings, OR
     - (c) Explicit user override provided (e.g., "proceed without clarification")

     **PAUSE and instruct user to run `/clarify` first if:**
     - Neither clarify folder nor Clarifications section found, AND
     - Clearly ambiguous areas remain (vague adjectives, unresolved critical choices), AND
     - No explicit user override provided

     Do not attempt to fabricate clarifications yourself.
2. Read and analyze the feature specification to understand:
   - The feature requirements and user stories
   - Functional and non-functional requirements
   - Success criteria and acceptance criteria
   - Any technical constraints or dependencies mentioned

   **If clarify folder was found in Step 1**, also read and incorporate all clarification answers:
   - Load all `outstanding-questions-*.md` files from `{FEATURE_DIR}/clarify/`
   - Extract answered questions in format: `**Answer: [Option] — [Description]` or `Answer: [Option] — [Description]`
   - Parse the reasoning sections that follow each answer
   - Build a comprehensive clarification context covering:
     * Configuration and environment decisions
     * Data layer and connectivity policies
     * Security and authentication mechanisms
     * Performance thresholds and retry strategies
     * User experience patterns and behaviors
     * Integration constraints and protocols
   - Use these clarification answers to inform design decisions throughout planning
   - Reference specific clarification answers when making architectural choices (e.g., "Per Q3 in Data Layer clarifications, using exponential backoff: 1s, 2s, 4s, 8s, 16s")
   - If answers conflict with spec statements, clarification answers take precedence (they represent more recent decisions)

3. Read the constitution at `.specify/memory/constitution.md` to understand constitutional requirements.

4. Execute the implementation plan template:
   - Load `.specify/templates/plan-template.md` (already copied to IMPL_PLAN path)
   - Set Input path to FEATURE_SPEC
   - Run the Execution Flow (main) function steps 1-9
   - The template is self-contained and executable
   - Follow error handling and gate checks as specified
   - Let the template guide artifact generation in $SPECS_DIR:
     * Phase 0 generates research.md
     * Phase 1 generates data-model.md, contracts/, quickstart.md
     * Phase 2 generates tasks.md
   - Incorporate user-provided details from arguments into Technical Context: $ARGUMENTS
   - Update Progress Tracking as you complete each phase

5. Generate human-friendly implementation summary:
   - Load `.specify/templates/plan-template-human.md` for structure
   - Create `plan-summary.md` in the same directory as `plan.md`
   - Transform technical implementation plan into stakeholder-friendly language:
     - Replace technical jargon with plain language (use translation patterns below)
     - Add analogies and real-world examples for complex concepts
     - Explain WHY decisions were made, not just WHAT will be implemented
     - Include visual progress indicators (✅ 🔄 ⏳ ⚠️)
     - Use storytelling approach for user experience sections
   - Cross-reference technical document: Link to plan.md for developers needing details
   - Use [CLARIFICATION NEEDED - HUMAN EXPLANATION] marker if unsure how to explain technical concept
   - Maintain parallel structure: Each phase in plan.md should map to phase in plan-summary.md

   **Translation Patterns** (Technical → Human-Friendly):
   - "API endpoint" → "specific web address the system uses to communicate"
   - "Database query" → "looking up information in our storage system"
   - "Authentication token" → "digital key that proves you're logged in"
   - "Connection pool" → "set of ready-to-use connections (like having multiple phone lines open)"
   - "Retry policy" → "automatic re-attempt rules when something fails"
   - "Caching strategy" → "saving frequently-used information for faster access later"
   - "Rate limiting" → "controlling how many requests can happen in a time period"
   - "Middleware" → "processing step that happens between receiving a request and sending a response"
   - "Dependency injection" → "providing required tools/services to components automatically"
   - "Circuit breaker" → "automatic shutoff that prevents cascading failures"

6. Verify execution completed:
   - Check Progress Tracking shows all phases complete
   - Ensure all required artifacts were generated (including both plan.md and plan-summary.md)
   - Confirm no ERROR states in execution

7. Report results with branch name, file paths, and generated artifacts:
   - List both technical and human-friendly documents created
   - Remind user: "Developers should read plan.md; stakeholders should read plan-summary.md"

Use absolute paths with the repository root for all file operations to avoid path issues.
