# Spec Kit Project Init Guide

Goal
- Create a fresh project folder that includes templates, spec files, and scripts so your agent can work.

Basic init
```bash
specify init my-project
```

Pick your AI agent (examples)
```bash
specify init my-project --ai copilot
specify init my-project --ai claude
specify init my-project --ai cursor
```

Windows/cross‑platform scripts
```bash
specify init my-project --ai copilot --script ps
```

Current folder (no new directory)
```bash
specify init . --ai copilot
# or
specify init --here --ai copilot
```

Force into a non‑empty folder (overwrites conflicts)
```bash
specify init --here --force --ai copilot
```

Skip git setup (optional)
```bash
specify init my-project --no-git
```

Use a GitHub token if your network needs it
```bash
specify init my-project --ai claude --github-token ghp_your_token_here
```

What you get after init
- A .specify folder with:
  - memory/constitution.md
  - scripts to help your agent
  - templates for specs, plans, and tasks

Pro tip
- If your repo isn’t using Git, you can set which feature you’re working on with an environment variable later:
  - SPECIFY_FEATURE=<feature-folder-name>