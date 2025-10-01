# Guide: /specify — Write the Feature Spec

What it does
- Turns your idea into a clear spec with user stories and acceptance criteria.

When to use it
- After you set your project principles using /constitution.

How to run (example prompt)
```text
/specify Build an application that helps me organize photos into albums by date, and drag and drop albums on the main page. No uploads; store metadata locally.
```

What to include
- Who is the user?
- What problem are we solving?
- What must be true for “done” (acceptance criteria)?
- Edge cases (errors, empty states)

What you get
- A new branch (like 001-feature-name)
- A spec folder with spec.md inside (user stories + requirements)

Tips
- Focus on “what” and “why,” not tech stack (that comes in /plan).
- Use simple, testable sentences for acceptance criteria.