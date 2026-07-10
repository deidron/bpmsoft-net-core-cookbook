# Dev container

A reproducible Linux dev environment that matches CI — same .NET SDK (`global.json`)
and the same linters. Open the repo in **VS Code** (Dev Containers extension) or
**GitHub Codespaces**.

> ⚠️ **Before "Reopen in Container":** create `.devcontainer/devcontainer.env`
> (see [Build / test](#build--test)). The container is launched with `--env-file`,
> so it **will not start** until that file exists.

## What's inside

- **.NET 8 SDK** (base image `mcr.microsoft.com/devcontainers/dotnet`)
- **gh** (GitHub CLI), **shellcheck**, **actionlint** `1.7.12`, **yq** `4.53.3` — pinned to CI
- VS Code extensions: C#, YAML, shellcheck, GitHub Actions

## Lint

The tools are on `PATH` natively — no Docker needed inside the container:

```bash
actionlint -color
find .github -name '*.sh' -print0 | xargs -0 shellcheck --shell=bash
```

(From the **host** you can instead run the Docker-based `scripts/lint.sh`.)

## Build / test

`REMOTE_CONTAINERS=true` (set automatically by Dev Containers), so `CoreLibPath`
resolves to `.platform-libs` (CI mode: `netstandard2.0`, builds on Linux). The
platform DLLs are **not** in the repo — they're fetched from the private libs repo.

### 1. Create `.devcontainer/devcontainer.env`

**Required — the container is launched with `--env-file`, so it won't start without
this file.** Copy the template and fill it in:

```bash
cp .devcontainer/devcontainer.env.example .devcontainer/devcontainer.env
```

- `GH_PAT` — a token with **read** access to the private libs repo.
- `PLATFORM_LIBS_REPO` — that repo's `owner/name`.

Gitignored — never commit the token. (In Codespaces, create this file there too.)

### 2. Fetch the libs

Runs **automatically on container create** (`postCreateCommand`) from `devcontainer.env`.
To (re)run manually:

```bash
bash scripts/fetch-platform-libs.sh
```

### 3. Build

```bash
bash .github/scripts/build.sh <Package>
```

Linting needs no token; only the full build does.

## Troubleshooting

**`ResolvePackageAssets` fails with `C:\Program Files (x86)\...\NuGetPackages`** — the C# server
is reading `obj/` that was restored on **Windows** (the workspace is bind-mounted, so Windows build
artifacts leak in). This cleanup runs automatically on every container start (`postStartCommand`);
to force it now and re-restore on Linux:

```bash
find . -type d \( -name obj -o -name bin \) -prune -exec rm -rf {} +
dotnet restore <Package>/<Package>.csproj
```

Then run **Developer: Reload Window**. (`obj/`/`bin/` are disposable build output — gitignored.)

**Platform types (`BPMSoft.*`) show as unresolved** — fetch the platform DLLs into `.platform-libs/`
first (see Build / test above). The project still loads without them; only those references are red.
