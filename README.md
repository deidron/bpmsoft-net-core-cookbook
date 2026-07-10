# BPMSoft .NET Core Cookbook

A monorepo of example packages for the **BPMSoft** platform (.NET, target SDK 8.0,
built in CI against `netstandard2.0`). Every top-level folder containing a `descriptor.json`
file is a standalone package. CI discovers packages automatically — no manual registration needed.

## Structure

```
.
├── <Package>/              # a BPMSoft package (has descriptor.json)
│   ├── descriptor.json     #   package manifest
│   ├── *.csproj / *.sln    #   optional — code project (then built and tested)
│   ├── Files/              #   src, schemas, resources
│   └── tests/              #   optional — unit tests
├── global.json             # pins the .NET SDK version
├── platform.json           # selects the BPMSoft platform version/edition for the libs fetch
├── .editorconfig           # code style (enforced in CI)
└── .github/
    ├── workflows/          # CI/CD (see below)
    ├── scripts/            # detect / format / build / pack / deploy
    ├── actions/            # composite actions (setup env, restore platform libs)
    ├── templates/          # schema-package.csproj template
    └── rulesets/           # branch/tag protection rulesets (imported manually)
```

A package is treated as a **code project** if it has a `*.csproj` in its root — such a package
is additionally built (`dotnet build`) and tested. Packages without a `.csproj` (schema-only)
are merely formatted and packed.

## CI/CD

Runs on push and PR to `main` ([`ci.yml`](.github/workflows/ci.yml) → orchestrator):

| Stage | What it does |
|-------|--------------|
| **Detect** | scans the root, builds the list of packages |
| **Format** | format check for code projects (`dotnet format`) |
| **Format schemas** | format check for schema-only packages |
| **Build & Test** | `dotnet build` + `dotnet test` (Release) for each code project |
| **Pack** | `ubs zip` — produces `.gz` package artifacts |
| **Deploy (development)** | after each merge to `main`, once CI is green ([`ci.yml`](.github/workflows/ci.yml)) |

This repo is **trunk-based** (`main` only); deploy targets depend on the trigger:

| Trigger | GitHub Environment |
|---------|--------------------|
| merge to `main` (CI green) | `development` |
| `v*` release tag ([`deploy-prod.yml`](.github/workflows/deploy-prod.yml), gated) | `production` |

> **Deploy is currently a stub:** [`deploy.sh`](.github/scripts/deploy.sh) prints `[STUB] ubs install ...`,
> the real install is commented out. To enable real deployment, uncomment `ubs install`
> and set the `BPMSOFT_URL` / `BPMSOFT_USER` / `BPMSOFT_PASSWORD` secrets in the relevant environment.

> Platform assemblies are fetched at CI time into `.platform-libs/` from the private libs
> repo (gitignored, never committed); selection is driven by `platform.json`. Locally,
> point `CoreLibPath` at your own BPMSoft install via `local.props`.

## Branches and contributing

Trunk-based: `feature/* → main`, releases are `v*` tags. See [CONTRIBUTING.md](CONTRIBUTING.md)
and [docs/BRANCHING_TRUNK_BASED.md](docs/BRANCHING_TRUNK_BASED.md) for details.
Branches and tags are protected by rulesets — see [.github/rulesets/README.md](.github/rulesets/README.md).
