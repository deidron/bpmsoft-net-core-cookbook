# .dev-build

Build hooks for the **configuration Dev csproj** (`BPMSoft.Configuration.Dev.csproj`),
kept out of that vendor file. Not a package: no `descriptor.json`, never packed or deployed.

## How it is picked up

The Dev csproj imports every `Pkg\**\Directory.Build.targets`, so it finds
`Directory.Build.targets` here. Nothing else does: packages and the schema project look for
`Directory.Build.*` only in their own parent folders. The targets also check
`AssemblyName` ends with `.Configuration` and skip CI (`GITHUB_ACTIONS`).

## What it does

It keeps `.configuration-refs/` (repo root, gitignored) equal to what the configuration
assembly references. `build-schema.sh` compiles schemas against that folder.

| Target | Runs | Copies |
|---|---|---|
| `AutoCollectAllRealDependencies` | after `ResolveAssemblyReferences` | every reference except .NET reference assemblies (the SDK supplies those) |
| `CollectConfigurationAssembly` | after `Build` | the configuration reference assembly (`obj\<Configuration>\ref\...`) |

Only changed files are copied. Files no longer referenced are removed, but only from a
folder the targets created: it holds `.configuration-refs.marker`. A folder with DLLs and no
marker (e.g. `ConfigurationRefsPath` pointed at the site root by mistake) is never cleaned.

## Normal build

Any configuration build refreshes the folder: Visual Studio, `dotnet build` of the Dev
csproj, or compiling the configuration in the application (it builds the same csproj).
Look for `[AutoCopy]` lines in the output or in `Build.log`.

## Refresh without building

From the folder with the Dev csproj:

```bash
dotnet msbuild BPMSoft.Configuration.Dev.csproj "-t:AutoCollectAllRealDependencies;CollectConfigurationAssembly" -nologo -v:m
```

The second target copies the reference assembly of the last configuration build; if there
has been none, it is skipped.

## Settings

- `ConfigurationRefsPath` — target folder; defaults to `.configuration-refs` in the repo root.
  Override with `-p:` or an environment variable.
