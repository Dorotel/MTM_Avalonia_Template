# Troubleshooting Guide

Problem: “specify” command not found
- Check uv is installed
- Try: uv tool list
- Re‑install: uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

Problem: Agent doesn’t show slash commands
- Make sure the project was initialized with “specify init”
- Open the project folder in your agent
- If still broken, re‑run “specify init --here --force” (beware overwrites)

Problem: Git auth issues (Linux)
- Install Git Credential Manager:
```bash
wget https://github.com/git-ecosystem/git-credential-manager/releases/download/v2.6.1/gcm-linux_amd64.2.6.1.deb
sudo dpkg -i gcm-linux_amd64.2.6.1.deb
git config --global credential.helper manager
```

Problem: Corporate network blocks stuff
- Use the --github-token option when running “specify init”
- Or set GH_TOKEN/GITHUB_TOKEN environment variable

Tip
- When you get an error, copy it into your agent chat. Ask it to help you fix the exact problem step by step.