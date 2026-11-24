---
description: Fix build errors from auto-built-errors.log, rebuild until clean
argument-hint: "[project folder, default 3385996759]"
---
Project folder: $ARGUMENTS (if empty, `3385996759`; in a mod repository, the projects of `scripts/settings.json` "projects").

1. Read `<project>/scripts/auto-built-errors.log`. If it does not exist, run `<project>/scripts/rebuild-project.ps1` to get the current errors.
2. If there are compilation errors, fix the source code to resolve them (ModLib sources: `ModLib/`).
3. If there are no compilation errors, review the warnings and suggest source code improvements based on them.
4. After fixing, run `powershell -NoProfile -ExecutionPolicy Bypass -File <project>/scripts/rebuild-and-deploy.ps1` to verify all errors are resolved.
5. If errors remain, repeat until there are none. Many "missing type/member" errors after a game update: stale `MelonLoader\Managed` (`scripts/setup/04-verify-environment.ps1`), not code bugs.
