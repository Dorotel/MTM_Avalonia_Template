# Roles and Permissions Matrix
Defines who can perform which actions in the app.

For humans
- Purpose: Prevent unauthorized operations and provide clarity for approvals.
- When used: Feature design, audit, onboarding.
- Dependencies: Auth service, policy catalog.
- What depends on it: Navigation guards, command guards, API authorization.
- Priority: High.

For AI agents
- Intent: Provide a policy table mapping roles (Clerk, Supervisor, Admin) to actions (Receive, Move, Issue, Adjust, Approve Over-Receipt, Manage Templates, View Reports).
- Dependencies: Authentication, session state, configuration.
- Consumers: UI guards, API policies, repositories.
- Non-functionals: Easy to change with audit trails.
- Priority: High.

Policy examples (Visual read-only enforced everywhere)
- Clerk: Receive (standard), Move/Issue (standard), cannot approve over-receipts.
  - Can read: PART (30-char ID), LOCATION (15-char ID), WAREHOUSE (15-char ID) via cached projections
  - Can write: App DB inventory transactions only (not Visual)
- Supervisor: Can approve over-receipts and negative adjustments.
  - Additional permissions for exception handling and adjustments
  - Same Visual read-only access as Clerk (no Visual writes)
- Admin: Manage templates, manage users (app-level via APPLICATION_USER table, not Visual roles), view all reports.
  - Visual access remains read-only even for admins
  - User management: APPLICATION_USER.NAME nvarchar(20), IS_ADMIN flag (Visual Data Table.csv Line: 574)

Clarification questions
- Q: Which transactions require Supervisor approval?
  - Why: Risk management.
  - Suggested: Over-receipts, negative adjustments, lot overrides.
  - Reason: Higher risk of error or fraud.
  - Options: [A] Over-receipts [B] Negative adjustments [C] Lot overrides [D] All [E] Other: ______