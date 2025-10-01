# Guide: /implement — Build the Feature

What it does
- Runs the task list, in order, to create code and tests that match your plan and spec.

When to use it
- After /analyze is clean (no critical issues).

How to run
```text
/implement
```

What it does for you
- Checks your prerequisites (constitution, spec, plan, tasks)
- Follows the task order and dependencies
- Encourages test‑driven development (TDD)
- Reports progress and errors

Your job during /implement
- Install any needed tools it asks for (like dotnet, npm, etc.)
- Run and test the app after it finishes
- Copy any new errors back to the agent if needed

Tip
- Keep commits small and frequent. Reference acceptance criteria IDs in PRs.