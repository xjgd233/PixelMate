---
name: pixelmate-product-hardening
description: Continue PixelMate's reliability, usability, and release work from a prioritized backlog. Use when the user asks to continue PixelMate product hardening, pick up the next planned item, or implement the audit recommendations.
---

# PixelMate Product Hardening

This skill is the compact execution layer for the product audit completed on
2026-11-05. It does not replace the project's engineering rules.

## Start cheaply

1. If this is the first action in the turn, run:

   ```sh
   git status --short
   git log -1 --oneline
   ```

2. Read `docs/phase-status.md` before changing code, as required by the project.
   Also follow `.agents/skills/pixelmate-engineer/SKILL.md`; read it only if it
   has not already been loaded in the current turn.
3. Do not reread the full README, phase reports, Git history, or source tree.
   Read [references/backlog.md](references/backlog.md) only when selecting the
   next item or when the user names an item whose acceptance criteria are needed.

## Select exactly one item

- If the user names an item, work only on that item.
- If the user says “continue”, “next”, or asks for the backlog, choose the first
  unchecked `P0` item unless its dependency or project phase gate blocks it.
- Before editing, state the selected item in one sentence.
- Stop after the selected item is implemented and verified. Do not silently
  continue into the next item.
- If the work crosses the project's phase gate, obtain the user's direction
  before doing it and follow the existing project status rules.

To inspect the backlog without loading all of it:

```sh
rg -n '^## |^- \[[ x]\]' .agents/skills/pixelmate-product-hardening/references/backlog.md
```

## Keep context and work small

- Locate code with `rg` using the symbols and filenames named in the backlog.
- Open only the relevant method, nearby tests, and the minimum XAML needed.
- Reuse existing controls, helpers, styles, and test patterns. Do not introduce
  a framework, service, database, or abstraction unless the item requires it.
- Fix the requested flow first. Note adjacent improvements instead of folding
  them into the same change.
- For source changes, run the smallest relevant test class first, then one full
  `dotnet test PixelMate.slnx -c Release --no-restore`. Do not repeat unchanged
  builds or tests.
- For documentation or asset-only changes, run `git diff --check`; do not run
  the .NET test suite.
- Do not recreate the README screenshot or redo competitor research. Both were
  completed on 2026-11-05.

## Completion response

Report only:

- what changed and why;
- files changed;
- verification actually run;
- the next unchecked backlog item.

Use a full phase report only when the project's phase process requires one.

## Product boundaries

Preserve the local-first, lightweight, pixel-accurate desktop-tool identity.
Do not add OCR, long screenshots, screen recording, cloud upload, accounts, or
large editing-suite features unless the user explicitly changes the scope.
