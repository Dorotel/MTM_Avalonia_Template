# CLI Guide: specify init and specify check

Why these matter
- They set up your project and verify your tools before you run slash commands.

specify init — create a new project
```bash
# Basic
specify init my-project

# Choose AI agent
specify init my-project --ai copilot
specify init my-project --ai claude
specify init my-project --ai cursor

# PowerShell scripts (Windows/cross‑platform)
specify init my-project --ai copilot --script ps

# Use current folder
specify init . --ai copilot
specify init --here --ai copilot

# Force into a non‑empty folder
specify init --here --force --ai copilot

# Skip git (optional)
specify init my-project --no-git

# Use GitHub token if required by your network
specify init my-project --ai claude --github-token ghp_your_token_here
```

specify check — verify your setup
```bash
specify check
```
- Looks for Git, Python/uv, and supported agent tools on your machine.

Troubleshooting
- If an agent tool is missing, install it and run “specify check” again.
- If init fails in a non‑empty directory, add “--force” (be careful—overwrites files).