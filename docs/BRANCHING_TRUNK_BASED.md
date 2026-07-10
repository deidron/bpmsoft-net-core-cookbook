# Branching model — trunk-based (`main` only, GitHub Flow)

This repo uses a single long-lived branch, **`main`**. Everything else is a short-lived branch
that merges back into `main` via PR. Releases are `v*` tags on a `main` commit — there is no
release branch and no integration (`dev`) branch, so none of the `dev`↔`main` synchronization
friction (fast-forward resyncs, "main is one commit ahead", back-merges) exists.

```
feature/* ──PR──▶ main ──tag──▶ vX.Y.Z
                   │              │
              development      production
            (deploy on merge)  (gated deploy on tag)
```

- **`main`** — the only permanent branch; always releasable, protected (see [Protection](#protection)).
- **`feature/*`, `fix/*`, `chore/*`, …** — short-lived, branched off `main`, deleted on merge
  (naming enforced by [`branch-naming.json`](../.github/rulesets/branch-naming.json)).

## Daily flow

```bash
git fetch origin
git switch -c feature/x origin/main       # branch off main
# ...work, commits...
git push -u origin feature/x
# open PR  base = main  →  CI (build, actionlint, shellcheck)  →  squash-merge  →  branch auto-deleted
```

Merges into `main` are **squash**, so history stays linear — one PR = one commit.

## Releases

A release is a tag on a `main` commit; there is no `dev → main` PR and no merge bubble.

```bash
git tag v1.9.2 <main-sha>     # version from $(Version) in the .props
git push origin v1.9.2        # then cut a GitHub Release
```

## Hotfixes

Same path as any change: branch off `main` → PR → squash-merge → tag if it warrants a release.
No special branch, no back-merge.

## Protection

`main` and the release tags are guarded by rulesets (definitions and import steps in
[`.github/rulesets/README.md`](../.github/rulesets/README.md)):

| Ruleset | Target | Enforces |
|---------|--------|----------|
| `main-protection` | `refs/heads/main` | PR required, no bypass, linear history, required CI, no force-push |
| `tag-protection` | `refs/tags/v*` | release tags cannot be deleted/moved/overwritten |
| `branch-naming` | `~ALL` minus allowed prefixes | `feature/ fix/ hotfix/ chore/ docs/ refactor/` (+ `dependabot/`) |

`main-protection` also limits PR merges to **squash only** (`allowed_merge_methods: ["squash"]`).
Beyond the rulesets, the repository is configured (GitHub repo settings, not config-as-code) to
disable the merge-commit/rebase buttons and to **auto-delete head branches** — together this keeps
`main` linear and feature branches short-lived.

## Deploy mapping

Deploy targets depend on the trigger, not the branch:

- **`development`** ← every merge to `main`, once CI is green
  ([`ci.yml`](../.github/workflows/ci.yml) `deploy-dev` job, `github.ref_name == 'main'`).
- **`production`** ← a `v*` release tag, gated by required reviewers
  ([`deploy-prod.yml`](../.github/workflows/deploy-prod.yml)).

## Guardrails

- ❌ No direct push to `main` — everything goes through a PR.
- ❌ No long-lived `dev` branch — it would reintroduce the `dev`↔`main` sync problem with no benefit.

> Trade-off to be aware of: `development` reflects `main` *after* merge, so there is no staging of
> "merged but not yet on `main`"; and releases rely on tag discipline rather than a visible release
> branch.
