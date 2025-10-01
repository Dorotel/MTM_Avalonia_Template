# Environment Variables You Might Use

SPECIFY_FEATURE
- What it does: tells the agent which feature to work on when you’re not using Git branches.
- How to use:
```bash
# example
export SPECIFY_FEATURE=001-photo-albums
```

When to use this
- If you’re experimenting outside a Git repo
- If your agent isn’t switching branches but you want to focus on a specific feature folder

Tip
- You usually won’t need this if you’re using Git branches created by the agent.