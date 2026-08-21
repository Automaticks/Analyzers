# Copilot instructions

Roslyn analyzer packages. Every analyzer in `src/` runs against this repository, with
`TreatWarningsAsErrors=true`, so a rule you add governs the code you write.

## The dogfood cache will mislead you

`.dogfood/` holds copies of the built analyzer DLLs so each project can load its siblings. **It is
one shared directory, not branch-aware, and survives `git checkout`.** So analyzers built on another
branch keep firing here, and the first build after a clean can fail with `CS8034 ... being used by
another process` when the copy target races `csc`.

**Purge it after switching branches, and re-run a build before believing a failure.**

```powershell
Remove-Item .dogfood -Recurse -Force
dotnet build          # first pass repopulates the cache
dotnet build          # second pass is the trustworthy one
```

CI is unaffected — every run starts from a clean checkout. Two concurrent `dotnet build` invocations
race on that same cache, so never chain two builds in one command; run them sequentially.

`dotnet test` needs `--project <path-to-.csproj>`; passing a directory errors out.

## Coverage gates

`ATXTST012` (public member never executed), `ATXTST013` and `ATXTST017` (file line and branch
coverage), `ATXTST015` (method branch coverage) and `ATXTST016` (report unusable) read a Cobertura
report from a previous test run. They stay silent when no report is supplied.

This repository requires **100% line and 100% branch coverage** (`.editorconfig`). A defensive
branch no test can reach is not an exemption — make it reachable or delete it.

```powershell
dotnet build
dotnet test --no-build --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
$root = (Get-Location).Path
dotnet build --no-restore "-p:AutomaticksCoverageReport=$root/tests/**/TestResults/coverage.cobertura.xml"
```

The report path **must be absolute**. MSBuild resolves a wildcard relative to each project
directory, so a repo-relative glob matches nothing and the gate passes checking nothing.

A package never dogfoods itself, so the coverage rules do not run on
`Automaticks.Testing.Analyzers`. Merge reports by hand when changing it.

## Correctness beats compatibility

Breaking consumers is an acceptable price for a correct rule. Never soften a rule, ship it disabled,
or leave a defect standing because upgrading costs downstream work. Say what breaks, then change it
anyway.

## Conventions that break the build

Read a neighbouring analyzer and its tests before writing anything. The ones that bite most:

- Member order (`ATXCS042`/`ATXCS064`): constants → fields → properties → constructors → overrides →
  methods; alphabetical within each group, public before private. This is the single most common
  build failure.
- No `using System.Linq` (`ATXLQ002`) — use explicit loops.
- No tuples (`ATXCS012`), no `ref` parameters, no inline `new` as an argument (`ATXCS058`).
- Bool **methods** start with `Can`/`Has`; bool **properties** start with `Is`/`Allow`.
- No namespace-qualified type references (`ATXCS072`), no `global::` (`ATXCS073`), no using aliases
  (`ATXCS074`), and no expression bodies on any member (`ATXCS075`-`ATXCS083`, one rule per kind).
- XML `<summary>` on every type and public member; never `<remarks>`; one short line per tag.
- Max 20 `.cs` files per folder and per namespace; 50 lines per method; 500 lines per class.

Files are UTF-8. Prefer the editing tools over PowerShell writes so encoding is preserved.

## Let the code fixes do the work

Most rules ship a fix, and `Directory.Build.targets` loads the `*.CodeFixes.dll` assemblies so they
work here too. Mechanical breaks like member order are cheapest to fix this way:

```powershell
dotnet format analyzers Analyzers.slnx --diagnostics ATXCS064 --severity error --include <file>
```

Build first: fixes come from `.dogfood`, so an edit is invisible until the cache refreshes.
