# Copilot Assets Checklist

> **Purpose:** Ensure AI assistance is grounded, safe, and context-aware for all contributors.
> **Priority:** High
> **Dependencies:** `.github` folder, stable instructions, Spec-Kit artifacts
> **Consumers:** All contributors and Copilot agents
> **Non-functionals:** Up-to-date, minimal ambiguity

---

## For Humans

| Aspect         | Description                                                                    |
| -------------- | ------------------------------------------------------------------------------ |
| **Intent**     | Make Copilot Chat helpful and safe by providing project context and guardrails |
| **Usage**      | Repo setup, periodic updates                                                   |
| **Depends On** | `.github` folder, copilot-instructions.md, specs with AC IDs                   |
| **Impact**     | Code quality, prompt effectiveness                                             |

---

## For AI Agents

| Asset                                                     | Purpose/Constraint                                                                                    |
| --------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `.github/copilot-instructions.md`                         | Project purpose, domain glossary, architecture, constraints, answering rules                          |
| `.github/pull_request_template.md`                        | Enforces spec/AC references, blocks PRs without AC IDs (see Constitution III, Test-First Development) |
| `.github/ISSUE_TEMPLATE/feature_request.yml`              | Requires spec link and acceptance criteria                                                            |
| `.github/ISSUE_TEMPLATE/bug_report.yml`                   | Requires steps, expected/actual, spec link                                                            |
| Editor/SDK pinning files (`.editorconfig`, `global.json`) | Ensures consistent tooling and formatting                                                             |

**Spec-Kit Integration:**

- All features must reference specs with AC IDs
- Traceability process enforced (see Constitution: Spec-Driven Development Workflow)

---

## Checklist

- [ ] `copilot-instructions.md` reflects Visual ERP read-only, API Toolkit-only, Android API-only policy
- [ ] PR template enforces spec/AC references (block PRs without AC IDs on protected branches)
- [ ] Issue templates link to specs and require acceptance criteria
- [ ] Editor/SDK pinning files present (`.editorconfig`, `global.json`)
- [ ] Specify files prompt current constraints

---

## Clarification Questions

| Question                  | Why/Reason            | Suggested | Options                                            |
| ------------------------- | --------------------- | --------- | -------------------------------------------------- |
| Block PRs without AC IDs? | Maintain traceability | Yes       | [A] Warn [B] Block [C] Block on protected branches |

---

> **Formatting:**
>
> - Use markdown tables for clarity
> - Reference constitutional principles (see [Constitution.md](../.specify/memory/constitution.md))
> - Keep sections concise and actionable
> - Use checklists for compliance tracking
