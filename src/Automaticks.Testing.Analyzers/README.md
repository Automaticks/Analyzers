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

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
