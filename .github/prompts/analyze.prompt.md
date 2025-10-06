---
description: Perform a non-destructive cross-artifact consistency and quality analysis across spec.md, plan.md, and tasks.md after task generation. Outputs comprehensive report to {FEATURE_DIR}/AnalysisReport.md.
---

The user input to you can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

Goal: Identify inconsistencies, duplications, ambiguities, and underspecified items across the three core artifacts (`spec.md`, `plan.md`, `tasks.md`) before implementation. This command MUST run only after `/tasks` has successfully produced a complete `tasks.md`. Analysis report is written to `{FEATURE_DIR}/AnalysisReport.md`.

Output Behavior: The analysis generates a comprehensive Markdown report saved to `{FEATURE_DIR}/AnalysisReport.md` (creating or overwriting if exists). A summary is output to chat with link to full report. The report file becomes the source of truth for all findings, coverage statistics, and remediation recommendations.

STRICTLY READ-ONLY (with report output): Do **not** modify spec.md, plan.md, or tasks.md files. Write findings to AnalysisReport.md. Offer optional remediation plan (user must explicitly approve before any follow-up editing).

Constitution Authority: The project constitution (`.specify/memory/constitution.md`) is **non-negotiable** within this analysis scope. Constitution conflicts are automatically CRITICAL and require adjustment of the spec, plan, or tasks—not dilution, reinterpretation, or silent ignoring of the principle. If a principle itself needs to change, that must occur in a separate, explicit constitution update outside `/analyze`.

Execution steps:

1. Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` once from repo root and parse JSON for FEATURE_DIR and AVAILABLE_DOCS. Derive absolute paths:
   - SPEC = FEATURE_DIR/spec.md
   - PLAN = FEATURE_DIR/plan.md
   - TASKS = FEATURE_DIR/tasks.md
   Abort with an error message if any required file is missing (instruct the user to run missing prerequisite command).

   **HALT CHECK**: If `{FEATURE_DIR}/clarification-needed.md` exists (without `-complete.md` suffix), STOP immediately. Inform user: "Cannot proceed with /analyze - clarification needed. Run /clarify to resolve blocking questions first." Do not perform analysis.

2. Load artifacts:
   - Parse spec.md sections: Overview/Context, Functional Requirements, Non-Functional Requirements, User Stories, Edge Cases (if present).
   - Parse plan.md: Architecture/stack choices, Data Model references, Phases, Technical constraints.
   - Parse tasks.md: Task IDs, descriptions, phase grouping, parallel markers [P], referenced file paths.
   - Load constitution `.specify/memory/constitution.md` for principle validation.
   - **IF EXISTS**: Load clarify/ folder for clarification validation:
     - Check for `{FEATURE_DIR}/clarify/outstanding-questions-*.md` files
     - Extract all answered questions (look for `**Answer:` or `Answer:` patterns)
     - Build clarification decision inventory for cross-reference
     - Validate that clarification answers are reflected in spec.md and plan.md

3. Build internal semantic models:
   - Requirements inventory: Each functional + non-functional requirement with a stable key (derive slug based on imperative phrase; e.g., "User can upload file" -> `user-can-upload-file`).
   - User story/action inventory.
   - Task coverage mapping: Map each task to one or more requirements or stories (inference by keyword / explicit reference patterns like IDs or key phrases).
   - Constitution rule set: Extract principle names and any MUST/SHOULD normative statements.

4. Detection passes:
   A. Duplication detection:
      - Identify near-duplicate requirements. Mark lower-quality phrasing for consolidation.
   B. Ambiguity detection:
      - Flag vague adjectives (fast, scalable, secure, intuitive, robust) lacking measurable criteria.
      - Flag unresolved placeholders (TODO, TKTK, ???, <placeholder>, etc.).
      - **IF clarify/ EXISTS**: Check if ambiguous areas have been clarified but not incorporated into spec/plan.
   C. Underspecification:
      - Requirements with verbs but missing object or measurable outcome.
      - User stories missing acceptance criteria alignment.
      - Tasks referencing files or components not defined in spec/plan.
      - **IF clarify/ EXISTS**: Validate that clarification answers are reflected in requirements and plan sections.
   D. Constitution alignment:
      - Any requirement or plan element conflicting with a MUST principle.
      - Missing mandated sections or quality gates from constitution.
   E. Coverage gaps:
      - Requirements with zero associated tasks.
      - Tasks with no mapped requirement/story.
      - Non-functional requirements not reflected in tasks (e.g., performance, security).
      - **IF clarify/ EXISTS**: Check if clarification decisions (e.g., retry policies, thresholds) have corresponding validation tasks.
   F. Inconsistency:
      - Terminology drift (same concept named differently across files).
      - Data entities referenced in plan but absent in spec (or vice versa).
      - Task ordering contradictions (e.g., integration tasks before foundational setup tasks without dependency note).
      - Conflicting requirements (e.g., one requires to use Next.js while other says to use Vue as the framework).
      - **IF clarify/ EXISTS**: Flag conflicts between clarification answers and spec/plan statements (clarification should take precedence).
   G. Clarification incorporation (if clarify/ folder exists):
      - Answered questions in clarify/ folder not reflected in spec.md or plan.md.
      - Critical decisions (security, performance, data handling) clarified but missing from non-functional requirements.
      - Conflicting answers between multiple clarification files (multi-feature specs).

5. Severity assignment heuristic:
   - CRITICAL: Violates constitution MUST, missing core spec artifact, or requirement with zero coverage that blocks baseline functionality.
   - HIGH: Duplicate or conflicting requirement, ambiguous security/performance attribute, untestable acceptance criterion.
   - MEDIUM: Terminology drift, missing non-functional task coverage, underspecified edge case.
   - LOW: Style/wording improvements, minor redundancy not affecting execution order.

6. Generate comprehensive Markdown report and write to `{FEATURE_DIR}/AnalysisReport.md`:

   ```markdown
   ## Specification Analysis Report - {Feature Name}

   **Feature**: {Feature Name}
   **Generated**: {YYYY-MM-DD}
   **Artifacts Analyzed**: spec.md, plan.md, tasks.md, constitution.md {version}
   **Status**: {Status summary}

   ---

   ### Executive Summary

   {Brief overview: total issues by severity, coverage statistics, constitutional compliance}

   ---

   ### Analysis Findings

   | ID  | Category    | Severity | Location(s)      | Summary                      | Recommendation                       |
   | --- | ----------- | -------- | ---------------- | ---------------------------- | ------------------------------------ |
   | A1  | Duplication | HIGH     | spec.md:L120-134 | Two similar requirements ... | Merge phrasing; keep clearer version |

   (Add one row per finding; generate stable IDs prefixed by category initial.)

   ---

   ### Coverage Summary

   #### Requirements Coverage Matrix

   | Requirement Key | Requirement Summary | Has Task? | Task IDs | Notes |
   | --------------- | ------------------- | --------- | -------- | ----- |

   **Coverage Statistics:**
   - **Total Requirements**: {count} ({FR count} Functional + {NFR count} Non-Functional)
   - **Requirements with Tasks**: {count} ({percentage}%)
   - **Requirements without Tasks**: {count} ({percentage}%)

   ---

   ### Constitution Alignment Issues

   **Status**: {PASS/FAIL summary}

   {List any principle violations with evidence and remediation}

   ---

   ### Unmapped Tasks

   {List any tasks without clear requirement mapping}

   ---

   ### Cross-Artifact Metrics

   | Metric                     | Value                   | Target | Status  |
   | -------------------------- | ----------------------- | ------ | ------- |
   | Total Requirements         | {count}                 | N/A    | ℹ️       |
   | Total Tasks                | {count}                 | N/A    | ℹ️       |
   | Requirements with >=1 Task | {count} ({percentage}%) | 100%   | {✅/⚠️}   |
   | Tasks with >=1 Requirement | {count} ({percentage}%) | 100%   | {✅/⚠️}   |
   | Critical Issues            | {count}                 | 0      | {✅/⚠️/❌} |
   | High Issues                | {count}                 | <5     | {✅/⚠️/❌} |
   | Medium Issues              | {count}                 | <10    | {✅/⚠️/❌} |
   | Low Issues                 | {count}                 | <15    | {✅/⚠️/❌} |
   | Ambiguity Count            | {count}                 | <5     | {✅/⚠️}   |
   | Duplication Count          | {count}                 | <3     | {✅/⚠️}   |
   | Constitutional Violations  | {count}                 | 0      | {✅/❌}   |

   **Overall Quality Score**: {score}/100
   ```

   After writing the report file, output to chat:
   - File location: `{FEATURE_DIR}/AnalysisReport.md`
   - Executive summary (2-3 sentences)
   - Critical/High issue count
   - Link to full report

7. Append to report file a Next Actions section:
   - If CRITICAL issues exist: Recommend resolving before `/implement`.
   - **If CRITICAL ambiguities detected** (vague requirements, conflicting specifications, missing acceptance criteria that block implementation correctness): Also create `{FEATURE_DIR}/clarification-needed.md` with:

     ```markdown
     # Clarification Needed - Analysis Found Critical Ambiguities
     **Generated by**: /analyze command
     **Date**: {YYYY-MM-DD}
     **Status**: Awaiting clarification

     ## Context
     Cross-artifact analysis identified critical ambiguities that would prevent correct implementation.

     ## Analysis Summary
     - Total findings: [count by severity]
     - Critical ambiguities: [count]
     - Affected requirements: [list requirement IDs or keys]

     ## Critical Ambiguities Requiring Clarification
     [Extract CRITICAL ambiguity findings from analysis report and reformulate as clarification questions]

     ## Recommendation
     Run /clarify to resolve these ambiguities before proceeding with /implement.
     ```

     Inform user in chat: "Analysis complete. Found [N] critical ambiguities. Created {FEATURE_DIR}/clarification-needed.md. Run /clarify to resolve before implementation."
   - If only LOW/MEDIUM: User may proceed, but provide improvement suggestions in report.
   - Provide explicit command suggestions in report: e.g., "Run /specify with refinement", "Run /plan to adjust architecture", "Manually edit tasks.md to add coverage for 'performance-metrics'".

8. Output to chat:

   ```
   ✅ Analysis Complete

   📄 Report: {FEATURE_DIR}/AnalysisReport.md

   📊 Summary:
   - {Total Issues} issues found ({Critical} Critical, {High} High, {Medium} Medium, {Low} Low)
   - {Coverage}% requirement coverage
   - Constitutional compliance: {PASS/FAIL}

   {If issues exist:}
   ⚠️ Action Required: Review AnalysisReport.md for details
   {If CRITICAL:}
   🚫 BLOCKED: Resolve CRITICAL issues before /implement
   {If only LOW/MEDIUM:}
   ✅ Ready for /implement (improvements recommended)
   ```

9. Ask the user: "Would you like me to suggest concrete remediation edits for the top N issues or apply fixes automatically?" (Do NOT apply them automatically without explicit approval.)

Behavior rules:
- **ALWAYS write report to `{FEATURE_DIR}/AnalysisReport.md`** (create or overwrite if exists)
- Generate report even if zero issues found (success report with coverage statistics)
- NEVER hallucinate missing sections—if absent, report them in the file
- KEEP findings deterministic: if rerun without changes, produce consistent IDs and counts
- LIMIT total findings in the main table to 50; aggregate remainder in summarized overflow note
- Provide chat summary but keep detailed findings in the report file

Context: $ARGUMENTS
