# Copilot instructions

Roslyn analyzer packages. Every analyzer in `src/` runs against this repository itself, with
`TreatWarningsAsErrors=true`, so a rule you add immediately governs the code you write.

## The dogfood cache will mislead you

`.dogfood/` holds copies of the built analyzer DLLs so each project can load its siblings.
**It is a single shared directory that is not branch-aware and survives `git checkout`.**

Consequences, both observed:

- Analyzers built on one branch keep applying after you switch. A rule that exists only on another
  branch will fire here, on source that has no such rule — and "fixing" those hits is wasted work.
- The first build after a clean can fail with `CS8034 ... being used by another process` or
  `MSB3026`, because the cache-copy target races with running `csc` processes.

So: **purge it after switching branches, and re-run a build before believing a failure.**

```powershell
Remove-Item .dogfood -Recurse -Force
dotnet build          # first pass repopulates the cache
dotnet build          # second pass is the trustworthy one
```

CI is unaffected — every run starts from a clean checkout.

## Run one build at a time

Two concurrent `dotnet build` invocations race on that same cache. Never chain two builds in one
command just to grep different things; run them sequentially.

`dotnet test` needs `--project <path-to-.csproj>`; passing a directory errors out.

## Coverage gates

`ATXTST012` (public member never executed), `ATXTST013` (file line coverage), `ATXTST015` (method
branch coverage) and `ATXTST016` (report unusable) read a Cobertura report from a previous test run.
They stay silent when no report is supplied, so a clean clone still builds.

This repository requires **100% line and 100% branch coverage** (`.editorconfig`). A defensive
branch no test can reach is not an exemption — either make it reachable or delete it.

```powershell
dotnet build
dotnet test --no-build --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
$root = (Get-Location).Path
dotnet build --no-restore "-p:AutomaticksCoverageReport=$root/tests/**/TestResults/coverage.cobertura.xml"
```

The report path **must be absolute**. MSBuild resolves a wildcard relative to each project
directory, so a repo-relative glob silently matches nothing and the gate passes while checking
nothing at all.

## Conventions that break the build

Read a neighbouring analyzer and its tests before writing anything. The ones that bite most:

- Member order (`ATXCS042`/`ATXCS064`): constants → fields → properties → constructors → overrides →
  methods; alphabetical within each group, public before private. This is the single most common
  build failure.
- No `using System.Linq` (`ATXLQ002`) — use explicit loops.
- No tuples (`ATXCS012`), no `ref` parameters, no inline `new` as an argument (`ATXCS058`).
- Bool **methods** start with `Can`/`Has`; bool **properties** start with `Is`/`Allow`.
- No namespace-qualified type references (`ATXCS072`) and no expression-bodied methods (`ATXCS075`).
- XML `<summary>` on every type and public member; never `<remarks>`; one short line per tag.
- Max 20 `.cs` files per folder and per namespace; 50 lines per method; 500 lines per class.

Files are UTF-8. Prefer the editing tools over PowerShell writes so encoding is preserved.
