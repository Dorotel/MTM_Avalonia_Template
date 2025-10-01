# Project Structure Guide (What’s in the Folder?)

After you run “specify init”, you’ll see a folder like this:
```
.specify/
  memory/
    constitution.md        ← your project rules
  scripts/                 ← helper scripts (create feature, check tools, etc.)
  specs/
    001-your-feature/
      spec.md              ← your feature spec (stories + acceptance criteria)
templates/
  spec-template.md
  plan-template.md
  tasks-template.md
```

What each part is for
- constitution.md: The rules your agent follows (quality, testing, UX, performance).
- spec.md: The “what and why” of your feature.
- plan files (created later): The “how.”
- tasks.md (created later): The step‑by‑step checklist.
- scripts: Helpers the agent can run for you.

Tip
- Keep one feature per folder (001‑, 002‑, etc.). Easy to track and review.