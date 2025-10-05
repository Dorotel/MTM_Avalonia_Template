# GitHub Copilot Instructions Directory

This directory contains instruction files that GitHub Copilot uses to provide context-aware assistance.

## Structure

```
.github/
├── copilot-instructions.md                    # Main root-level instructions (auto-discovered)
└── instructions/
    ├── README.md                              # This file
    ├── copilot-instructions.md                # Source file (synced to root)
    ├── Themes.instructions.md                 # Theme system usage guide
    └── [Future].instructions.md               # Additional instruction files
```

## File Discovery

VS Code GitHub Copilot automatically discovers:

1. **Root-level**: `.github/copilot-instructions.md` - Main project instructions
2. **Instruction files**: `.github/instructions/*.instructions.md` - Specific topic instructions

## Naming Convention

- Use `*.instructions.md` suffix for instruction files
- Use descriptive names (e.g., `Themes.instructions.md`, `Services.instructions.md`)
- Include `applyTo` frontmatter to scope instructions to specific file patterns

## Frontmatter Example

```markdown
---
description: 'Brief description of what this instruction file covers'
applyTo: '**/*.axaml,**/*ViewModel.cs'
---
```

## Maintenance

- The root `.github/copilot-instructions.md` is a copy of `instructions/copilot-instructions.md`
- When updating `instructions/copilot-instructions.md`, remember to sync it to the root:
  ```powershell
  Copy-Item -Path ".github\instructions\copilot-instructions.md" -Destination ".github\copilot-instructions.md" -Force
  ```

## Current Instruction Files

| File | Description | Applies To |
|------|-------------|------------|
| `copilot-instructions.md` | Main project development guidelines | `**/*` |
| `Themes.instructions.md` | Avalonia Theme System usage guide | `**/*.axaml,**/*ViewModel.cs` |

---

**Last Updated**: 2025-10-05
