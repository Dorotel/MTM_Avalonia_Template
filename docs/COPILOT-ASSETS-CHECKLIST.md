# Copilot Assets Checklist
Ensures AI assistance is grounded and consistent across the repo.

For humans
- Purpose: Make Copilot Chat helpful and safe by giving it project context and guardrails.
- When used: Repo setup and periodic updates.
- Dependencies: .github folder access, stable instructions.
- What depends on it: Code quality and prompt effectiveness.
- Priority: High.

For AI agents
- Intent: Provide and maintain:
  - .github/copilot-instructions.md (project purpose, domain glossary, architecture, constraints, answering rules).
  - .github/pull_request_template.md (reference spec AC IDs).
  - .github/ISSUE_TEMPLATE/feature_request.yml (spec link, AC).
  - .github/ISSUE_TEMPLATE/bug_report.yml (steps, expected/actual, spec).
- Dependencies: Specs with AC IDs; traceability process.
- Consumers: All contributors using Copilot.
- Non-functionals: Up-to-date, minimal ambiguity.
- Priority: High.

Checklist
- [ ] copilot-instructions.md reflects Visual read-only rule, API Toolkit-only, and Android API-only policy.
- [ ] PR template enforces spec/AC references.
- [ ] Issue templates link to specs and request acceptance criteria.
- [ ] Editor/SDK pinning files present (.editorconfig, global.json).
- [ ] “Specify files” prompts current with constraints.

Clarification questions
- Q: Should PRs without AC IDs be blocked?
  - Why: Maintain traceability.
  - Suggested: Yes.
  - Reason: Aligns spec and code.
  - Options: [A] Warn [B] Block [C] Block on protected branches