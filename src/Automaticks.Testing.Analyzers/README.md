# Automaticks.Testing.Analyzers

Roslyn analyzers for test project conventions — naming, forbidden APIs, and test structure.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.Testing.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXTST001` | Mocking frameworks are not allowed | Testing | Error | Analyzer |
| `ATXTST002` | Test class name must match the class under test | Testing | Warning | Analyzer |
| `ATXTST003` | Test method name must follow the three-part convention | Testing | Warning | Analyzer |
| `ATXTST004` | Task.Delay without TimeProvider is not allowed | Testing | Error | Analyzer |
| `ATXTST005` | Boolean decision needs too many test cases | Testing | Warning | Analyzer |
| `ATXTST006` | Bitmask test must use a single-bit mask | Testing | Warning | Analyzer |
| `ATXTST007` | Default label on an exhaustive enum switch blocks full branch coverage | Testing | Warning | Analyzer |
| `ATXTST008` | Conditional compilation on DEBUG hides the shipped code path from tests | Testing | Warning | Analyzer |
| `ATXTST009` | Debug.Assert condition must not perform side effects | Testing | Error | Analyzer |
| `ATXTST010` | Ambient dependency must be reached through an injectable seam | Testing | Warning | Analyzer |
| `ATXTST011` | Test method must contain an assertion | Testing | Warning | Analyzer |
| `ATXTST012` | Public member must be covered by a test | Testing | Warning | Analyzer |
| `ATXTST013` | File line coverage must meet the configured minimum | Testing | Warning | Analyzer |
| `ATXTST014` | TimeProvider.System is not allowed in tests | Testing | Warning | Analyzer |
| `ATXTST015` | Method branch coverage must meet the configured minimum (opt-in) | Testing | Warning (disabled by default) | Analyzer |
| `ATXTST016` | Supplied coverage report must be usable | Testing | Warning | Analyzer |

## Coverage rules

`ATXTST012`, `ATXTST013`, and `ATXTST015` read a Cobertura report produced by a previous test run.
A Roslyn analyzer cannot measure coverage itself, and it is not allowed to read the file system, so
the report must be handed to the compiler as an `AdditionalFiles` item.

```shell
dotnet test --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
dotnet build -p:AutomaticksCoverageReport=/abs/path/to/coverage.cobertura.xml
```

The package ships MSBuild targets that turn `AutomaticksCoverageReport` into the required item.
`AutomaticksCoverageReport` accepts a single path or a wildcard; a solution-wide test run emits one
report per test project and the analyzers merge them. Use an **absolute** path for a wildcard —
MSBuild resolves a glob relative to each project directory, so a repo-relative glob silently matches
nothing and the rules would appear to pass. All three rules stay silent when no report is supplied,
so a clean clone still builds before any test run has happened.

Set the `ATXTST013` and `ATXTST015` thresholds in `.editorconfig`:

```ini
[*.cs]
automaticks.minimum_line_coverage = 80
automaticks.minimum_branch_coverage = 80
```

`ATXTST015` is **disabled by default**. A branch-coverage bar is far more project-specific than a
line-coverage one — most mature codebases sit well below 80% branch coverage even when line coverage
is healthy — so enabling it for everyone on upgrade would break far more builds than it helps. Turn
it on deliberately once your suite is ready:

```ini
[*.cs]
dotnet_diagnostic.ATXTST015.severity = warning
```

Because the report comes from an earlier run, it can be stale relative to edited source. `ATXTST012`
therefore matches on method name rather than line number, and reports only members the report
positively shows as unexecuted — members it does not mention at all are skipped.

`ATXTST016` guards against the opposite failure: a report that *was* supplied but is empty,
unparseable, or contains no file entries. Without it, a malformed report would make `ATXTST012`,
`ATXTST013`, and `ATXTST015` pass vacuously — checking nothing while looking like a clean run.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
