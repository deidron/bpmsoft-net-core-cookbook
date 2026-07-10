# Linting

CI runs two linters in the **Lint** job ([`ci-orchestrator.yml`](../.github/workflows/ci-orchestrator.yml)),
gated by the aggregate `CI Success` check:

| Linter | Lints | Catches |
|--------|-------|---------|
| **actionlint** | `.github/workflows/*.yml` | invalid workflow syntax, undefined `secrets`/`vars`/`inputs`/`needs`, bad expressions, deprecated runner labels; also shellchecks **inline `run:`** blocks |
| **shellcheck** | `.github/**/*.sh` | quoting bugs (word-splitting/globbing), unsafe patterns, portability issues — each with an `SC####` code |

> Coverage gap: neither tool lints `run:` blocks inside composite **`action.yml`** files. Keep real
> shell logic in `.sh` scripts (covered by shellcheck) rather than inline in `action.yml`. The local
> `scripts/lint.sh` additionally **parse-checks** every `action.yml` (so a broken composite action is
> caught locally), but it does not analyse the shell inside them.

## Run locally before pushing

Catches issues in seconds instead of a CI round-trip. Run from the **repo root**. Requires Docker.

### Quick — one command

Runs both linters (works on Linux/macOS and Windows Git Bash):

```bash
bash scripts/lint.sh
```

### Docker (manual)

```bash
# actionlint — reads .github/actionlint.yaml automatically
docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:1.7.12 -color

# shellcheck — the -alpine variant ships a shell, so find|xargs works
docker run --rm -v "$PWD:/mnt" -w /mnt koalaman/shellcheck-alpine:stable \
  sh -c "find .github -name '*.sh' -print0 | xargs -0 shellcheck --shell=bash"
```

On **Windows PowerShell** (use `${PWD}`):

```powershell
docker run --rm -v "${PWD}:/repo" -w /repo rhysd/actionlint:1.7.12 -color
docker run --rm -v "${PWD}:/mnt" -w /mnt koalaman/shellcheck-alpine:stable sh -c "find .github -name '*.sh' -print0 | xargs -0 shellcheck --shell=bash"
```

Exit code `0` = clean (check with `$LASTEXITCODE` in PowerShell, `$?` in bash).

### Native install (optional, faster)

- macOS: `brew install shellcheck actionlint`
- Linux: `apt-get install shellcheck` (actionlint: see its releases)
- Windows: `scoop install shellcheck actionlint`

```bash
actionlint -color
find .github -name '*.sh' -print0 | xargs -0 shellcheck --shell=bash
```

`--shell=bash` is used because the scripts are bash and invoked via `bash x.sh` (so they need no shebang
to satisfy shellcheck).

## Suppressing a finding

- **shellcheck:** add `# shellcheck disable=SC2086` on the line above, or use `.shellcheckrc`.
- **actionlint:** scope it in [`.github/actionlint.yaml`](../.github/actionlint.yaml) under
  `paths.<glob>.ignore` (as done for `job-deploy.yml`'s environment secrets, which actionlint can't see).
  Prefer a scoped ignore over a global one.
