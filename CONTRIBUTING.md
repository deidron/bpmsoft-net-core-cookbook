# Contributing Guide

## Branching

Trunk-based: branch off `main`, open a PR back into `main` (squash, linear history). No direct
push; CI must pass; no approval required. Releases are `v*` tags on a `main` commit.
Full rationale and the deploy mapping: [docs/BRANCHING_TRUNK_BASED.md](docs/BRANCHING_TRUNK_BASED.md).

## Branch naming

Working branches are `<prefix>/<short-description>` in kebab-case (lowercase, digits, `-`):

| Prefix | Purpose |
|--------|---------|
| `feature/` | new functionality / new package |
| `fix/` | bug fix |
| `hotfix/` | urgent fix |
| `chore/` | infrastructure, dependencies, chores |
| `docs/` | documentation only |
| `refactor/` | refactoring with no behavior change |

**Enforced** by the [`branch-naming.json`](.github/rulesets/branch-naming.json) ruleset — a branch
with a non-conforming name cannot be created (only `main` is exempt).

## Adding a package

1. Top-level folder named after the package (PascalCase).
2. Add a `descriptor.json` — without it CI won't see the package.
3. For a code package, add a `*.csproj` (+ `*.sln` to wire in tests under `tests/`).

`detect.sh` picks it up automatically — no manual registration.

## Pull Request checklist

- [ ] Formatting passes — `bash .github/scripts/format.sh <Package>` (or `format-schema.sh` for schema packages).
- [ ] Build and tests green — `bash .github/scripts/build.sh <Package>`.
- [ ] Workflows / shell scripts linted if touched — see [docs/LINTING.md](docs/LINTING.md).
- [ ] `descriptor.json` reflects any new package dependencies / metadata changes.
- [ ] No build artifacts (`bin/`, `obj/`, `.vs/`) or platform DLLs (`.platform-libs/`) committed.

Code style is enforced via [`.editorconfig`](.editorconfig) and checked in CI.
