# Feature Specification: [FEATURE NAME]

**Feature Branch**: `[###-feature-name]`
**Created**: [DATE]
**Status**: Draft
**Input**: User description: "$ARGUMENTS"

## Execution Flow (main)

```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

## Clarifications Q&A Template

Use this section whenever any [NEEDS CLARIFICATION: …] markers exist in the spec. Keep each question atomic and traceable.

### How to use
- Create one entry per ambiguity with a unique ID: CL-001, CL-002, …
- Quote the exact marker text in “Question”.
- Add a proposed interim assumption (to unblock planning) and note its risk.
- After an answer is agreed, update “Answer/Decision” and list all “Spec Changes”.
- Remove the corresponding [NEEDS CLARIFICATION: …] tag(s) from the body once resolved.

### Index
- Open: list CL IDs with a short label
- Resolved: list CL IDs with a short outcome

### Entry Template (copy per question)
- ID: CL-###
- Tag location(s): section and line or bullet where the marker appears
- Question (exact): the full text from the marker
- Context: why this matters for scope, users, or testing
- Options considered:
  - Option A: brief
  - Option B: brief
- Proposed interim assumption: the assumption to proceed if not answered now
- Impact if unresolved: risks to scope, timeline, testing, or dependencies
- Priority: High | Medium | Low
- Owner: decision-maker
- Due date: yyyy-mm-dd
- Status: Open | Answered | Deferred
- Answer/Decision: the agreed resolution (concise)
- Spec changes:
  - Update section “…”
  - Add/modify requirements “…”
  - Remove marker(s) at “…”
- Notes: any follow-ups or dependencies

### Minimal Example (illustrative)
- ID: CL-001
- Tag location(s): Requirements > FR-006
- Question (exact): authenticate users via which method?
- Context: affects user flows and acceptance tests
- Options considered: A) Email/Password B) SSO
- Proposed interim assumption: Email/Password
- Impact if unresolved: blocks acceptance scenarios
- Priority: High
- Owner: Product
- Due date: 2025-10-10
- Status: Answered
- Answer/Decision: Use Email/Password
- Spec changes:
  - Updated Requirements (FR-006) to specify Email/Password
  - Removed marker from FR-006
- Notes: Revisit SSO in a separate feature

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers
- 📅 Keep it concise and focused on key points
- 🔄 Include examples and use cases to illustrate concept
- ✍️ Use clear and simple language, avoiding jargon, but make it easy for a copilot agent to understand

---

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
   - User types and permissions
   - Data retention/deletion policies
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Security/compliance needs
   - **MAMP MySQL database changes** (if feature modifies database):
     - Which tables are created/modified?
     - What columns, types, and constraints?
     - Are JSON files in `.github/mamp-database/` referenced?
     - Is `migrations-history.json` updated with version increment?
   - **Visual ERP integration** (if feature accesses Visual):
     - Which Visual tables/data are accessed?
     - Are API Toolkit commands read-only?
     - Are commands in whitelist (`docs/VISUAL-WHITELIST.md`)?
     - Platform-specific access patterns (Windows direct vs Android via MTM Server API)?

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
[Describe the main user journey in plain language]

### Acceptance Scenarios
1. **Given** [initial state], **When** [action], **Then** [expected outcome]
2. **Given** [initial state], **When** [action], **Then** [expected outcome]

### Edge Cases
- What happens when [boundary condition]?
- How does system handle [error scenario]?

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST [specific capability, e.g., "allow users to create accounts"]
- **FR-002**: System MUST [specific capability, e.g., "validate email addresses"]
- **FR-003**: Users MUST be able to [key interaction, e.g., "reset their password"]
- **FR-004**: System MUST [data requirement, e.g., "persist user preferences"]
- **FR-005**: System MUST [behavior, e.g., "log all security events"]

*Example of marking unclear requirements:*
- **FR-006**: System MUST authenticate users via [NEEDS CLARIFICATION: auth method not specified - email/password, SSO, OAuth?]
- **FR-007**: System MUST retain user data for [NEEDS CLARIFICATION: retention period not specified]

### Key Entities *(include if feature involves data)*
- **[Entity 1]**: [What it represents, key attributes without implementation]
- **[Entity 2]**: [What it represents, relationships to other entities]

### Database Schema Changes *(include if feature modifies MAMP MySQL database)*

**IMPORTANT**: Before documenting changes, ALWAYS reference existing schema in `.github/mamp-database/schema-tables.json`

**Tables Modified/Created**:
- **[TableName]**: [Brief description of purpose]
  - Columns added/modified: [list columns with types]
  - Indexes required: [list indexes for performance]
  - Foreign keys: [relationships to other tables]
  - Reason: [Why this change is needed]

**Schema Documentation Requirements**:
- [ ] Read `.github/mamp-database/schema-tables.json` before planning
- [ ] Document exact table names (case-sensitive: `Users`, not `users`)
- [ ] Document exact column names (case-sensitive: `UserId`, not `userid`)
- [ ] Specify data types (VARCHAR(100), INT, DATETIME, BOOLEAN, etc.)
- [ ] Document nullable vs NOT NULL constraints
- [ ] Document default values where applicable
- [ ] List indexes needed for query performance
- [ ] Update `.github/mamp-database/migrations-history.json` with version increment

**Example**:
- **UserPreferences**: Stores user-specific application settings
  - Columns: `PreferenceId` (INT, PK), `UserId` (INT, FK), `PreferenceKey` (VARCHAR(100)), `PreferenceValue` (TEXT), `LastUpdated` (DATETIME)
  - Indexes: INDEX on `UserId`, INDEX on `PreferenceKey`
  - Foreign keys: `UserId` → `Users.UserId`
  - Reason: FR-032 requires persistent user preferences across sessions

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified
- [ ] Database schema changes documented (if applicable)
- [ ] JSON files in `.github/mamp-database/` referenced (if database changes)

---

## Execution Status
*Updated by main() during processing*

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---
