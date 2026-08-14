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
| `ATXDC018` | #pragma warning disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix |
| `ATXDC019` | // ReSharper disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix |
| `ATXDC056` | [SuppressMessage] is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix |

## Code fixes

Every rule in this package ships a code fix. Apply them from the IDE light bulb, or in bulk:

```shell
dotnet format analyzers --diagnostics ATXDC056 --severity error
```

| ID | Fix |
|---|---|
| `ATXDC018` | Remove the `#pragma warning disable` directive |
| `ATXDC019` | Remove the `// ReSharper disable` comment |
| `ATXDC056` | Remove the `[SuppressMessage]` attribute |

Each fix deletes the suppression only. The diagnostic it was hiding will surface on the next
build, which is the point — fix that root cause rather than re-suppressing it.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
