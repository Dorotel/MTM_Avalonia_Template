# Specify Command Checklist

> **Purpose:** Ensure the `/specify` process produces aligned documents using project standards.
> **When to Use:** Whenever generating or refreshing feature documentation.
> **Dependencies:** Specify files folder, master prompt, per-feature prompts.
> **Priority:** High
> **Impact:** Spec quality and team alignment.

---

## For Humans

- Follow the checklist to ensure all `/specify` runs meet project requirements.
- Validate output for alignment with standards and completeness.

---

## For AI Agents

- **Intent:** Orchestrate `/specify` runs using `specify.master-prompt.md` (and `MTM-TEMPLATE-CONSOLIDATION.master-prompt.md` for consolidation).
- **Actions:**
  - Update `docs/specs/features/*` only.
  - Never inject code into normal documentation.
- **Dependencies:**
  - Repo context loading
  - Prompt templates
  - File write permissions
- **Consumers:** Spec reviewers, developers, QA.
- **Non-Functionals:**
  - Deterministic formatting
  - Idempotent updates
  - Conflict detection

---

## Steps

1. **Review Constraints:**

   - `CORE-SERVICES-AND-SYSTEMS.md`
   - `VISUAL-SERVER-INTEGRATION.md`
   - `DATABASE-MYSQL-57-MAMP-SETUP.md`
   - Visual credentials/allowlist docs

2. **Run `/specify`:**

   - Use the master prompt for full refresh.
   - For minor updates, use per-feature prompt.

3. **Validate Output:**
   - Visual read-only (Toolkit-only) policy present with citations.
   - Android path uses API only.
   - Acceptance criteria include IDs.
   - Clarification questions appended.

---

## Clarification Questions

- **Q:** Should `/specify` runs open PRs automatically?
  - **Why:** Control over changes.
  - **Suggested:** Yes, with review required.
  - **Reason:** Maintains history/audit trail.
  - **Options:**
    - [A] Direct commit
    - [B] PR with review
    - [C] PR on protected branches only

---

> **Formatting Standards:**
>
> - Use semantic headings and lists.
> - Prefer bold for key terms.
> - Ensure deterministic, idempotent formatting.
> - Align with [Project Constitution](../.specify/memory/constitution.md) and markdown best practices.
