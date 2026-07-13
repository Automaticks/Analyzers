# Automaticks.Diagnostics.CodeAnalysis.Analyzers

Roslyn analyzers for `System.Diagnostics.CodeAnalysis` — diagnostic suppression rules.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.Diagnostics.CodeAnalysis.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXDC018` | #pragma warning disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer |
| `ATXDC019` | // ReSharper disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer |
| `ATXDC056` | [SuppressMessage] is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer |

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
