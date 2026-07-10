# Repository Rulesets

Ready-made rulesets in the [github/ruleset-recipes](https://github.com/github/ruleset-recipes) format.
These are not active settings — they must be imported into the repository manually.

> ⚠️ **These files are the source of truth, but GitHub does NOT apply them from the repo.** Editing a
> JSON here changes nothing until you re-import it (see [How to import](#how-to-import)). After any
> change, re-import the affected ruleset so the active rule and this file don't drift apart.

## What's inside

| File | Target | What it does |
|------|--------|--------------|
| [`main-protection.json`](main-protection.json) | `main` branch (default) | PR required (no approval needed, no bypass), thread resolution, linear history, no force-push/deletion, required CI checks (strict) |
| [`tag-protection.json`](tag-protection.json) | `v*` tags | No deletion, moving, or overwriting of release tags |
| [`branch-naming.json`](branch-naming.json) | all branches | Blocks creating branches with non-conforming names (allowed: `main`, `feature/**`, `fix/**`, `hotfix/**`, `chore/**`, `docs/**`, `refactor/**`, `dependabot/**`) |

This repo uses the **trunk-based** model — a single long-lived `main` branch
(see [docs/BRANCHING_TRUNK_BASED.md](../../docs/BRANCHING_TRUNK_BASED.md)), so there is one
branch ruleset, not two. There is no `dev-protection` and no `~DEFAULT_BRANCH` trap.

## Bypass

Branch protection on `main` has **no bypass** (`bypass_actors: []`) — direct push
is blocked for everyone, including the owner; all changes go through a PR. The `tag-protection`
and `branch-naming` sets still let the **Repository admin** role (`actor_id: 5`) bypass
(`bypass_mode: always`). Adjust `bypass_actors` per set to taste.

## Required status checks

CI is composed of nested reusable workflows with matrix jobs, so most check names are
dynamic (`Run orchestrator / Build & Test / <package>` — one per package) and can't be
hard-coded. Instead, the orchestrator exposes a single stable aggregate gate:

- **`Run orchestrator / CI Success`** — a job that `needs` all the others and fails only if one of them
  actually failed (it is skipped on cancellation, so `cancel-in-progress` stays clean). This is
  the only required status check in `main` protection.

The check appears in the **Add checks** picker only after CI has run at least once with this job.
Until then the imported string stands as-is and starts matching on the first run.

## How to import

### Via UI
**Settings → Rules → Rulesets → New ruleset → Import a ruleset** → select the JSON file.

### Via gh CLI
```bash
gh api repos/deidron/bpmsoft-net-core-cookbook/rulesets \
  --method POST -H "Accept: application/vnd.github+json" \
  --input .github/rulesets/main-protection.json

gh api repos/deidron/bpmsoft-net-core-cookbook/rulesets \
  --method POST -H "Accept: application/vnd.github+json" \
  --input .github/rulesets/tag-protection.json

gh api repos/deidron/bpmsoft-net-core-cookbook/rulesets \
  --method POST -H "Accept: application/vnd.github+json" \
  --input .github/rulesets/branch-naming.json
```

> Tip: import with `"enforcement": "evaluate"` first (evaluation mode without blocking),
> check the **Insights → Rule Insights** tab to confirm nothing breaks, then switch to `"active"`.
