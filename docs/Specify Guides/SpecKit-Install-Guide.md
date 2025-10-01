# Spec Kit Install Guide (Easy Mode)

Who this is for
- Anyone new to Spec Kit who wants the simplest way to get started.

What Spec Kit does
- It helps you plan and build software by writing good specs first, then turning them into tasks and working code.

Before you start
- You need:
  - Python 3.11+
  - Git
  - uv (a Python tool manager)
  - One AI coding agent (for example: GitHub Copilot, Claude Code, Cursor, Gemini CLI, etc.)

Step 1 — Install the Specify tool (the CLI)
- Persistent install (recommended):
```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
```
- One‑time run (no install):
```bash
uvx --from git+https://github.com/github/spec-kit.git specify init <PROJECT_NAME>
```

Step 2 — Check your tools
```bash
specify check
```
- This checks you have Git and your AI agent tools set up.

Step 3 — Confirm your AI agent can see commands
- After project init (next guide), your agent should show these commands:
  - /constitution
  - /specify
  - /clarify
  - /plan
  - /tasks
  - /analyze
  - /implement

Tips
- If you’re on Windows, PowerShell works great. Use the “ps” option later if needed.
- If you hit auth issues, try running “specify check” and fix what it lists.

Links
- Spec Kit README: https://github.com/github/spec-kit#readme
- Video overview: https://www.youtube.com/watch?v=a9eR1xsfvHg