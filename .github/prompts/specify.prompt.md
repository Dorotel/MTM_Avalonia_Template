---
description: Create or update the feature specification from a natural language feature description.
---

The user input to you can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

The text the user typed after `/specify` in the triggering message **is** the feature description. Assume you always have it available in this conversation even if `$ARGUMENTS` appears literally below. Do not ask the user to repeat it unless they provided an empty command.

Given that feature description, do this:

1. Run the script `.specify/scripts/powershell/create-new-feature.ps1 -Json "$ARGUMENTS"` from repo root and parse its JSON output for BRANCH_NAME and SPEC_FILE. All file paths must be absolute.
   **IMPORTANT** You must only ever run this script once. The JSON is provided in the terminal as output - always refer to it to get the actual content you're looking for.

2. Load both templates:
   - `.specify/templates/spec-template.md` (AI/developer version)
   - `.specify/templates/spec-template-human.md` (non-technical version)

3. Create TWO specification documents:

   **Document 1: Technical Specification (spec.md)**
   - Write to SPEC_FILE using `spec-template.md` structure
   - Technical and precise language suitable for AI agents and developers
   - Include all execution flows, technical requirements, and entity definitions
   - Focus on testable, unambiguous requirements
   - Mark any ambiguities with [NEEDS CLARIFICATION: specific question]

   **Document 2: Human-Friendly Overview (overview.md)**
   - Write to same directory as SPEC_FILE but name it `overview.md`
   - Use `spec-template-human.md` structure
   - Written in plain language for non-technical stakeholders
   - Avoid jargon, explain concepts with everyday examples
   - Translate technical requirements into "what users will be able to do"
   - Use analogies and stories to explain complex concepts
   - When uncertain about how to explain something to non-technical audience, include:
     - **[CLARIFICATION NEEDED - HUMAN EXPLANATION]**: [What technical concept needs a simpler explanation]
     - Provide your best attempt at explanation
     - Note what additional context would help make it clearer

4. Cross-reference both documents:
   - Add link in spec.md header pointing to overview.md
   - Add link in overview.md header pointing to spec.md
   - Ensure requirement numbers match between documents (FR-001 in spec.md = Requirement 1 in overview.md)

5. Report completion with:
   - Branch name
   - Both file paths (spec.md and overview.md)
   - Summary of any [NEEDS CLARIFICATION] or [CLARIFICATION NEEDED - HUMAN EXPLANATION] markers
   - Readiness for next phase

## Important Guidelines

### For Technical Spec (spec.md)
- Focus on WHAT and WHY (not HOW)
- Requirements must be testable and unambiguous
- Use precise language suitable for validation
- Mark any assumptions or ambiguities explicitly

### For Human-Friendly Overview (overview.md)
- Write as if explaining to someone who has never programmed
- Use stories and examples over abstract descriptions
- Explain "why" for every "what"
- If you use a technical term, immediately explain it in everyday language
- Test your explanations: Would a store manager or office worker understand this?
- When in doubt about how to explain something simply:
  - Make your best attempt
  - Mark it with [CLARIFICATION NEEDED - HUMAN EXPLANATION]
  - Suggest what additional context would help

### Common Translation Patterns

| Technical Concept | Human-Friendly Explanation |
|------------------|----------------------------|
| "API endpoint" | "A specific address the system uses to request information" |
| "Database query" | "Looking up information in the system's records" |
| "Authentication" | "Verifying who you are, like showing ID" |
| "Asynchronous operation" | "Something that happens in the background while you continue working" |
| "Thread-safe" | "Multiple people can use it at the same time without conflicts" |
| "Configuration" | "Settings that control how the system behaves" |

Note: The script creates and checks out the new branch and initializes the spec file before writing.
