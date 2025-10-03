# Roles and Permissions Matrix

> **Purpose:** Prevent unauthorized operations and provide clarity for approvals.
> **Usage:** Feature design, audit, onboarding.
> **Dependencies:** Auth service, policy catalog.
> **Consumers:** Navigation guards, command guards, API authorization.
> **Priority:** High.

## Role-Based Policy Table

| Role           | Actions Allowed                                                              | Visual Access | Write Scope                        | Special Permissions                      |
| -------------- | ---------------------------------------------------------------------------- | ------------- | ---------------------------------- | ---------------------------------------- |
| **Clerk**      | Receive (standard), Move, Issue, Adjust                                      | Read-only     | App DB inventory transactions only | Cannot approve over-receipts             |
| **Supervisor** | All Clerk actions, Approve Over-Receipt, Negative Adjustments, Lot Overrides | Read-only     | App DB inventory transactions only | Exception handling, adjustment approval  |
| **Admin**      | All Supervisor actions, Manage Templates, Manage Users, View Reports         | Read-only     | App DB inventory transactions only | User management (APPLICATION_USER table) |

### Data Access

- **Read:**
  - PART (ID, 30 chars)
  - LOCATION (ID, 15 chars)
  - WAREHOUSE (ID, 15 chars)
  - _All via cached projections; Visual ERP is always read-only_
- **Write:**
  - Inventory transactions in app DB only (never Visual ERP)

### User Management

- **Admin only:**
  - Manage users via `APPLICATION_USER` table
    - `NAME` (nvarchar(20))
    - `IS_ADMIN` flag
    - _(Visual Data Table.csv Line: 574)_

## Supervisor Approval Requirements

> **Which transactions require Supervisor approval?**
>
> - **Why:** Risk management
> - **Suggested:**
>   - [A] Over-receipts
>   - [B] Negative adjustments
>   - [C] Lot overrides
>   - [D] All
>   - [E] Other: **\_\_**
> - **Reason:** Higher risk of error or fraud

---

**Formatting Standards:**

- Table layout for role/action mapping
- Semantic headings
- Data types and field lengths specified
- Visual ERP access is always read-only
- Markdown aligns with project constitution and extension best practices
