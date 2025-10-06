---
description: Systematically fix ALL issues (CRITICAL to LOW) identified in AnalysisReport.md from /analyze command. Applies automated remediation with validation.
---

The user input to you can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

Goal: Automatically remediate ALL issues identified in `{FEATURE_DIR}/AnalysisReport.md` by the `/analyze` command. This includes CRITICAL, HIGH, MEDIUM, and LOW severity findings. All changes are validated and tracked.

Prerequisites: `/analyze` must have been run successfully and `AnalysisReport.md` must exist.

Execution steps:

1. Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` once from repo root and parse JSON for FEATURE_DIR and AVAILABLE_DOCS. Derive absolute paths:
   - ANALYSIS_REPORT = FEATURE_DIR/AnalysisReport.md
   - SPEC = FEATURE_DIR/spec.md
   - PLAN = FEATURE_DIR/plan.md
   - TASKS = FEATURE_DIR/tasks.md

   **HALT CHECK**: If `{FEATURE_DIR}/AnalysisReport.md` does NOT exist, STOP immediately. Inform user: "Cannot proceed with /fix - no analysis report found. Run /analyze first to identify issues."

2. Load and parse AnalysisReport.md:
   - Extract all findings from "Analysis Findings" table (ID, Category, Severity, Location, Summary, Recommendation)
   - Parse Coverage Summary to identify requirements without tasks
   - Parse Constitution Alignment Issues
   - Parse Unmapped Tasks section
   - Build prioritized fix queue: CRITICAL → HIGH → MEDIUM → LOW

3. Load target artifacts:
   - Read spec.md, plan.md, tasks.md
   - Load constitution `.specify/memory/constitution.md` for validation
   - Parse current state of all requirements, tasks, and cross-references

4. Execute remediation passes (in order):

   **A. CRITICAL Fixes (Constitution Violations & Blocking Issues)**
   - Constitution MUST violations: Adjust spec/plan/tasks to align with non-negotiable principles
   - Missing core artifacts: Create skeleton sections if entirely absent
   - Requirements with zero coverage blocking baseline functionality: Generate placeholder tasks or flag for manual review
   - After each fix: Validate against constitution; if still non-compliant, escalate to user with explanation

   **B. HIGH Severity Fixes (Duplicates, Conflicts, Security/Performance Ambiguities)**
   - Duplicate requirements: Merge into single consolidated requirement (keep clearer phrasing, preserve all acceptance criteria)
   - Conflicting requirements: Analyze context and resolve (prefer later/more specific requirement; if unresolvable, flag for user decision)
   - Ambiguous security/performance attributes: Add measurable criteria based on project standards (e.g., "secure" → "AES-256 encryption", "fast" → "< 200ms response time")
   - Untestable acceptance criteria: Rewrite with concrete, verifiable conditions

   **C. MEDIUM Severity Fixes (Terminology, Coverage Gaps, Underspecification)**
   - Terminology drift: Standardize terminology across all three files (use most recent/spec version as source of truth)
   - Missing non-functional task coverage: Generate test/validation tasks for NFRs (performance, security, accessibility)
   - Underspecified edge cases: Add concrete examples and expected behaviors to requirements
   - Requirements without mapped tasks: Create placeholder tasks in appropriate phase with [GENERATED] marker

   **D. LOW Severity Fixes (Style, Wording, Minor Redundancy)**
   - Style/wording improvements: Apply consistent formatting and clear language
   - Minor redundancy: Remove or consolidate without affecting semantic meaning
   - Task ordering improvements: Add dependency notes where logical sequencing is implied but not explicit

5. Coverage gap remediation:
   - For each requirement without tasks: Generate task(s) with:
     - Stable task ID (next available in sequence)
     - Clear description referencing requirement key/ID
     - Appropriate phase assignment (based on dependency analysis)
     - Acceptance criteria aligned with requirement
     - [GENERATED] marker for user review
   - For each unmapped task: Either:
     - Find implicit requirement connection and document in task description
     - Or flag as orphan task for user review (may be infrastructure/tooling task without direct FR)

6. Consistency enforcement:
   - Terminology normalization pass across all three files
   - Data entity reference synchronization (ensure entities in plan exist in spec)
   - Task ordering validation (flag any remaining contradictions with dependency notes)
   - Cross-reference all requirement IDs/keys in tasks

7. Validation pass:
   - Re-run internal consistency checks (same logic as analyze.prompt.md steps 3-4)
   - Verify all original findings are resolved
   - Check for no new issues introduced by fixes
   - Validate constitutional compliance maintained

8. Generate fix summary report and append to `{FEATURE_DIR}/AnalysisReport.md`:

   ```markdown
   ---

   ## Remediation Summary

   **Executed**: {YYYY-MM-DD HH:MM}
   **Command**: /fix
   **Status**: {COMPLETE/PARTIAL}

   ### Fixes Applied

   | Original ID | Category      | Severity | Action Taken                           | Files Modified    | Status |
   | ----------- | ------------- | -------- | -------------------------------------- | ----------------- | ------ |
   | C1          | Constitution  | CRITICAL | Added missing async pattern validation | plan.md, tasks.md | ✅      |
   | D1          | Duplication   | HIGH     | Merged FR-013a into FR-013             | spec.md           | ✅      |
   | I1          | Inconsistency | MEDIUM   | Standardized "SecretsService" term     | spec.md, plan.md  | ✅      |
   | A1          | Ambiguity     | LOW      | Added concrete timeout threshold       | spec.md           | ✅      |

   ### Statistics

   - **Total Issues Fixed**: {count}
   - **Files Modified**: {list}
   - **New Tasks Generated**: {count}
   - **Requirements Updated**: {count}

   ### Validation Results

   - Requirements Coverage: {before}% → {after}%
   - Unmapped Tasks: {before} → {after}
   - Constitutional Compliance: {PASS/FAIL}
   - New Issues Introduced: {count}

   ### Remaining Actions

   {If any issues couldn't be auto-fixed:}
   ⚠️ **Manual Review Required**:
   - {List issues requiring human judgment}
   - {Provide specific guidance for each}

   {If all fixed:}
   ✅ All issues resolved. Feature artifacts are now consistent and complete.

   ### Next Steps

   {Based on validation results:}
   - Re-run /analyze to verify 100% resolution: `.specify/scripts/powershell/check-prerequisites.ps1 -Command analyze`
   - Proceed to /implement: `.specify/scripts/powershell/check-prerequisites.ps1 -Command implement`
   ```

9. Create backup before applying fixes:
   - Copy spec.md → spec.md.backup-{timestamp}
   - Copy plan.md → plan.md.backup-{timestamp}
   - Copy tasks.md → tasks.md.backup-{timestamp}
   - Inform user: "Backups created in {FEATURE_DIR}/ with .backup-{timestamp} suffix"

10. Apply all fixes using multi_replace_string_in_file for efficiency:
    - Group fixes by file (spec.md, plan.md, tasks.md)
    - Apply all changes to each file in single operation
    - Use precise string matching with sufficient context (5+ lines before/after)

11. Output to chat:

    ```
    ✅ Fix Complete

    📄 Updated Report: {FEATURE_DIR}/AnalysisReport.md

    📊 Summary:
    - {Total} issues fixed ({Critical} Critical, {High} High, {Medium} Medium, {Low} Low)
    - {Files Modified} files modified
    - {New Tasks} tasks generated
    - Requirements coverage: {before}% → {after}%

    💾 Backups: {FEATURE_DIR}/*.backup-{timestamp}

    {If validation passed:}
    ✅ Validation passed - all issues resolved
    📋 Next: Run /analyze to verify or proceed to /implement

    {If manual review needed:}
    ⚠️ Manual review required for {count} items (see report)
    {List specific items}
    ```

12. Ask user: "Would you like me to run /analyze again to verify all fixes, or proceed to /implement?"

Behavior rules:
- **ALWAYS create backups before modifying files** (.backup-{timestamp} suffix)
- **ALWAYS append remediation summary to AnalysisReport.md** (preserve original findings)
- **NEVER silently skip issues** - if can't auto-fix, document reason and provide guidance
- **PREFER specificity over generalization** - add concrete values rather than keeping vague terms
- **VALIDATE after fixing** - re-run consistency checks to ensure no new issues introduced
- **USE multi_replace_string_in_file** for efficiency when multiple fixes target same file
- **FLAG ambiguities requiring human judgment** - don't guess at user intent for critical decisions
- **MAINTAIN traceability** - every fix references original finding ID from analysis report

Safety guardrails:
- If a fix would violate constitution: STOP and escalate to user
- If conflicting requirements can't be resolved programmatically: FLAG for manual decision
- If generated task would duplicate existing task: MERGE rather than duplicate
- If terminology normalization affects meaning: PRESERVE original + note variant in parentheses
- If fix introduces new ambiguity: REVERT and mark as requiring manual intervention

Context: $ARGUMENTS
