# Specify Command Checklist
How to run prompt-driven spec generation safely and consistently.

For humans
- Purpose: Ensure the /specify process produces aligned documents using our standards.
- When used: Any time you generate or refresh feature docs.
- Dependencies: specify files folder, master prompt, per-feature prompts.
- What depends on it: Spec quality and team alignment.
- Priority: High.

For AI agents
- Intent: Orchestrate runs using specify files/specify.master-prompt.md (and MTM-TEMPLATE-CONSOLIDATION.master-prompt.md for consolidation); update docs/specs/features/*; never inject code into normal docs.
- Dependencies: Repo context loading, prompt templates, file write permissions.
- Consumers: Spec reviewers, developers, QA.
- Non-functionals: Deterministic formatting; idempotent updates; conflict detection.
- Priority: High.

Steps
- Review constraints in CORE-SERVICES-AND-SYSTEMS.md, VISUAL-SERVER-INTEGRATION.md, DATABASE-MYSQL-57-MAMP-SETUP.md, and Visual credentials/allowlist docs.
- Run /specify with the master prompt for a full refresh.
- For small updates, run /specify with a per-feature prompt.
- After generation, validate:
  - Visual read-only (Toolkit-only) policy present in relevant sections with citations.
  - Android path uses API only.
  - Acceptance criteria include IDs.
  - Clarification questions appended.

Clarification questions
- Q: Should /specify runs open PRs automatically?
  - Why: Control over changes.
  - Suggested: Yes, with review required.
  - Reason: Keeps history/audit trail.
  - Options: [A] Direct commit [B] PR with review [C] PR on protected branches only