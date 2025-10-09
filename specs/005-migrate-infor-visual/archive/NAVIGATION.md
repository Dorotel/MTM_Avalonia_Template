# Feature 005 Restart - Visual Navigation

**Quick reference diagram for navigating all generated files**

---

## File Tree

```
specs/005-migrate-infor-visual/
│
├── 📋 RESTART-GUIDE.md ⭐ START HERE
│   └── Step-by-step instructions for restart
│
├── 📄 RESTART-PROMPT.md
│   └── Main prompt file for /speckit.specify command
│
├── 📊 FILE-SUMMARY.md
│   └── Overview of all created files
│
├── 🌳 NAVIGATION.md (this file)
│   └── Visual navigation diagram
│
├── 📝 CLARIFICATION-QUESTIONS.html
│   └── Interactive Q&A form (already completed)
│
└── 📁 reference/
    │
    ├── 📖 README.md ⭐ REFERENCE INDEX
    │   └── Index and usage guide for all references
    │
    ├── 💬 REFERENCE-CLARIFICATIONS.md
    │   └── User's answers to 21 questions
    │
    ├── 🎨 REFERENCE-EXISTING-PATTERNS.md
    │   └── Codebase patterns to follow
    │
    ├── 🧩 REFERENCE-CUSTOM-CONTROLS.md
    │   └── 10 custom controls to extract/create
    │
    ├── ⚙️ REFERENCE-SETTINGS-INVENTORY.md
    │   └── 60+ settings requiring UI
    │
    ├── ✅ REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
    │   └── Constitution compliance checklist
    │
    └── 🔌 REFERENCE-VISUAL-API-SCOPE.md
        └── Visual ERP integration scope
```

---

## Reading Order

### For Quick Start (30 minutes)

```
1. RESTART-GUIDE.md (5 min)
   ↓
2. reference/README.md (5 min)
   ↓
3. reference/REFERENCE-CLARIFICATIONS.md (5 min)
   ↓
4. FILE-SUMMARY.md (5 min)
   ↓
5. Skim other references as needed (10 min)
```

### For Deep Understanding (2 hours)

```
1. RESTART-GUIDE.md (10 min)
   ↓
2. reference/README.md (10 min)
   ↓
3. reference/REFERENCE-CLARIFICATIONS.md (10 min)
   ↓
4. reference/REFERENCE-EXISTING-PATTERNS.md (15 min)
   ↓
5. reference/REFERENCE-CUSTOM-CONTROLS.md (20 min)
   ↓
6. reference/REFERENCE-SETTINGS-INVENTORY.md (20 min)
   ↓
7. reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md (25 min)
   ↓
8. reference/REFERENCE-VISUAL-API-SCOPE.md (20 min)
   ↓
9. FILE-SUMMARY.md (10 min)
```

---

## File Purpose Quick Reference

| Icon | File                                  | Purpose                          | Priority |
| ---- | ------------------------------------- | -------------------------------- | -------- |
| 📋   | RESTART-GUIDE.md                      | How to restart Feature 005       | 🔴 Must  |
| 📄   | RESTART-PROMPT.md                     | Spec-kit prompt file             | 🔴 Must  |
| 📊   | FILE-SUMMARY.md                       | Overview of all files            | 🟡 Should|
| 🌳   | NAVIGATION.md                         | This navigation guide            | 🟢 Nice  |
| 📝   | CLARIFICATION-QUESTIONS.html          | Interactive Q&A (completed)      | 🟢 Nice  |
| 📖   | reference/README.md                   | Reference files index            | 🔴 Must  |
| 💬   | reference/REFERENCE-CLARIFICATIONS.md | User's Q&A answers               | 🔴 Must  |
| 🎨   | reference/REFERENCE-EXISTING-PATTERNS.md| Codebase patterns             | 🟡 Should|
| 🧩   | reference/REFERENCE-CUSTOM-CONTROLS.md| Custom controls catalog         | 🔴 Must  |
| ⚙️   | reference/REFERENCE-SETTINGS-INVENTORY.md| Settings catalog             | 🔴 Must  |
| ✅   | reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md| Compliance checklist| 🔴 Must|
| 🔌   | reference/REFERENCE-VISUAL-API-SCOPE.md| Visual ERP scope              | 🔴 Must  |

**Priority Legend**:

- 🔴 Must Read - Critical for understanding
- 🟡 Should Read - Important for implementation
- 🟢 Nice to Have - Reference when needed

---

## Phase-Specific File Usage

### Specification Writing Phase

```
Primary References:
├── reference/REFERENCE-CLARIFICATIONS.md
├── reference/REFERENCE-CUSTOM-CONTROLS.md
├── reference/REFERENCE-SETTINGS-INVENTORY.md
├── reference/REFERENCE-VISUAL-API-SCOPE.md
└── reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md

Support:
└── reference/README.md
```

### Planning Phase

```
Primary References:
├── reference/REFERENCE-EXISTING-PATTERNS.md
├── reference/REFERENCE-CUSTOM-CONTROLS.md
├── reference/REFERENCE-SETTINGS-INVENTORY.md
└── reference/REFERENCE-VISUAL-API-SCOPE.md

Validation:
└── reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
```

### Implementation Phase (Radio Silence)

```
Primary References:
├── reference/REFERENCE-EXISTING-PATTERNS.md (code style)
├── reference/REFERENCE-CUSTOM-CONTROLS.md (control APIs)
├── reference/REFERENCE-SETTINGS-INVENTORY.md (setting keys)
└── reference/REFERENCE-VISUAL-API-SCOPE.md (API contracts)

Continuous Validation:
└── reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
```

### Validation Phase

```
Validation Files:
├── reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
├── reference/REFERENCE-CUSTOM-CONTROLS.md
├── reference/REFERENCE-SETTINGS-INVENTORY.md
└── reference/REFERENCE-VISUAL-API-SCOPE.md
```

---

## File Sizes and Complexity

```
📋 RESTART-GUIDE.md
│  Size: 400 lines
│  Complexity: ⭐⭐ Medium
│  Read Time: 10 min
│
📄 RESTART-PROMPT.md
│  Size: 900 lines
│  Complexity: ⭐⭐⭐ High
│  Read Time: 15 min (but consumed by spec-kit)
│
📁 reference/
│
├── 📖 README.md
│   │  Size: 300 lines
│   │  Complexity: ⭐ Low
│   │  Read Time: 5 min
│   │
├── 💬 REFERENCE-CLARIFICATIONS.md
│   │  Size: 200 lines
│   │  Complexity: ⭐ Low
│   │  Read Time: 5 min
│   │
├── 🎨 REFERENCE-EXISTING-PATTERNS.md
│   │  Size: 400 lines
│   │  Complexity: ⭐⭐ Medium
│   │  Read Time: 10 min
│   │
├── 🧩 REFERENCE-CUSTOM-CONTROLS.md
│   │  Size: 450 lines
│   │  Complexity: ⭐⭐ Medium
│   │  Read Time: 12 min
│   │
├── ⚙️ REFERENCE-SETTINGS-INVENTORY.md
│   │  Size: 400 lines
│   │  Complexity: ⭐⭐⭐ High
│   │  Read Time: 15 min
│   │
├── ✅ REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
│   │  Size: 550 lines
│   │  Complexity: ⭐⭐⭐ High
│   │  Read Time: 20 min
│   │
└── 🔌 REFERENCE-VISUAL-API-SCOPE.md
    │  Size: 500 lines
    │  Complexity: ⭐⭐⭐ High
    │  Read Time: 18 min
```

**Total**: ~4400 lines, ~110 minutes total read time

---

## Common Navigation Paths

### "I want to understand the overall scope"

```
RESTART-GUIDE.md
└── Scope Summary section
```

### "I want to see what decisions were made"

```
reference/REFERENCE-CLARIFICATIONS.md
└── All 21 Q&A pairs
```

### "I want to know what controls to create"

```
reference/REFERENCE-CUSTOM-CONTROLS.md
└── Controls to Extract (5)
└── New Controls to Create (5)
```

### "I want to know what settings to implement"

```
reference/REFERENCE-SETTINGS-INVENTORY.md
└── Settings Categories (8)
    └── 60+ settings documented
```

### "I want to verify constitutional compliance"

```
reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
└── Principle I-XI checklists
```

### "I want to understand Visual ERP integration"

```
reference/REFERENCE-VISUAL-API-SCOPE.md
└── Data Entities (3)
    ├── Items
    ├── Work Orders
    └── Inventory Transactions
```

### "I want to see code examples"

```
reference/REFERENCE-EXISTING-PATTERNS.md
└── MVVM Patterns
└── XAML Patterns
└── Database Patterns
└── Testing Patterns
```

---

## Quick Action Commands

### Generate Specification

```bash
/speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md
```

### Validate Implementation

```powershell
# Build
dotnet build MTM_Template_Application.sln

# Test
dotnet test MTM_Template_Application.sln

# Validate
.\.specify\scripts\powershell\validate-implementation.ps1

# Constitutional audit
.\.specify\scripts\powershell\constitutional-audit.ps1
```

---

## Next Steps

1. ✅ Read `RESTART-GUIDE.md` (you are here)
2. ⏳ Read `reference/README.md` for reference overview
3. ⏳ Skim other references based on need
4. ⏳ Run `/speckit.specify` command
5. ⏳ Review generated spec, plan, and tasks
6. ⏳ Approve for implementation

---

## Support

If you get lost:

1. Return to this `NAVIGATION.md` file
2. Check `FILE-SUMMARY.md` for overview
3. Use `reference/README.md` for reference guidance
4. Refer to `RESTART-GUIDE.md` for workflow

**Created**: October 8, 2025

**Feature**: 005-migrate-infor-visual (Restart)
